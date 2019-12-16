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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using NexusClient.Interfaces;

namespace NexusClient.Testing.Try1
{
	public class Network<TConverter, TSend, TReceive> where TSend : IMessageSerializer<TSend>
		where TReceive : IMessageDeserializer<TReceive> where TConverter : IMessageConverter<TSend, TReceive>
	{
		internal class MessageApi
		{
			private readonly Network<TConverter, TSend, TReceive> network;

			internal MessageApi(Network<TConverter, TSend, TReceive> network)
			{
				this.network = network;
			}

			public Message To(params string[] userId)
			{
				var m = Try1.Message.GetDefault();
				m.Sender = network.UserId;
				m.Recepients = userId;
				return m;
			}

			public Message ToAll()
			{
				var m = Try1.Message.GetDefault();
				m.Sender = network.UserId;
				m.Recepients = network.Participants.Keys.ToArray();
				return m;
			}

			public Message ToAllExcept(params string[] userId)
			{
				var m = Try1.Message.GetDefault();
				m.Sender = network.UserId;
				m.Recepients = new string[] { };
				var l = network.Participants.Keys.Where(e => !userId.Contains(e));
				m.Recepients = l.ToArray();
				return m;
			}

			public Message ToOthers()
			{
				var m = Try1.Message.GetDefault();
				m.Sender = network.UserId;
				m.Recepients = new string[] { };
				var l = network.Participants.Keys.Where(e => !e.Equals(network.UserId));
				m.Recepients = l.ToArray();
				return m;
			}

			public Message ToOthersExcept(params string[] userId)
			{
				var m = Try1.Message.GetDefault();
				m.Sender = network.UserId;
				m.Recepients = new string[] { };
				var l = network.Participants.Keys.Where(e => !userId.Contains(e) && !e.Equals(network.UserId));
				m.Recepients = l.ToArray();
				return m;
			}

			public Message ToSelf()
			{
				var m = Try1.Message.GetDefault();
				m.Sender = network.UserId;
				m.Recepients = new[] {network.UserId};
				return m;
			}

			public Message To(string userId)
			{
				var m = Try1.Message.GetDefault();
				m.Sender = network.UserId;
				m.Recepients = new[] {userId};
				return m;
			}
		}

		internal MessageApi Message { get; }
		public string UserId { get; set; }
		protected readonly Dictionary<string, string> Participants = new Dictionary<string, string>();

		private BinaryReader Reader;
		private BinaryWriter Writer;

		public Network()
		{
			Message = new MessageApi(this);
		}

		public void Update()
		{
		}

		public Network<TConverter, TSend, TReceive> AddParticipants(params string[] userId)
		{
			foreach (var id in userId)
			{
				Participants.Add(id, id);
			}

			return this;
		}

		public Network<TConverter, TSend, TReceive> RemoveParticipants(params string[] userId)
		{
			foreach (var id in userId)
			{
				Participants.Remove(id);
			}

			return this;
		}
	}

	public interface IMessageConverter<T>
	{
	}
}