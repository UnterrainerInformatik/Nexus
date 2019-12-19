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
using NexusClient.Network.Interfaces;

namespace NexusClient.Network.Apis
{
	public struct MessageApi<TConv, TSer, TDes, T>
		where TSer : IMessageSer<T>
		where TDes : IMessageDes<T>
		where TConv : ITransport<T>
	{
		internal string Sender { get; set; }
		internal string[] Recipients { get; set; }
		internal SendType TransportSendType { get; set; }
		internal Enum MessageType { get; set; }
		internal T Content { get; set; }

		private TargetApi<TConv, TSer, TDes, T> TargetApi { get; }

		public static MessageApi<TConv, TSer, TDes, T> Create()
		{
			return new MessageApi<TConv, TSer, TDes, T>
			{
				Content = default(T),
				TransportSendType = Network.SendType.RELIABLE
			};
		}

		public MessageApi<TConv, TSer, TDes, T> WithSendType(SendType type)
		{
			TransportSendType = type;
			return this;
		}

		public MessageApi<TConv, TSer, TDes, T> WithContent(Enum messageType, T data)
		{
			MessageType = messageType;
			Content = data;
			return this;
		}

		public void Send()
		{
			TargetApi.Send(this);
		}
	}
}