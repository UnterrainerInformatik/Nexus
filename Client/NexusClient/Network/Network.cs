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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using NexusClient.Network.Apis;
using NexusClient.Network.Interfaces;
using Serilog;

namespace NexusClient.Network
{
	public partial class Network<TConv, TSer, TDes, T> where TConv : ITransport<T>
		where TSer : IMessageSer<T>
		where TDes : IMessageDes<T>
		where T : IMessageDto
	{
		public string UserId { get; set; }

		internal object LockObject = new object();
		internal TargetApi<TConv, TSer, TDes, T> Message { get; }
		internal TConv Converter { get; set; }
		internal readonly Dictionary<string, string> Participants = new Dictionary<string, string>();
		internal ITransport Transport { get; set; }

		private readonly Dictionary<object, HandlerGroup<TConv, TSer, TDes, T>> handlerGroups =
			new Dictionary<object, HandlerGroup<TConv, TSer, TDes, T>>();

		private readonly Dictionary<object, HandlerGroup<TConv, TSer, TDes, T>> addList =
			new Dictionary<object, HandlerGroup<TConv, TSer, TDes, T>>();

		private readonly Dictionary<object, HandlerGroup<TConv, TSer, TDes, T>> addAfterRemovingList =
			new Dictionary<object, HandlerGroup<TConv, TSer, TDes, T>>();

		private readonly List<object> removeList = new List<object>();

		public Network(ITransport transport, TConv converter)
		{
			Logger.Init();
			Converter = converter;
			Transport = transport;
			Message = new TargetApi<TConv, TSer, TDes, T>(this);

			writeStream = new MemoryStream(writeBuffer);
			writer = new BinaryWriter(writeStream);
			readStream = new MemoryStream(readBuffer, 0, READ_BUFFER_SIZE);
			reader = new BinaryReader(readStream);
		}

		public void Initialize()
		{
			UserId = Transport.Login();
			Log.Debug($"[{UserId}] Initialize");
		}

		public Network<TConv, TSer, TDes, T> AddParticipants(params string[] userIds)
		{
			Log.Debug($"[{UserId}] AddParticipants [{string.Join(",", userIds)}]");
			foreach (var id in userIds) Participants.Add(id, id);
			return this;
		}

		public Network<TConv, TSer, TDes, T> RemoveParticipants(params string[] userIds)
		{
			Log.Debug($"[{UserId}] RemoveParticipants [{string.Join(",", userIds)}]");
			foreach (var id in userIds) Participants.Remove(id);
			return this;
		}
	}
}