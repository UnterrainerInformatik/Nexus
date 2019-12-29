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
using System.Linq;
using Microsoft.Xna.Framework;
using NexusClient.Converters.MessagePack;
using NexusClient.HandlerGroups.Bully;
using NexusClient.Network.Testing;
using NexusClient.Nexus;
using NexusClient.Utils;
using NUnit.Framework;
using Serilog;

namespace HandlerTests.Bully
{
	public class NexusClient
	{
		public Nexus<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto> Nexus { get; }
		private TestTransport Transport { get; }
		public BullyHandlerGroup BullyHandlerGroup { get; }
		public readonly string UserId;

		public NexusClient(TestServer server, MessagePackConverter converter)
		{
			Transport = new TestTransport(server);
			Nexus = new Nexus<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto>(Transport,
				converter);
			Nexus.Initialize();
			UserId = Nexus.UserId;

			BullyHandlerGroup = new BullyHandlerGroup(UserId);
			Nexus.RegisterOrOverwriteHandlerGroup(BullyHandlerGroup);
		}
	}

	[TestFixture]
	class BullyTest
	{
		private TestServer server;
		private Dictionary<string, NexusClient> clients;
		private MessagePackConverter converter;

		[SetUp]
		public void Setup()
		{
			Logger.Init();
			server = new TestServer();
			converter = new MessagePackConverter();
			clients = new Dictionary<string, NexusClient>();
			for (var i = 0; i < 20; i++)
			{
				var client = new NexusClient(server, converter);
				clients.Add(client.UserId, client);
			}
			foreach (var client in clients.Values) client.Nexus.AddParticipants(clients.Keys.ToArray());
		}

		[Test]
		public void WhenCallerIsLowestIdCallerWinsTest()
		{
			var gt = new TestGameTime();
			Update(gt.Value());
			clients["user1"].BullyHandlerGroup.StartBullyElection();
			
			for (var i = 0; i < 5; i++)
			{
				Log.Debug(
					$"--- t = {gt.Value().TotalGameTime} seconds----------------------------------------------------------");
				PrintLeaders();
				gt.Advance(1);
				Update(gt.Value());
			}
		}

		[Test]
		public void WhenCallerIsSecondLowestIdCallerGetsUpdateTest()
		{
			var gt = new TestGameTime();
			Update(gt.Value());
			clients["user2"].BullyHandlerGroup.StartBullyElection();

			for (var i = 0; i < 5; i++)
			{
				Log.Debug(
					$"--- t = {gt.Value().TotalGameTime} seconds----------------------------------------------------------");
				PrintLeaders();
				gt.Advance(1);
				Update(gt.Value());
			}
		}

		[Test]
		public void WhenCallerIsHighestIdCallerGetsUpdateTest()
		{
			var gt = new TestGameTime();
			Update(gt.Value());
			clients["user9"].BullyHandlerGroup.StartBullyElection();

			for (var i = 0; i < 50; i++)
			{
				Log.Debug(
					$"--- t = {gt.Value().TotalGameTime} seconds----------------------------------------------------------");
				PrintLeaders();
				gt.Advance(1);
				Update(gt.Value());
			}
		}

		private void PrintLeaders()
		{
			var s = "### Leaders: [ ";
			foreach (var nexusObject in clients.Values)
			{
				if (!"user1".Equals(nexusObject.UserId)) s += " | ";
				s += nexusObject.BullyHandlerGroup.LeaderId ?? "null";
			}

			s += " ] ###";
			Log.Debug(s);
		}

		private void Update(GameTime gt)
		{
			foreach (var nexusObject in clients.Values) nexusObject.Nexus.Update(gt);
		}
	}
}