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
using NexusClient.Interfaces;
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

        private const int WRITE_BUFFER_SIZE = 1024*1024*8;
        private readonly byte[] writeBuffer = new byte[WRITE_BUFFER_SIZE];
        private readonly MemoryStream writeStream;
        private BinaryWriter writer;

        private const int READ_BUFFER_SIZE = 1024*1024*8;
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

        public bool Connect(out Guid userId)
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
        public BinaryWriter Create(ushort messageType)
        {
            // Reset the buffer of the stream.
            writeStream.Position = 0;
            writer.Write(messageType);
            return writer;
        }

        public BinaryWriter Create(Enum messageType)
        {
            var t = messageType.GetType();
            if (!t.IsDefined(typeof(FlagsAttribute), false) || t.GetEnumUnderlyingType() != typeof(ushort))
            {
                throw new ArgumentException(
                    "The parameter 'messageType' has to be a Flag-Enumeration of underlying-messageType ushort.");
            }

            var o = Convert.ChangeType(messageType, TypeCode.UInt16);
            if (o == null)
            {
                throw new ArgumentException("The parameter 'messageType' is not convertible to ushort (UInt16).");
            }

            return Create((ushort) o);
        }

        /// <summary>
        ///     Gets you the message-reader for the current message you've retrieved by calling ReadNext().
        ///     Resets the underlying stream.
        ///     Don't ever call reader.close() since we re-use the underlying stream and the reader.
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        private BinaryReader Read(out ushort messageType)
        {
            // Reset the buffer on the stream.
            readStream.Position = 0;
            // We don't have to copy our message-buffer to our read-buffer since they are the same thing.

            messageType = reader.ReadUInt16();
            return reader;
        }
        
        /// <summary>
        ///     You should use this method to poll for new messages.
        ///     If no more message is waiting, this method will return null.
        /// </summary>
        /// <returns></returns>
        public Message? ReadNext(byte[] buffer)
        {
            if (ConnectedToServer)
            {
                uint messageSize;
                if (Networking.IsP2PMessageAvailable(out messageSize))
                {
                    Guid remoteSteamId;
                    uint bytesRead;
                    if (Networking.ReadP2PMessage(buffer, messageSize, out bytesRead, out remoteSteamId))
                    {
                        var result = new Message();
                        result.RemoteUserId = remoteSteamId;
                        result.MessageSize = messageSize;
                        result.Data = buffer;
                        ushort t;
                        result.Reader = Read(out t);
                        result.MessageType = t;
                        BitsReceivedLastSecond += result.MessageSize;
                        return result;
                    }
                }
            }
            return null;
        }

        public Message? ReadNext()
        {
            var m = ReadNext(readBuffer);
            if (!m.HasValue)
            {
                return null;
            }

            var result = m.Value;
            ushort t;
            result.Reader = Read(out t);
            result.MessageType = t;
            BitsReceivedLastSecond += result.MessageSize;
            return result;
        }

        public bool Send(Guid userId, P2PSendType sendType)
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
        public bool SendP2PMessage(Guid remoteSteamId, byte[] data, long length, P2PSendType type)
        {
            var result = false;
            if (ConnectedToServer)
            {
                result = Networking.SendP2PMessage(remoteSteamId, data, (uint)length, type);
                
                lock (errorsLockObject)
                {
                    ConcurrentQueue<P2PError> errors;
                    if (!connectionErrors.TryGetValue(remoteSteamId, out errors))
                    {
                        connectionErrors.Add(remoteSteamId, new ConcurrentQueue<P2PError>());
                    }
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
                bitsClearTimer = BITS_CLEAR_TIMER - bitsClearTimer%BITS_CLEAR_TIMER;
                BitsSentLastSecond = 0;
                BitsReceivedLastSecond = 0;
            }

            lock (lockObject)
            {
                foreach (var handler in registeredHandlers.Values)
                {
                    handler.Update(gt);
                }
                ConsolidateHandlers();
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
                    foreach (var handler in registeredHandlers.Values)
                    {
                        if (m.Value.Handled)
                        {
                            break;
                        }
                        handler.Handle(m.Value);
                    }
                    ConsolidateHandlers();
                    m = ReadNext();
                }
            }
        }
    }
}