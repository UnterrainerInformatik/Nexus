// ***************************************************************************
// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
// 
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to <http://unlicense.org>
// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using NexusClient.Converters;
using NexusClient.Nexus;
using NexusClient.Nexus.Apis;

namespace NexusClient.HandlerGroups
{
	internal struct HandlerStoreItem
	{
		public Delegate HandlerDelegate { get; set; }
		public Type Type { get; set; }

		public Type GenericType { get; set; }
		public MethodInfo GenericReadMessageMethod { get; set; }
	}

	public abstract class HandlerGroup<TCnv, TSer, TDes, TDto> where TCnv : IConverter<TDto>
		where TSer : IMessageSer<TDto>
		where TDes : IMessageDes<TDto>
		where TDto : IMessageDto
	{
		protected readonly object LockObject = new object();
		protected MethodInfo method;

		private readonly Dictionary<string, HandlerStoreItem> handlerStore = new Dictionary<string, HandlerStoreItem>();


		public delegate void HandleMessageDelegate<in Td>(Td message, string senderId);

		public HandlerTargetApi<TCnv, TSer, TDes, TDto> Message { get; internal set; }
		public Nexus<TCnv, TSer, TDes, TDto> Nexus { get; internal set; }
		public HashSet<string> Participants { get; } = new HashSet<string>();

		protected HandlerGroup(bool active = true)
		{
			Active = active;
		}

		public bool Active { get; set; }

		public virtual void Initialize(Nexus<TCnv, TSer, TDes, TDto> nexus)
		{
			Nexus = nexus;
			Message = new HandlerTargetApi<TCnv, TSer, TDes, TDto>(Nexus, this);
			method = Nexus.Converter.GetType().GetMethod("ReadMessage");
			if (method == null)
				throw new ArgumentException("The converter you passed doesn't have a method 'ReadMessage'.");

			var keys = new List<string>(handlerStore.Keys);
			foreach (var key in keys)
			{
				var item = handlerStore[key];
				item.GenericReadMessageMethod = method.MakeGenericMethod(item.GenericType);
				handlerStore[key] = item;
			}
		}

		public void AddHandler<T>(Enum key, HandleMessageDelegate<T> handler) where T : TDto
		{
			handlerStore.Add(key.ToString(), ConvertDelegate(handler));
		}

		private HandlerStoreItem ConvertDelegate<T>(HandleMessageDelegate<T> handler)
		{
			var item = new HandlerStoreItem
			{
				HandlerDelegate =
				Delegate.CreateDelegate(typeof(HandleMessageDelegate<T>), handler.Target, handler.Method),
				Type = handler.GetType(),
				GenericType = handler.GetType().GetGenericArguments()[4]
			};
			return item;
		}

		public bool Handle(string messageType, LowLevelMessage message)
		{
			if (!Active) return false;

			if (!handlerStore.ContainsKey(messageType)) return false;

			var h = handlerStore[messageType];

			var mObject =
				h.GenericReadMessageMethod.Invoke(Nexus.Converter, new object[] {message.Stream, message.MessageSize});
			var m = Convert.ChangeType(mObject, h.GenericType);
			h.HandlerDelegate.DynamicInvoke(m, message.UserId);

			return true;
		}

		public virtual void Update(GameTime gt)
		{
		}
	}
}