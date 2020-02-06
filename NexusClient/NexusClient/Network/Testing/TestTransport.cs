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

using NexusClient.Interfaces;
using NexusClient.Nexus;

namespace NexusClient.Network.Testing
{
	public class TestTransport : ITransport
	{
		public TestServer Server { get; set; }
		public string UserId { get; set; }

		public TestTransport(TestServer server)
		{
			Server = server;
		}

		public string Login()
		{
			UserId = Server.Login();
			return UserId;
		}

		public void Logout()
		{
			Server.Logout(UserId);
			UserId = null;
		}

		public bool IsP2PMessageAvailable(out uint messageSize)
		{
			var r = Server.IsMessageAvailableFor(UserId, out var size);
			messageSize = size;
			return r;
		}

		public bool ReadP2PMessage(byte[] buffer, uint messageSize, out uint bytesRead, out string senderId)
		{
			bytesRead = 0;
			senderId = null;
			if (!Server.ReadMessageFor(UserId, out var m))
				return false;
			senderId = m.SenderId;
			if (buffer.Length < m.Size)
				return false;
			m.Buffer.CopyTo(buffer, 0);
			bytesRead = m.Size;
			return true;
		}

		public bool SendP2PMessage(string recipientId, byte[] data, uint length, SendType sendType)
		{
			return Server.SendMessageFor(UserId, recipientId, data, length);
		}
	}
}