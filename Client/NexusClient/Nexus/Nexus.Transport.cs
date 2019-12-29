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
using System.IO;

namespace NexusClient.Nexus
{
	public partial class Nexus<TConv, TSer, TDes, T>
	{
		private const int WRITE_BUFFER_SIZE = 1024 * 1024 * 8;
		private readonly byte[] writeBuffer = new byte[WRITE_BUFFER_SIZE];
		private readonly MemoryStream writeStream;
		private readonly BinaryWriter writer;

		private const int READ_BUFFER_SIZE = 1024 * 1024 * 8;
		private readonly byte[] readBuffer = new byte[READ_BUFFER_SIZE];
		private readonly MemoryStream readStream;
		private readonly BinaryReader reader;

		public LowLevelMessage? ReadNext()
		{
			if (!Transport.IsP2PMessageAvailable(out var messageSize)) return null;

			if (!Transport.ReadP2PMessage(readBuffer, messageSize, out _, out var senderId)) return null;

			readStream.Position = 0;
			var t = reader.ReadString();
			BitsReceivedLastSecond += messageSize;

			return new LowLevelMessage
			{
				UserId = senderId, MessageSize = messageSize, Stream = readStream, MessageType = t
			};
		}

		public void Send<TObject>(Enum messageType, TObject content, SendType sendType, IEnumerable<string> recipients)
			where TObject : T
		{
			writeStream.Position = 0;
			writer.Write(messageType.ToString());
			Converter.WriteMessage(writeStream, content, out _);
			writeStream.Flush();
			var length = writeStream.Position;

			foreach (var recipient in recipients)
			{
				Transport.SendP2PMessage(recipient, writeBuffer, (uint) length, sendType);
				BitsSentLastSecond += length;
			}
		}

		private void HandleMessages()
		{
			var m = ReadNext();
			while (m.HasValue)
				lock (LockObject)
				{
					foreach (var group in handlerGroups.Values)
						if (group.Handle(m.Value.MessageType, m.Value))
							break;

					ConsolidateHandlerGroups();
					m = ReadNext();
				}
		}
	}
}