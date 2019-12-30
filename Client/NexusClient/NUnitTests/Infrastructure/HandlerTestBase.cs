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
using System.Linq;
using Microsoft.Xna.Framework;
using NexusClient.Converters.MessagePack;
using NexusClient.HandlerGroups;
using NexusClient.Network.Testing;
using NexusClient.Nexus.Implementations;
using NexusClient.Utils;
using Serilog;

namespace NexusClient.NUnitTests.Infrastructure
{
	public class DictionaryByType
	{
		private readonly
			IDictionary<Type, HandlerGroup<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto>>
			dictionary =
				new Dictionary<Type, HandlerGroup<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto>
				>();

		public void Add<T>(T value)
			where T : HandlerGroup<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto>
		{
			dictionary.Add(typeof(T), value);
		}

		public ICollection<Type> Keys => dictionary.Keys;

		public ICollection<HandlerGroup<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto>> Values =>
			dictionary.Values;

		public void Put<T>(T value)
			where T : HandlerGroup<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto>
		{
			dictionary[typeof(T)] = value;
		}

		public T Get<T>() where T : HandlerGroup<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto>
		{
			return (T) dictionary[typeof(T)];
		}

		public bool TryGet<T>(out T value)
			where T : HandlerGroup<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto>
		{
			if (dictionary.TryGetValue(typeof(T), out var tmp))
			{
				value = (T) tmp;
				return true;
			}

			value = default(T);
			return false;
		}
	}

	public class NexusClient
	{
		public MessagePackNexus Nexus { get; }
		private TestTransport Transport { get; }

		public DictionaryByType Handlers = new DictionaryByType();

		public readonly string UserId;

		public NexusClient(TestServer server, MessagePackConverter converter)
		{
			Transport = new TestTransport(server);
			Nexus = new MessagePackNexus(Transport, converter);
			Nexus.Initialize();
			UserId = Nexus.UserId;
		}
	}

	public delegate void AddHandlersToClient(string clientName, DictionaryByType handlerDictionary);

	public class HandlerTestBase
	{
		protected TestServer server;
		private TestGameTime gameTime;
		protected Dictionary<string, NexusClient> clients;
		protected MessagePackConverter converter;

		protected virtual void Initialize(int numberOfClients, AddHandlersToClient addHandlersToClientDelegate)
		{
			Logger.Init();
			gameTime = new TestGameTime();
			server = new TestServer();
			converter = new MessagePackConverter();
			clients = new Dictionary<string, NexusClient>();
			for (var i = 0; i < numberOfClients; i++)
			{
				var nexusObjects = new NexusClient(server, converter);
				addHandlersToClientDelegate.Invoke(nexusObjects.UserId, nexusObjects.Handlers);
				foreach (var handler in nexusObjects.Handlers.Values)
					nexusObjects.Nexus.RegisterOrOverwriteHandlerGroup(handler);
				nexusObjects.Nexus.Update(gameTime.Value());
				clients.Add(nexusObjects.UserId, nexusObjects);
			}

			foreach (var nexusObjects in clients.Values) nexusObjects.Nexus.AddParticipants(clients.Keys.ToArray());
		}

		protected GameTime AdvanceFrame()
		{
			return gameTime.AdvanceFrame();
		}

		protected GameTime AdvanceFrames(int numberOfFrames)
		{
			return gameTime.AdvanceFrames(numberOfFrames);
		}

		protected GameTime AdvanceMillis(double milliseconds)
		{
			return gameTime.AdvanceMillis(milliseconds);
		}

		protected GameTime GameTime()
		{
			return gameTime.Value();
		}

		protected virtual void DebugLogGameTime()
		{
			Log.Debug(
				$"--- gameTime = {gameTime.Value().TotalGameTime:c} ----------------------------------------------------------");
		}

		protected virtual void Update()
		{
			foreach (var nexusObject in clients.Values) nexusObject.Nexus.Update(gameTime.Value());
			server.Update(gameTime.Value());
		}
	}
}