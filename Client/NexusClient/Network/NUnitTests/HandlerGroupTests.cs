﻿// ***************************************************************************
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

using Moq;
using NexusClient.Network.NUnitTests.TestInfrastructure;
using NexusClient.Testing;
using NUnit.Framework;

namespace NexusClient.Network.NUnitTests
{
	[TestFixture()]
	public class HandlerGroupTests
	{
		private TestHandlerGroup hg;

		[SetUp]
		public void Setup()
		{
			Mock<TestServer> server = new Mock<TestServer>();
			hg = new TestHandlerGroup(server.Object);
		}

		[Test]
		public void AddAndGetHandlerDoesNotThrowException()
		{
			var content = new TestContent();

			hg.Handle("ELECTION_CALL",
				new Message<TestContent> { Content = content, SenderId = "1", Type = TestType.ELECTION_CALL });
		}

		[Test]
		public void RightHandlersAreGettingCalled()
		{
			var content = new TestContent();

			hg.Handle("ELECTION_CALL",
				new Message<TestContent> {Content = content, SenderId = "1", Type = TestType.ELECTION_CALL});
			Assert.AreEqual(0, hg.ElectionCallAnswerCount);
			Assert.AreEqual(1, hg.ElectionCallCount);

			hg.Handle("ELECTION_CALL_ANSWER",
				new Message<TestContent> {Content = content, SenderId = "2", Type = TestType.ELECTION_CALL_ANSWER});
			Assert.AreEqual(1, hg.ElectionCallAnswerCount);
			Assert.AreEqual(1, hg.ElectionCallCount);
		}
	}
}