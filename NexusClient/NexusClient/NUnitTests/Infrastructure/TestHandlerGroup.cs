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

using NexusClient.Converters.MessagePack;
using NexusClient.HandlerGroups;
using NexusClient.Network.Testing;
using Serilog;

namespace NexusClient.NUnitTests.Infrastructure
{
	public class TestHandlerGroup : HandlerGroup<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto>
	{
		public TestServer Server { get; }
		public int ElectionCallCount { get; private set; }
		public int ElectionCallAnswerCount { get; private set; }
		
		public TestHandlerGroup(TestServer server)
		{
			Server = server;
			AddHandler<TestMessage>(TestMessageType.ELECTION_CALL, ElectionCallReceived);
			AddHandler<TestMessage>(TestMessageType.ELECTION_CALL_ANSWER, ElectionCallAnswerReceived);
		}

		private void ElectionCallReceived(TestMessage message, string senderId)
		{
			ElectionCallCount++;
			Log.Debug(
				$"[{Nexus.UserId}]: Election-call message from [{senderId}] handled. TestField: [{message.TestField}]");
		}

		private void ElectionCallAnswerReceived(TestMessage message, string senderId)
		{
			ElectionCallAnswerCount++;
			Log.Debug(
				$"{Nexus.UserId}]: Election-call-answer message from [{senderId}] handled. TestField: [{message.TestField}]");
		}
	}
}