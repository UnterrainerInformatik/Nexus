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

using NexusClient.Converters.MessagePack;
using NexusClient.Network.Testing;
using NexusClient.Nexus.Implementations;
using NexusClient.NUnitTests.Infrastructure;
using NexusClient.Utils;
using NUnit.Framework;
using Serilog;

namespace HandlerTests
{
	[TestFixture()]
	public class NetworkTests
	{
		private MessagePackNexus n1;
		private MessagePackNexus n2;
		private TestHandlerGroup hg1;
		private TestHandlerGroup hg2;
		private TestServer server;

		[SetUp]
		public void Setup()
		{
			Logger.Init();
			server = new TestServer();
			var transport1 = new TestTransport(server);
			var transport2 = new TestTransport(server);
			var converter = new MessagePackConverter();
			hg1 = new TestHandlerGroup(server);
			hg2 = new TestHandlerGroup(server);

			n1 = new MessagePackNexus(transport1, converter);
			n1.Initialize();
			n2 = new MessagePackNexus(transport2, converter);
			n2.Initialize();

			n1.RegisterOrOverwriteHandlerGroup(hg1);
			hg1.Participants.Add(n1.UserId);
			hg1.Participants.Add(n2.UserId);
			n2.RegisterOrOverwriteHandlerGroup(hg2);
			hg2.Participants.Add(n1.UserId);
			hg2.Participants.Add(n2.UserId);
		}

		[Test]
		public void AddAndGetHandlerTest()
		{
			var gameTime = new TestGameTime();
			n1.Message.To(hg1.Participants)
				.Send(TestType.ELECTION_CALL, new TestContent() {TestField = "test from user1"});
			n2.Message.ToOthers(hg2.Participants)
				.Send(TestType.ELECTION_CALL, new TestContent() {TestField = "test from user2"});
			n1.Message.ToOthersExcept(hg1.Participants, n2.UserId).Send(TestType.ELECTION_CALL_ANSWER,
				new TestContent() {TestField = "should not be received"});

			for (var i = 0; i < 3; i++)
			{
				gameTime.AdvanceFrame();
				n1.Update(gameTime.Value());
				n2.Update(gameTime.Value());
				server.Update(gameTime.Value());
			}
		}

		[Test]
		public void MessagesDoNotMultiplyTest()
		{
			var gameTime = new TestGameTime();

			for (var i = 0; i < 3; i++)
			{
				Log.Debug(
					$"--- t = {gameTime.Value().TotalGameTime} seconds----------------------------------------------------------");
				n1.Message.To(hg1.Participants)
					.Send(TestType.ELECTION_CALL, new TestContent() {TestField = "test from user1"});
				gameTime.AdvanceFrame();
				n1.Update(gameTime.Value());
				n2.Update(gameTime.Value());
				server.Update(gameTime.Value());
			}
		}
	}
}