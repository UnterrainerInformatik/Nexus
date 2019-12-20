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

using System.Linq;
using NexusClient.Network.Interfaces;

namespace NexusClient.Network.Apis
{
	public class TargetApi<TTrans, TSer, TDes, T> where TSer : IMessageSer<T>
		where TDes : IMessageDes<T>
		where TTrans : ITransport<T>
		where T : IMessageDto
	{
		private readonly Network<TTrans, TSer, TDes, T> network;

		internal TargetApi(Network<TTrans, TSer, TDes, T> network)
		{
			this.network = network;
		}

		internal void Send(MessageApi<TTrans, TSer, TDes, T> messageApiResult)
		{
			network.Send(messageApiResult);
		}

		public MessageApi<TTrans, TSer, TDes, T> To(params string[] userId)
		{
			var m = MessageApi<TTrans, TSer, TDes, T>.Create(this);
			m.Sender = network.UserId;
			m.Recipients = userId;
			return m;
		}

		public MessageApi<TTrans, TSer, TDes, T> ToAll()
		{
			var m = MessageApi<TTrans, TSer, TDes, T>.Create(this);
			m.Sender = network.UserId;
			m.Recipients = network.Participants.Keys.ToArray();
			return m;
		}

		public MessageApi<TTrans, TSer, TDes, T> ToAllExcept(params string[] userId)
		{
			var m = MessageApi<TTrans, TSer, TDes, T>.Create(this);
			m.Sender = network.UserId;
			m.Recipients = new string[] { };
			var l = network.Participants.Keys.Where(e => !userId.Contains(e));
			m.Recipients = l.ToArray();
			return m;
		}

		public MessageApi<TTrans, TSer, TDes, T> ToOthers()
		{
			var m = MessageApi<TTrans, TSer, TDes, T>.Create(this);
			m.Sender = network.UserId;
			m.Recipients = new string[] { };
			var l = network.Participants.Keys.Where(e => !e.Equals(network.UserId));
			m.Recipients = l.ToArray();
			return m;
		}

		public MessageApi<TTrans, TSer, TDes, T> ToOthersExcept(params string[] userId)
		{
			var m = MessageApi<TTrans, TSer, TDes, T>.Create(this);
			m.Sender = network.UserId;
			m.Recipients = new string[] { };
			var l = network.Participants.Keys.Where(e => !userId.Contains(e) && !e.Equals(network.UserId));
			m.Recipients = l.ToArray();
			return m;
		}

		public MessageApi<TTrans, TSer, TDes, T> ToSelf()
		{
			var m = MessageApi<TTrans, TSer, TDes, T>.Create(this);
			m.Sender = network.UserId;
			m.Recipients = new[] {network.UserId};
			return m;
		}

		public MessageApi<TTrans, TSer, TDes, T> To(string userId)
		{
			var m = MessageApi<TTrans, TSer, TDes, T>.Create(this);
			m.Sender = network.UserId;
			m.Recipients = new[] {userId};
			return m;
		}
	}
}