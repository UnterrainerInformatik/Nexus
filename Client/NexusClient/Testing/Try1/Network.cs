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

namespace NexusClient.Testing.Try1
{
	public class Network
	{
		public string UserId { get; set; }
		protected readonly Dictionary<string, string> Participants = new Dictionary<string, string>();

		private BinaryReader Reader;
		private BinaryWriter Writer;

		public void Update()
		{
		}

		public Network AddParticipants(params string[] userId)
		{
			foreach (var id in userId)
			{
				Participants.Add(id, id);
			}

			return this;
		}

		public Network RemoveParticipants(params string[] userId)
		{
			foreach (var id in userId)
			{
				Participants.Remove(id);
			}

			return this;
		}

		public Message To(params string[] userId)
		{
			var m = Message.GetDefault();
			m.Sender = UserId;
			m.Recepients = userId;
			return m;
		}

		public Message ToAll()
		{
			var m = Message.GetDefault();
			m.Sender = UserId;
			m.Recepients = Participants.Keys.ToArray();
			return m;
		}

		public Message ToAllExcept(params string[] userId)
		{
			var m = Message.GetDefault();
			m.Sender = UserId;
			m.Recepients = new string[] { };
			var l = Participants.Keys.Where(e => !userId.Contains(e));
			m.Recepients = l.ToArray();
			return m;
		}

		public Message ToOthers()
		{
			var m = Message.GetDefault();
			m.Sender = UserId;
			m.Recepients = new string[] { };
			var l = Participants.Keys.Where(e => !e.Equals(UserId));
			m.Recepients = l.ToArray();
			return m;
		}

		public Message ToOthersExcept(params string[] userId)
		{
			var m = Message.GetDefault();
			m.Sender = UserId;
			m.Recepients = new string[] { };
			var l = Participants.Keys.Where(e => !userId.Contains(e) && !e.Equals(UserId));
			m.Recepients = l.ToArray();
			return m;
		}

		public Message ToSelf()
		{
			var m = Message.GetDefault();
			m.Sender = UserId;
			m.Recepients = new[] {UserId};
			return m;
		}

		public Message To(string userId)
		{
			var m = Message.GetDefault();
			m.Sender = UserId;
			m.Recepients = new[] {userId};
			return m;
		}
	}
}