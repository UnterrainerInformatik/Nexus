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
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using NexusClient.Converters;
using NexusClient.Nexus;

namespace NexusClient.HandlerGroups
{
	internal struct HandlerStoreItem
	{
		public Delegate Del { get; set; }
		public Type Type { get; set; }
		public Enum MessageType { get; set; }

		public Type GenericType { get; set; }
		public MethodInfo GenericMethod { get; set; }
		public Delegate Handler { get; set; }
	}

	public abstract class HandlerGroup<TTrans, TSer, TDes, T> where TTrans : ITransport<T>
		where TSer : IMessageSer<T>
		where TDes : IMessageDes<T>
		where T : IMessageDto
	{
		protected readonly object LockObject = new object();

		protected MethodInfo method;

		public delegate void HandleMessageDelegate<TD>(TD message, string senderId);

		private readonly Dictionary<string, HandlerStoreItem> handlerStore = new Dictionary<string, HandlerStoreItem>();

		public Nexus<TTrans, TSer, TDes, T> Nexus { get; internal set; }

		protected HandlerGroup(bool active = true)
		{
			Active = active;
		}

		public bool Active { get; set; }

		public void Initialize(Nexus<TTrans, TSer, TDes, T> nexus)
		{
			Nexus = nexus;
			method = Nexus.Converter.GetType().GetMethod("ReadMessage");
			if(method == null) throw new ArgumentException("The converter you passed doesn't have a method 'ReadMessage'.");

			List<string> keys = new List<string>(handlerStore.Keys); 
			foreach (var key in keys)
			{
				var item = handlerStore[key];
				item.GenericMethod = method.MakeGenericMethod(item.GenericType);
				handlerStore.Remove(key);
				handlerStore.Add(key, item);
			}
		}

		public void AddHandler<TT>(Enum key, HandleMessageDelegate<TT> handler) where TT : T
		{
			handlerStore.Add(key.ToString(), ConvertDelegate(key, handler));
		}

		private HandlerStoreItem ConvertDelegate<TT>(Enum messageType, HandleMessageDelegate<TT> handler)
		{
			var item = new HandlerStoreItem();
			item.Del = Delegate.CreateDelegate(typeof(HandleMessageDelegate<TT>), handler.Target, handler.Method);
			item.Type = handler.GetType();
			item.MessageType = messageType;
			item.GenericType = item.Type.GetGenericArguments()[4];
			item.Handler = Delegate.CreateDelegate(item.Type, item.Del.Target, item.Del.Method);
			return item;
		}

		public bool Handle(string messageType, LowLevelMessage message)
		{
			if (!Active) return false;

			if (!handlerStore.ContainsKey(messageType)) return false;

			var h = handlerStore[messageType];

			var mObject = h.GenericMethod.Invoke(Nexus.Converter, new object[] {message.Stream, message.MessageSize});
			var m = Convert.ChangeType(mObject, h.GenericType);
			h.Handler.DynamicInvoke(m, message.UserId);

			return true;
		}

		public virtual void Update(GameTime gt)
		{
		}
	}
}