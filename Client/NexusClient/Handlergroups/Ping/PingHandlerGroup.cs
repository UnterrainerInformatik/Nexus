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
using Microsoft.Xna.Framework;
using NexusClient.Converters.MessagePack;
using NexusClient.HandlerGroups.Ping.DTOs;
using NexusClient.Nexus;
using Serilog;

namespace NexusClient.HandlerGroups.Ping
{
	public class PingHandlerGroup : HandlerGroup<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto>
	{
		public static readonly DateTime DATE_TIME_UNIX_TIMESTAMP_MINVALUE =
			new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public Dictionary<string, PingData> LocalPingDataAboutUsers { get; } = new Dictionary<string, PingData>();

		public Dictionary<string, Dictionary<string, PingData>> BroadcastDataFromPingingUsers { get; } =
			new Dictionary<string, Dictionary<string, PingData>>();

		public Dictionary<string, Dictionary<string, PingData>> BroadcastDataPerPingedUsers { get; } =
			new Dictionary<string, Dictionary<string, PingData>>();

		public readonly Timer.Timer Timer = new Timer.Timer(1000f);
		public bool IsActivelyPinging { get; set; }

		public PingHandlerGroup(bool isActivelyPinging)
		{
			IsActivelyPinging = isActivelyPinging;
			AddHandler<PingMessage>(PingMessageType.PING, PingMessageReceived);
			AddHandler<PongMessage>(PingMessageType.PONG, PongMessageReceived);
			AddHandler<PingBroadcastMessage>(PingMessageType.BROADCAST, BroadcastMessageReceived);
		}

		public override void Initialize(
			Nexus<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto> nexus)
		{
			base.Initialize(nexus);
			LocalPingDataAboutUsers.Add(Nexus.UserId,
				new PingData(DATE_TIME_UNIX_TIMESTAMP_MINVALUE) {UserId = Nexus.UserId});
		}

		public override void Update(GameTime gt)
		{
			base.Update(gt);

			if (!IsActivelyPinging) return;
			if (!Timer.Update(gt)) return;

			PingOthers();
			BroadcastPingResults();
		}

		public void PingOthers()
		{
			var now = DateTime.UtcNow;
			Log.Debug($"Pinging clients at [{now:hh:mm:ss.FFF}]");
			Message.ToOthers().Send(PingMessageType.PING, new PingMessage() {ServerLastPingSentUtc = ToTimestamp(now)});
			var keys = new List<string>(LocalPingDataAboutUsers.Keys);
			foreach (var key in keys)
			{
				var item = LocalPingDataAboutUsers[key];
				item.ServerLastPingSentUtc = now;
				LocalPingDataAboutUsers[key] = item;
			}
		}

		public void BroadcastPingResults()
		{
			Log.Debug($"Broadcasting ping results.");

			AddResultsToBroadcastDataCollections(Nexus.UserId, LocalPingDataAboutUsers.Values);

			Message.ToOthers().Send(PingMessageType.BROADCAST,
				new PingBroadcastMessage() {Data = LocalPingDataAboutUsers.Values.ToArray()});
		}

		public static double ToTimestamp(DateTime d)
		{
			return d.ToUniversalTime().Subtract(DATE_TIME_UNIX_TIMESTAMP_MINVALUE).TotalMilliseconds;
		}

		public static DateTime ToDateTime(double d)
		{
			return DATE_TIME_UNIX_TIMESTAMP_MINVALUE.AddMilliseconds(Convert.ToDouble(d));
		}

		private void PingMessageReceived(PingMessage message, string senderId)
		{
			var data = LocalPingDataAboutUsers[Nexus.UserId];
			data.ServerLastPingSentUtc = ToDateTime(message.ServerLastPingSentUtc);
			data.ClientLastPingReceivedUtc = DateTime.UtcNow;

			Log.Debug(
				$"[{Nexus.UserId}]: Ping received from [{senderId}] at [{data.ClientLastPingReceivedUtc:hh:mm:ss.FFF}] " +
				$"with server-time [{data.ServerLastPingSentUtc:hh:mm:ss.FFF}]. Sending Pong.");

			Message.To(senderId).Send(PingMessageType.PONG,
				new PongMessage()
				{
					ServerLastPingSentUtc = message.ServerLastPingSentUtc,
					ClientLastPingReceivedUtc = ToTimestamp(data.ClientLastPingReceivedUtc)
				});
		}

		private void PongMessageReceived(PongMessage message, string senderId)
		{
			if (!LocalPingDataAboutUsers.TryGetValue(senderId, out var data))
				data = new PingData(DATE_TIME_UNIX_TIMESTAMP_MINVALUE) {UserId = senderId};

			data.ServerLastPingSentUtc = ToDateTime(message.ServerLastPingSentUtc);
			data.ClientLastPingReceivedUtc = ToDateTime(message.ClientLastPingReceivedUtc);
			data.ServerLastPongReceivedUtc = DateTime.UtcNow;

			var ms = data.ServerLastPongReceivedUtc.Subtract(data.ServerLastPingSentUtc).TotalMilliseconds;
			LocalPingDataAboutUsers[senderId] = data;
			Log.Debug(
				$"[{Nexus.UserId}]: Pong received from [{senderId}] at [{data.ServerLastPongReceivedUtc:hh:mm:ss.FFF}] " +
				$"to ping from [{data.ServerLastPingSentUtc:hh:mm:ss.FFF}] -> {ms:###,###,###,###}ms.");
		}

		private void BroadcastMessageReceived(PingBroadcastMessage message, string senderId)
		{
			AddResultsToBroadcastDataCollections(senderId, message.Data);
		}

		private void AddResultsToBroadcastDataCollections(string fromUserId, IEnumerable<PingData> dataRows)
		{
			var dataFrom = GetCollectionExtendingIfNecessary(BroadcastDataFromPingingUsers, fromUserId);
			foreach (var data in dataRows)
			{
				dataFrom[data.UserId] = data;
				GetCollectionExtendingIfNecessary(BroadcastDataPerPingedUsers, data.UserId)[fromUserId] = data;
			}
		}

		private Dictionary<string, PingData> GetCollectionExtendingIfNecessary(
			IDictionary<string, Dictionary<string, PingData>> dictionary, string id)
		{
			if (dictionary.TryGetValue(id, out var c)) return c;
			c = new Dictionary<string, PingData>();
			dictionary[id] = c;

			return c;
		}
	}
}