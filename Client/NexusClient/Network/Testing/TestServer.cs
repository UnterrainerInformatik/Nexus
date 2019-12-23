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
using Serilog;

namespace NexusClient.Network.Testing
{
	public class TestMessage
	{
		public byte[] Buffer { get; set; }
		public uint Size { get; set; }
		public string SenderId { get; set; }
	}

	public class TestServer
	{
		protected int LastUserId { get; set; } = 1;
		protected readonly Dictionary<string, Queue<TestMessage>> UserConnections =
			new Dictionary<string, Queue<TestMessage>>();

		public string Login()
		{
			var userId = $"user{LastUserId++}";
			UserConnections.Add(userId, new Queue<TestMessage>());
			Log.Verbose($"[{userId}] Login");
			return userId;
		}

		public void Logout(string userId)
		{
			Log.Verbose($"[{userId}] Logout");
			UserConnections.Remove(userId);
		}

		public bool IsLoggedIn(string userId)
		{
			return UserConnections.TryGetValue(userId, out _);
		}

		public Queue<TestMessage> GetQueueFor(string userId)
		{
			return !UserConnections.TryGetValue(userId, out var q) ? null : q;
		}

		public bool IsMessageAvailableFor(string userId, out uint size)
		{
			Log.Verbose($"[{userId}] IsMessageAvailable");
			size = 0;
			var q = GetQueueFor(userId);
			if (q == null || q.Count == 0) return false;

			var m = q.Peek();
			size = m.Size;
			return true;
		}

		public bool ReadMessageFor(string userId, out TestMessage message)
		{
			Log.Verbose($"[{userId}] GetMessage");
			message = null;
			var q = GetQueueFor(userId);
			if (q == null) return false;
			message = q.Dequeue();
			Log.Verbose($"[{userId}] ...message sender is [{message.SenderId}]");
			return true;
		}

		public bool SendMessageFor(string senderId, string recipientId, byte[] data, uint length)
		{
			Log.Verbose($"[{senderId}] SendMessage to [{recipientId}]");
			var q = GetQueueFor(recipientId);
			if (q == null)
				return false;
			var m = new TestMessage();
			m.SenderId = senderId;
			m.Size = length;
			m.Buffer = new byte[length];
			Buffer.BlockCopy(data, 0, m.Buffer, 0, (int) length);
			q.Enqueue(m);
			return true;
		}
	}
}