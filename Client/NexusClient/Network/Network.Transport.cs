using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace NexusClient.Network
{
	public partial class Network<TConv, TSer, TDes, T>
	{
		private const int WRITE_BUFFER_SIZE = 1024 * 1024 * 8;
		private readonly byte[] writeBuffer = new byte[WRITE_BUFFER_SIZE];
		private readonly MemoryStream writeStream;
		private BinaryWriter writer;

		private const int READ_BUFFER_SIZE = 1024 * 1024 * 8;
		private readonly byte[] readBuffer = new byte[READ_BUFFER_SIZE];
		private MemoryStream readStream;
		private BinaryReader reader;

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

			if (!Networking.ReadP2PMessage(readBuffer, messageSize, out var _, out var remoteSteamId)) return null;

			var result = new LowLevelMessage();
			result.UserId = remoteSteamId.ToString();
			result.MessageSize = messageSize;
			result.Data = readBuffer;
			result.Reader = GetReaderFor(out var t);
			result.MessageType = t;
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
					T message = Converter.ReadMessage(m.Value.Data, m.Value.MessageSize);
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
