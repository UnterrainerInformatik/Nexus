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
		private static readonly DateTime dateTimeUnixTimestampMinvalue =
			new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public Dictionary<string, PingData> LocalPingDataAboutUsers { get; } = new Dictionary<string, PingData>();

		public Dictionary<string, Dictionary<string, PingData>> BroadcastDataByPingingUsers { get; } =
			new Dictionary<string, Dictionary<string, PingData>>();

		public Dictionary<string, Dictionary<string, PingData>> BroadcastDataByPingedUsers { get; } =
			new Dictionary<string, Dictionary<string, PingData>>();

		public readonly Timer.Timer Timer;
		public bool IsActivelyPinging { get; set; }

		public PingHandlerGroup(bool isActivelyPinging = true, float pingIntervalInMilliseconds = 1000f)
		{
			IsActivelyPinging = isActivelyPinging;
			Timer = new Timer.Timer(pingIntervalInMilliseconds);
			AddHandler<PingMessage>(PingMessageType.PING, PingMessageReceived);
			AddHandler<PongMessage>(PingMessageType.PONG, PongMessageReceived);
			AddHandler<PingBroadcastMessage>(PingMessageType.BROADCAST, BroadcastMessageReceived);
		}

		public override void Initialize(
			Nexus<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto> nexus)
		{
			base.Initialize(nexus);
			LocalPingDataAboutUsers.Add(Nexus.UserId,
				new PingData() {UserId = Nexus.UserId});
		}

		public override void Update(GameTime gt)
		{
			base.Update(gt);

			if (!IsActivelyPinging) return;
			if (!Timer.Update(gt)) return;

			Log.Debug($"[{Nexus.UserId}]: Pinging clients at [{DateTime.UtcNow:hh:mm:ss.FFF}] and broadcasting old results.");
			PingOthers();
			BroadcastPingResults();
		}

		private void PingOthers()
		{
			var now = DateTime.UtcNow;
			Message.ToOthers().Send(PingMessageType.PING, new PingMessage() {ServerLastPingSentUtc = ToTimestamp(now)});
			var keys = new List<string>(LocalPingDataAboutUsers.Keys);
			foreach (var key in keys)
			{
				var item = LocalPingDataAboutUsers[key];
				item.MessageSentUtc = now;
				LocalPingDataAboutUsers[key] = item;
			}
		}

		private void BroadcastPingResults()
		{
			AddResultsToBroadcastDataCollections(Nexus.UserId, LocalPingDataAboutUsers.Values);

			Message.ToOthers().Send(PingMessageType.BROADCAST,
				new PingBroadcastMessage() {Data = LocalPingDataAboutUsers.Values.ToArray()});
		}

		private static double ToTimestamp(DateTime d)
		{
			return d.ToUniversalTime().Subtract(dateTimeUnixTimestampMinvalue).TotalMilliseconds;
		}

		private static DateTime ToDateTime(double d)
		{
			return dateTimeUnixTimestampMinvalue.AddMilliseconds(Convert.ToDouble(d));
		}

		private void PingMessageReceived(PingMessage message, string senderId)
		{
			Log.Debug($"[{Nexus.UserId}]: Ping received from [{senderId}] at [{DateTime.UtcNow:hh:mm:ss.FFF}] " +
					$"with server-time [{ToDateTime(message.ServerLastPingSentUtc):hh:mm:ss.FFF}]. Sending Pong.");

			Message.To(senderId).Send(PingMessageType.PONG,
				new PongMessage() {ServerLastPingSentUtc = message.ServerLastPingSentUtc});
		}

		private void PongMessageReceived(PongMessage message, string senderId)
		{
			if (!LocalPingDataAboutUsers.TryGetValue(senderId, out var data))
				data = new PingData() {UserId = senderId};

			data.MessageSentUtc = ToDateTime(message.ServerLastPingSentUtc);
			data.MessageReceivedUtc = DateTime.UtcNow;
			data.LastRoundtripTimeInMillis = data.MessageReceivedUtc.Subtract(data.MessageSentUtc).TotalMilliseconds;
			LocalPingDataAboutUsers[senderId] = data;

			Log.Debug(
				$"[{Nexus.UserId}]: Pong received from [{senderId}] at [{data.MessageReceivedUtc:hh:mm:ss.FFF}] " +
				$"to ping from [{data.MessageSentUtc:hh:mm:ss.FFF}] -> {data.LastRoundtripTimeInMillis:###,###,###,###}ms.");
		}

		private void BroadcastMessageReceived(PingBroadcastMessage message, string senderId)
		{
			Log.Debug($"[{Nexus.UserId}]: Broadcast received from [{senderId}] at [{DateTime.UtcNow:hh:mm:ss.FFF}] " +
					$"with [{message.Data.Length}] elements.");
			AddResultsToBroadcastDataCollections(senderId, message.Data);
		}

		private void AddResultsToBroadcastDataCollections(string fromUserId, IEnumerable<PingData> dataRows)
		{
			var dataFrom = GetCollectionExtendingIfNecessary(BroadcastDataByPingingUsers, fromUserId);
			foreach (var data in dataRows)
			{
				dataFrom[data.UserId] = data;
				GetCollectionExtendingIfNecessary(BroadcastDataByPingedUsers, data.UserId)[fromUserId] = data;
			}
		}

		private static Dictionary<string, PingData> GetCollectionExtendingIfNecessary(
			IDictionary<string, Dictionary<string, PingData>> dictionary, string id)
		{
			if (dictionary.TryGetValue(id, out var c)) return c;
			c = new Dictionary<string, PingData>();
			dictionary[id] = c;

			return c;
		}
	}
}