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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using NexusClient.Network;
using NexusClient.Network.Interfaces;
using NexusClient.Steam;

namespace NexusClient
{
	[PublicAPI]
	public partial class Client
	{
		private readonly object lockObject = new object();
		private readonly object errorsLockObject = new object();

		private readonly Dictionary<Guid, ConcurrentQueue<P2PError>> connectionErrors =
			new Dictionary<Guid, ConcurrentQueue<P2PError>>();

		private const int WRITE_BUFFER_SIZE = 1024 * 1024 * 8;
		private readonly byte[] writeBuffer = new byte[WRITE_BUFFER_SIZE];
		private readonly MemoryStream writeStream;
		private BinaryWriter writer;

		private const int READ_BUFFER_SIZE = 1024 * 1024 * 8;
		private readonly byte[] readBuffer = new byte[READ_BUFFER_SIZE];
		private MemoryStream readStream;
		private BinaryReader reader;

		private const float BITS_CLEAR_TIMER = 1000f;
		private float bitsClearTimer = BITS_CLEAR_TIMER;

		public long BitsSentLastSecond { get; set; }
		public long BitsReceivedLastSecond { get; set; }
		public long CurrentMessageSize => writeStream.Position;

		public bool ConnectedToServer { get; set; }

		private IConnection Connection { get; set; } = new SteamConnection();
		private INetworking Networking { get; set; } = new SteamNetworking();

		public Client()
		{
			writeStream = new MemoryStream(writeBuffer);
			writer = new BinaryWriter(writeStream);
			readStream = new MemoryStream(readBuffer, 0, READ_BUFFER_SIZE);
			reader = new BinaryReader(readStream);
		}

		public bool Connect(out string userId)
		{
			return Connection.ConnectToServer(out userId);
		}

		public bool Disconnect()
		{
			return Connection.DisconnectFromServer();
		}

		/// <summary>
		///     Gets you the message-writer.
		///     Resets the underlying stream and buffer.
		///     Don't ever call writer.close() since we re-use the underlying stream and the writer.
		///     You don't have to call writer.flush() before you send your message either. That is handled in Send() for you.
		/// </summary>
		public BinaryWriter Create(string messageType)
		{
			// Reset the buffer of the stream.
			writeStream.Position = 0;
			writer.Write(messageType);
			return writer;
		}

		public BinaryWriter Create(Enum messageType)
		{
			return Create(messageType.ToString());
		}

		/// <summary>
		///     Gets you the message-reader for the current message you've retrieved by calling ReadNext().
		///     Resets the underlying stream.
		///     Don't ever call reader.close() since we re-use the underlying stream and the reader.
		/// </summary>
		/// <param name="messageType"></param>
		/// <returns></returns>
		private BinaryReader Read(out string messageType)
		{
			// Reset the buffer on the stream.
			readStream.Position = 0;
			// We don't have to copy our message-buffer to our read-buffer since they are the same thing.

			messageType = reader.ReadString();
			return reader;
		}

		/// <summary>
		///     You should use this method to poll for new messages.
		///     If no more message is waiting, this method will return null.
		/// </summary>
		/// <returns></returns>
		public LowLevelMessage? ReadNext(byte[] buffer)
		{
			if (!ConnectedToServer) return null;

			if (!Networking.IsP2PMessageAvailable(out var messageSize)) return null;

			if (!Networking.ReadP2PMessage(buffer, messageSize, out var _, out var remoteSteamId)) return null;

			var result = new LowLevelMessage();
			result.UserId = remoteSteamId.ToString();
			result.MessageSize = messageSize;
			result.Data = buffer;
			result.Reader = Read(out var t);
			result.MessageType = t;
			BitsReceivedLastSecond += result.MessageSize;
			return result;
		}

		public LowLevelMessage? ReadNext()
		{
			var m = ReadNext(readBuffer);
			if (!m.HasValue)
			{
				return null;
			}

			var result = m.Value;
			result.Reader = Read(out var t);
			result.MessageType = t;
			BitsReceivedLastSecond += result.MessageSize;
			return result;
		}

		public bool Send(Guid userId, SendType sendType)
		{
			writer.Flush();
			BitsSentLastSecond += writeStream.Position;
			return SendP2PMessage(userId, writeBuffer, writeStream.Position, sendType);
		}

		/// <summary>
		///     This opens the P2P session.
		///     That procedure may take very long. Maybe even longer than the timeout, so it's best to re-try a send after you've
		///     received a timeout for a second time.
		/// </summary>
		/// <param name="remoteSteamId">The remote user's identifier.</param>
		/// <param name="data">The data.</param>
		/// <param name="length">The length.</param>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public bool SendP2PMessage(Guid remoteSteamId, byte[] data, long length, SendType type)
		{
			if (!ConnectedToServer) return false;

			var result = Networking.SendP2PMessage(remoteSteamId.ToString(), data, (uint) length, type);

			lock (errorsLockObject)
			{
				if (!connectionErrors.TryGetValue(remoteSteamId, out _))
				{
					connectionErrors.Add(remoteSteamId, new ConcurrentQueue<P2PError>());
				}
			}

			return result;
		}

		public void Update(GameTime gt)
		{
			var elapsed = (float) gt.ElapsedGameTime.TotalMilliseconds;
			bitsClearTimer -= elapsed;
			if (bitsClearTimer <= 0f)
			{
				bitsClearTimer = BITS_CLEAR_TIMER - bitsClearTimer % BITS_CLEAR_TIMER;
				BitsSentLastSecond = 0;
				BitsReceivedLastSecond = 0;
			}

			lock (lockObject)
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
				lock (lockObject)
				{
					foreach (var group in handlerGroups.Values)
					{
						
					}

					ConsolidateHandlerGroups();
					m = ReadNext();
				}
			}
		}
	}
}