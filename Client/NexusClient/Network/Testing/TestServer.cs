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
using Faders;
using Microsoft.Xna.Framework;
using Serilog;

namespace NexusClient.Network.Testing
{
	public class Connection
	{
		public string UserId { get; set; }

		public TimeSpan LatencyFixed { get; set; }
		public Interval<double> LatencyRandInMilliseconds { get; set; }

		public List<TestMessage> Messages { get; set; }

		public static Connection Create(string userId)
		{
			return new Connection()
			{
				UserId = userId,
				LatencyFixed = TimeSpan.FromMilliseconds(1),
				LatencyRandInMilliseconds = new Interval<double>(0, 0),
				Messages = new List<TestMessage>()
			};
		}
	}

	public class TestMessageComparer : IComparer<TestMessage>
	{
		public int Compare(TestMessage x, TestMessage y)
		{
			return x.WillBeReceivedAt.CompareTo(y.WillBeReceivedAt);
		}
	}

	public struct TestMessage
	{
		public byte[] Buffer { get; set; }
		public uint Size { get; set; }
		public string SenderId { get; set; }
		public TimeSpan WillBeReceivedAt { get; set; }

		public bool LatencyHasExpired(GameTime gt)
		{
			return gt.TotalGameTime.CompareTo(WillBeReceivedAt) >= 0;
		}
	}

	public class TestServer
	{
		protected int LastUserId { get; set; } = 1;
		protected readonly Dictionary<string, Connection> Connections = new Dictionary<string, Connection>();
		protected GameTime currentGameTime = new GameTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0));

		private readonly TestMessageComparer testMessageComparer = new TestMessageComparer();
		private byte[] uint64Buffer;
		private readonly Random random = new Random();

		public void Update(GameTime gt)
		{
			currentGameTime = gt;
		}

		public string Login()
		{
			var userId = $"user{LastUserId++}";
			Connections.Add(userId, Connection.Create(userId));
			Log.Verbose($"[{userId}] Login");
			return userId;
		}

		public void Logout(string userId)
		{
			Log.Verbose($"[{userId}] Logout");
			Connections.Remove(userId);
		}

		public bool IsLoggedIn(string userId)
		{
			return Connections.TryGetValue(userId, out _);
		}

		/// <summary>
		///     Resets the latency settings for the given user to the default values.
		///     The defaults are:
		///     Fixed-latency = 1ms
		///     Random-latency = [0ms, 0ms]
		/// </summary>
		/// <param name="userId">The ID of the user to set the latency for.</param>
		public void LatencyResetFor(string userId)
		{
			LatencyFixedFor(userId, TimeSpan.FromMilliseconds(0));
			LatencyRandInMillisecondsFor(userId, new Interval<double>(0, 0));
		}

		/// <summary>
		///     Latency is calculated upon sending a message.
		/// </summary>
		/// <param name="userId">The ID of the user to set the latency for.</param>
		/// <param name="value">The fixed latency (value is always added; default is 1ms).</param>
		public void LatencyFixedFor(string userId, TimeSpan value)
		{
			GetConnectionFor(userId).LatencyFixed = value;
		}

		/// <summary>
		///     Latency is calculated upon sending a message.
		/// </summary>
		/// <param name="userId">The ID of the user to set the latency for.</param>
		/// <param name="interval">
		///     An interval in between an additional latency-value is picked and added to the overall-latency
		///     for the message currently being sent (defaults to new Interval<double>(0,0))</double>
		/// </param>
		public void LatencyRandInMillisecondsFor(string userId, Interval<double> interval)
		{
			GetConnectionFor(userId).LatencyRandInMilliseconds = interval;
		}

		private Connection GetConnectionFor(string userId)
		{
			return !Connections.TryGetValue(userId, out var q) ? null : q;
		}

		private double RandomIn(Interval<double> interval)
		{
			if (interval.Min.CompareTo(interval.Max) == 0) return 0d;

			uint64Buffer = new byte[8];
			random.NextBytes(uint64Buffer);
			var uLong = BitConverter.ToUInt64(uint64Buffer, 0);
			return interval.Min + ((double) uLong / ulong.MaxValue) * (interval.Max - interval.Min);
		}

		private TimeSpan CalculateLatency(Connection conn)
		{
			return TimeSpan.FromMilliseconds(0d).Add(conn.LatencyFixed)
				.Add(TimeSpan.FromMilliseconds(RandomIn(conn.LatencyRandInMilliseconds)));
		}

		public bool IsMessageAvailableFor(string userId, out uint size)
		{
			Log.Verbose($"[{userId}] IsMessageAvailable");
			size = 0;
			var conn = GetConnectionFor(userId);
			var messages = conn.Messages;
			if (messages == null || messages.Count == 0) return false;

			messages.Sort(testMessageComparer);
			var m = messages[0];
			if (!m.LatencyHasExpired(currentGameTime)) return false;

			size = m.Size;
			return true;
		}

		public bool ReadMessageFor(string userId, out TestMessage message)
		{
			Log.Verbose($"[{userId}] GetMessage");
			message = new TestMessage();
			var conn = GetConnectionFor(userId);
			var messages = conn.Messages;
			if (messages == null) return false;

			messages.Sort(testMessageComparer);
			var m = messages[0];
			if (!m.LatencyHasExpired(currentGameTime)) return false;

			messages.RemoveAt(0);
			message = m;
			Log.Verbose($"[{userId}] ...message sender is [{message.SenderId}]");
			return true;
		}

		public bool SendMessageFor(string senderId, string recipientId, byte[] data, uint length)
		{
			Log.Verbose($"[{senderId}] SendMessage to [{recipientId}]");
			var conn = GetConnectionFor(recipientId);
			var messages = conn.Messages;
			if (messages == null) return false;

			var m = new TestMessage
			{
				SenderId = senderId,
				Size = length,
				Buffer = new byte[length],
				WillBeReceivedAt = currentGameTime.TotalGameTime.Add(CalculateLatency(conn))
			};
			Buffer.BlockCopy(data, 0, m.Buffer, 0, (int) length);
			messages.Add(m);
			return true;
		}
	}
}