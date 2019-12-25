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
using NexusClient.Network.Testing;
using NexusClient.Nexus;
using NexusClient.NUnitTests.Infrastructure;
using NexusClient.Utils;
using NUnit.Framework;
using Serilog;

namespace HandlerTests
{
	public class NexusObjects
	{
		public Nexus<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto> Nexus { get; }
		private TestTransport Transport { get; }
		public TestHandlerGroup TestHandlerGroup { get; }
		public readonly string UserId;

		public NexusObjects(TestServer server, MessagePackConverter converter)
		{
			Transport = new TestTransport(server);
			Nexus = new Nexus<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto>(Transport,
				converter);
			Nexus.Initialize();
			UserId = Nexus.UserId;

			TestHandlerGroup = new TestHandlerGroup(server);
			Nexus.RegisterOrOverwriteHandlerGroup(TestHandlerGroup);
		}
	}

	[TestFixture]
	class TestHandlerTests
	{
		private TestServer server;
		private Dictionary<string, NexusObjects> nexi;
		private MessagePackConverter converter;

		[SetUp]
		public void Setup()
		{
			Logger.Init();
			server = new TestServer();
			converter = new MessagePackConverter();
			nexi = new Dictionary<string, NexusObjects>();
			for (var i = 0; i < 2; i++)
			{
				var nexusObjects = new NexusObjects(server, converter);
				nexi.Add(nexusObjects.UserId, nexusObjects);
			}

			foreach (var nexusObjects in nexi.Values) nexusObjects.Nexus.AddParticipants(nexi.Keys.ToArray());
		}

		[Test]
		public void AddingHandlersWorksTest()
		{
			var gt = new TestGameTime();
			Update(gt.Value());

			Log.Debug(
				$"--- t = {gt.Value().TotalGameTime} seconds----------------------------------------------------------");
			gt.Advance(1);
			Update(gt.Value());
		}

		private void Update(GameTime gt)
		{
			foreach (var nexusObject in nexi.Values) nexusObject.Nexus.Update(gt);
		}
	}
}