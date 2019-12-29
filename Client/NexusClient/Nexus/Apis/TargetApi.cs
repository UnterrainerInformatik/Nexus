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
using NexusClient.Converters;

namespace NexusClient.Nexus.Apis
{
	public class TargetApi<TConv, TSer, TDes, T> where TSer : IMessageSer<T>
		where TDes : IMessageDes<T>
		where TConv : IConverter<T>
		where T : IMessageDto
	{
		private readonly Nexus<TConv, TSer, TDes, T> nexus;

		internal TargetApi(Nexus<TConv, TSer, TDes, T> nexus)
		{
			this.nexus = nexus;
		}

		internal void Send<TObject>(Enum messageType, TObject content, SendType sendType, IEnumerable<string> recipientIds)
			where TObject : T
		{
			nexus.Send(messageType, content, sendType, recipientIds);
		}

		public MessageApi<TConv, TSer, TDes, T> To(params string[] userId)
		{
			var m = MessageApi<TConv, TSer, TDes, T>.Create(this);
			m.Recipients = userId;
			return m;
		}

		public MessageApi<TConv, TSer, TDes, T> ToAll()
		{
			var m = MessageApi<TConv, TSer, TDes, T>.Create(this);
			m.Recipients = nexus.Participants.Keys.ToArray();
			return m;
		}

		public MessageApi<TConv, TSer, TDes, T> ToAllExcept(params string[] userId)
		{
			var m = MessageApi<TConv, TSer, TDes, T>.Create(this);
			m.Recipients = new string[] { };
			var l = nexus.Participants.Keys.Where(e => !userId.Contains(e));
			m.Recipients = l.ToArray();
			return m;
		}

		public MessageApi<TConv, TSer, TDes, T> ToOthers()
		{
			var m = MessageApi<TConv, TSer, TDes, T>.Create(this);
			m.Recipients = new string[] { };
			var l = nexus.Participants.Keys.Where(e => !e.Equals(nexus.UserId));
			m.Recipients = l.ToArray();
			return m;
		}

		public MessageApi<TConv, TSer, TDes, T> ToOthersExcept(params string[] userId)
		{
			var m = MessageApi<TConv, TSer, TDes, T>.Create(this);
			m.Recipients = new string[] { };
			var l = nexus.Participants.Keys.Where(e => !userId.Contains(e) && !e.Equals(nexus.UserId));
			m.Recipients = l.ToArray();
			return m;
		}

		public MessageApi<TConv, TSer, TDes, T> ToSelf()
		{
			var m = MessageApi<TConv, TSer, TDes, T>.Create(this);
			m.Recipients = new[] {nexus.UserId};
			return m;
		}

		public MessageApi<TConv, TSer, TDes, T> To(string userId)
		{
			var m = MessageApi<TConv, TSer, TDes, T>.Create(this);
			m.Recipients = new[] {userId};
			return m;
		}
	}
}