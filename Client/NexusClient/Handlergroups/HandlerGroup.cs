﻿// ***************************************************************************
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

namespace NexusClient.HandlerGroups
{
	internal struct HandlerStoreItem
	{
		public Delegate HandlerDelegate { get; set; }
		public Type Type { get; set; }

		public Type GenericType { get; set; }
		public MethodInfo GenericReadMessageMethod { get; set; }
	}

	public abstract class HandlerGroup<TTpt, TSer, TDes, TDto> where TTpt : IConverter<TDto>
		where TSer : IMessageSer<TDto>
		where TDes : IMessageDes<TDto>
		where TDto : IMessageDto
	{
		protected readonly object LockObject = new object();

		protected MethodInfo method;

		public delegate void HandleMessageDelegate<in TD>(TD message, string senderId);

		private readonly Dictionary<string, HandlerStoreItem> handlerStore = new Dictionary<string, HandlerStoreItem>();

		public Nexus<TTpt, TSer, TDes, TDto> Nexus { get; internal set; }

		protected HandlerGroup(bool active = true)
		{
			Active = active;
		}

		public bool Active { get; set; }

		public void Initialize(Nexus<TTpt, TSer, TDes, TDto> nexus)
		{
			Nexus = nexus;
			method = Nexus.Converter.GetType().GetMethod("ReadMessage");
			if (method == null)
				throw new ArgumentException("The converter you passed doesn't have a method 'ReadMessage'.");

			var keys = new List<string>(handlerStore.Keys);
			foreach (var key in keys)
			{
				var item = handlerStore[key];
				item.GenericReadMessageMethod = method.MakeGenericMethod(item.GenericType);
				handlerStore.Remove(key);
				handlerStore.Add(key, item);
			}
		}

		public void AddHandler<TT>(Enum key, HandleMessageDelegate<TT> handler) where TT : TDto
		{
			handlerStore.Add(key.ToString(), ConvertDelegate(handler));
		}

		private HandlerStoreItem ConvertDelegate<TT>(HandleMessageDelegate<TT> handler)
		{
			var item = new HandlerStoreItem();
			item.HandlerDelegate =
				Delegate.CreateDelegate(typeof(HandleMessageDelegate<TT>), handler.Target, handler.Method);
			item.Type = handler.GetType();
			item.GenericType = handler.GetType().GetGenericArguments()[4];
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