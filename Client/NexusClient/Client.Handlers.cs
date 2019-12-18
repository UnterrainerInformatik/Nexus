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
using NexusClient.Network;

namespace NexusClient
{
	public partial class Client
	{
		private readonly Dictionary<object, MessageHandler> registeredHandlers =
			new Dictionary<object, MessageHandler>();

		private readonly Dictionary<object, MessageHandler> addList = new Dictionary<object, MessageHandler>();

		private readonly Dictionary<object, MessageHandler> readAfterRemovalList =
			new Dictionary<object, MessageHandler>();

		private readonly List<object> removeList = new List<object>();
		private readonly List<object> removeAndDisposeList = new List<object>();

		private void ConsolidateHandlers()
		{
			lock (lockObject)
			{
				foreach (var item in addList)
				{
					if (!removeList.Contains(item.Key) && !removeAndDisposeList.Contains(item.Key))
					{
						registeredHandlers.Add(item.Key, item.Value);
					}
					else
					{
						readAfterRemovalList.Add(item.Key, item.Value);
					}
				}

				foreach (var key in removeAndDisposeList)
				{
					var m = registeredHandlers[key];
					registeredHandlers.Remove(key);
				}

				foreach (var key in removeList)
				{
					registeredHandlers.Remove(key);
				}

				foreach (var item in readAfterRemovalList)
				{
					registeredHandlers.Add(item.Key, item.Value);
				}

				removeList.Clear();
				removeAndDisposeList.Clear();
				readAfterRemovalList.Clear();
				addList.Clear();
			}
		}

		public object RegisterOrOverwriteHandler(MessageHandler handler, object key = null)
		{
			lock (lockObject)
			{
				if (key == null)
				{
					key = new object();
				}

				if (registeredHandlers.ContainsKey(key))
				{
					removeList.Add(key);
				}

				addList.Add(key, handler);
				return key;
			}
		}

		public bool UnregisterHandler(object key)
		{
			lock (lockObject)
			{
				if (!registeredHandlers.TryGetValue(key, out _)) return false;
				removeList.Add(key);
				return true;
			}
		}

		public bool UnregisterAndDisposeHandler(object key)
		{
			lock (lockObject)
			{
				if (!registeredHandlers.TryGetValue(key, out _)) return false;
				removeAndDisposeList.Add(key);
				return true;
			}
		}

		public MessageHandler GetHandler(object key)
		{
			lock (lockObject)
			{
				registeredHandlers.TryGetValue(key, out var handler);
				return handler;
			}
		}

		public bool ActivateHandler(object key)
		{
			lock (lockObject)
			{
				return ModifyHandlerActive(key, true);
			}
		}

		public bool DeactivateHandler(object key)
		{
			lock (lockObject)
			{
				return ModifyHandlerActive(key, false);
			}
		}

		public void ClearHandlers()
		{
			lock (lockObject)
			{
				registeredHandlers.Clear();
			}
		}

		private bool ModifyHandlerActive(object key, bool isActive)
		{
			lock (lockObject)
			{
				if (!registeredHandlers.TryGetValue(key, out var handler)) return false;
				handler.IsActive = isActive;
				return true;
			}
		}
	}
}