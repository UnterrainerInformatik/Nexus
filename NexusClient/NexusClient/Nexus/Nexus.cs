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

using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using NexusClient.Converters;
using NexusClient.HandlerGroups;
using NexusClient.Interfaces;
using NexusClient.Nexus.Apis;
using NexusClient.Utils;
using Serilog;

namespace NexusClient.Nexus
{
	public abstract partial class Nexus<TCnv, TSer, TDes, TDto> where TCnv : IConverter<TDto>
		where TSer : IMessageSer<TDto>
		where TDes : IMessageDes<TDto>
		where TDto : IMessageDto
	{
		public string UserId { get; set; }

		internal object LockObject = new object();
		public TargetApi<TCnv, TSer, TDes, TDto> Message { get; }
		internal TCnv Converter { get; set; }
		internal ITransport Transport { get; set; }

		private readonly Dictionary<object, HandlerGroup<TCnv, TSer, TDes, TDto>> handlerGroups =
			new Dictionary<object, HandlerGroup<TCnv, TSer, TDes, TDto>>();

		private readonly Dictionary<object, HandlerGroup<TCnv, TSer, TDes, TDto>> addList =
			new Dictionary<object, HandlerGroup<TCnv, TSer, TDes, TDto>>();

		private readonly Dictionary<object, HandlerGroup<TCnv, TSer, TDes, TDto>> addAfterRemovingList =
			new Dictionary<object, HandlerGroup<TCnv, TSer, TDes, TDto>>();

		private readonly List<object> removeList = new List<object>();

		protected Nexus(ITransport transport, TCnv converter)
		{
			Logger.Init();
			Converter = converter;
			Transport = transport;
			Message = new TargetApi<TCnv, TSer, TDes, TDto>(this);

			writeStream = new MemoryStream(writeBuffer);
			writer = new BinaryWriter(writeStream);
			readStream = new MemoryStream(readBuffer, 0, READ_BUFFER_SIZE);
			reader = new BinaryReader(readStream);
		}

		public void Initialize()
		{
			UserId = Transport.Login();
			Log.Verbose($"[{UserId}] Initialize");
		}

		public void Update(GameTime gt)
		{
			UpdatePerformanceCounters(gt);

			lock (LockObject)
			{
				foreach (var handler in handlerGroups.Values) handler.Update(gt);
				ConsolidateHandlerGroups();
				HandleMessages();
			}
		}
	}
}