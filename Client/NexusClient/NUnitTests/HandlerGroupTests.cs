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

using System.IO;
using Moq;
using NexusClient.Converters.MessagePack;
using NexusClient.Network.Testing;
using NexusClient.Nexus;
using NexusClient.NUnitTests.Infrastructure;
using NUnit.Framework;

namespace NexusClient.NUnitTests
{
	[TestFixture()]
	public class HandlerGroupTests
	{
		private TestHandlerGroup hg;
		private MessagePackConverter converter;
		private TestContent content;
		private readonly byte[] buffer = new byte[2000];
		private Stream stream; 

		[SetUp]
		public void Setup()
		{
			stream = new MemoryStream(buffer);

			var server = new Mock<TestServer>();
			hg = new TestHandlerGroup(server.Object);
			converter = new MessagePackConverter();

			content = new TestContent();
			var writeStream = new MemoryStream(buffer);
			converter.Serializer.Serialize(content, writeStream);
		}

		[Test]
		public void AddAndGetHandlerDoesNotThrowException()
		{
			hg.Handle<MessagePackConverter, MessagePackDto>("ELECTION_CALL",
				new LowLevelMessage() {UserId = "1", MessageType = "ELECTION_CALL", Stream = stream}, converter);
		}

		[Test]
		public void RightHandlersAreGettingCalled()
		{
			var writeStream = new MemoryStream(buffer);
			converter.Serializer.Serialize(content, writeStream);

			hg.Handle<MessagePackConverter, MessagePackDto>("ELECTION_CALL",
				new LowLevelMessage() {UserId = "1", MessageType = "ELECTION_CALL", Stream = stream }, converter);
			stream.Position = 0;
			Assert.AreEqual(0, hg.ElectionCallAnswerCount);
			Assert.AreEqual(1, hg.ElectionCallCount);

			hg.Handle<MessagePackConverter, MessagePackDto>("ELECTION_CALL_ANSWER",
				new LowLevelMessage() {UserId = "2", MessageType = "ELECTION_CALL_ANSWER", Stream = stream }, converter);
			stream.Position = 0;
			Assert.AreEqual(1, hg.ElectionCallAnswerCount);
			Assert.AreEqual(1, hg.ElectionCallCount);
		}
	}
}