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
using System.IO;
using NexusClient.Network.Implementations.MessagePack;
using NUnit.Framework;

namespace NexusClient.Network.NUnitTests
{
	public enum TestType
	{
		ELECTION_CALL,
		ELECTION_CALL_ANSWER
	}

	public struct TestContent : MessagePackDto
	{
		public string TestField { get; set; }
	}

	public class TestHandlerGroup : HandlerGroup
	{
		public TestHandlerGroup()
		{
			AddHandler<TestContent>(TestType.ELECTION_CALL, ElectionCallReceived);
			AddHandler<TestContent>(TestType.ELECTION_CALL_ANSWER, ElectionCallAnswerReceived);
		}

		private void ElectionCallReceived(Message<TestContent> message)
		{
			Console.Out.WriteLine($"Election-call message handled. TestField: [{message.Content.TestField}]");
		}

		private void ElectionCallAnswerReceived(Message<TestContent> message)
		{
			Console.Out.WriteLine($"Election-call-answer message handled. TestField: [{message.Content.TestField}]");
		}
	}

	[TestFixture()]
	public class HandlerGroupTests
	{
		[Test]
		public void AddAndGetHandler()
		{
			var content = new TestContent();
			var msg = new Message<TestContent>();
			msg.Content = content;
			msg.SenderId = "1";
			msg.Type = TestType.ELECTION_CALL;

			var hg = new TestHandlerGroup();

			hg.Handle("ELECTION_CALL", msg);
		}
	}
}