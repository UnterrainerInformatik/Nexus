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

namespace NexusClient.Testing.Try1
{
	public struct Message
	{
		public string Sender;
		public string[] Recepients;

		public uint MessageSize;
		public byte[] Data;

		public SendType Type;
		public BinaryReader Reader;

		public bool Handled;

		public static Message GetDefault()
		{
			return new Message
			{
				Sender = null,
				Recepients = new string[] { },
				MessageSize = 0,
				Data = new byte[] { },
				Type = NexusClient.SendType.RELIABLE,

				Reader = null,
				Handled = false
			};
		}

		public Message SendType(SendType type)
		{
			Type = type;
			return this;
		}

		public Message WithContent(byte[] data)
		{
			Data = data;
			return this;
		}

		public void Send()
		{
		}
	}
}