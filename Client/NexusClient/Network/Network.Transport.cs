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
using Microsoft.Xna.Framework;

namespace NexusClient.Network
{
	public partial class Network<TTrans, TSer, TDes, T>
	{
		private const int WRITE_BUFFER_SIZE = 1024 * 1024 * 8;
		private readonly byte[] writeBuffer = new byte[WRITE_BUFFER_SIZE];
		private readonly MemoryStream writeStream;
		private readonly BinaryWriter writer;

		private const int READ_BUFFER_SIZE = 1024 * 1024 * 8;
		private readonly byte[] readBuffer = new byte[READ_BUFFER_SIZE];
		private readonly MemoryStream readStream;
		private readonly BinaryReader reader;

		public BinaryWriter GetWriterFor(Enum messageType)
		{
			writeStream.Position = 0;
			writer.Write(messageType.ToString());
			return writer;
		}

		private BinaryReader GetReaderFor(out string messageType)
		{
			readStream.Position = 0;
			messageType = reader.ReadString();
			return reader;
		}

		public LowLevelMessage? ReadNext()
		{
			if (!Networking.IsP2PMessageAvailable(out var messageSize)) return null;

			if (!Networking.ReadP2PMessage(readBuffer, messageSize, out _, out var remoteSteamId)) return null;

			var result = new LowLevelMessage
			{
				UserId = remoteSteamId.ToString(),
				MessageSize = messageSize,
				Data = readBuffer,
				Reader = GetReaderFor(out var t),
				MessageType = t
			};
			return result;
		}

		public void Update(GameTime gt)
		{
			lock (LockObject)
			{
				foreach (var handler in handlerGroups.Values)
				{
					handler.Update(gt);
				}

				ConsolidateHandlerGroups();
				HandleMessages();
			}
		}

		private void HandleMessages()
		{
			var m = ReadNext();
			while (m.HasValue)
			{
				lock (LockObject)
				{
					var message = Transport.ReadMessage(m.Value.Data, m.Value.MessageSize);
					foreach (var group in handlerGroups.Values)
					{
						if (group.Handle(m.Value.MessageType, message))
							break;
					}

					ConsolidateHandlerGroups();
					m = ReadNext();
				}
			}
		}
	}
}