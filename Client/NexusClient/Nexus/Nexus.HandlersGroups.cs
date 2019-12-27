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

using NexusClient.HandlerGroups;

namespace NexusClient.Nexus
{
	public partial class Nexus<TConv, TSer, TDes, T>
	{
		private void ConsolidateHandlerGroups()
		{
			lock (LockObject)
			{
				foreach (var item in addList)
				{
					if (!removeList.Contains(item.Key))
					{
						item.Value.Initialize(this);
						handlerGroups.Add(item.Key, item.Value);
					}
					else
					{
						addAfterRemovingList.Add(item.Key, item.Value);
					}
				}

				foreach (var key in removeList)
				{
					if (handlerGroups.TryGetValue(key, out var hg))
						hg.Nexus = null;
					handlerGroups.Remove(key);
				}

				foreach (var item in addAfterRemovingList)
				{
					item.Value.Initialize(this);
					handlerGroups.Add(item.Key, item.Value);
				}

				removeList.Clear();
				addAfterRemovingList.Clear();
				addList.Clear();
			}
		}

		public object RegisterOrOverwriteHandlerGroup(HandlerGroup<TConv, TSer, TDes, T> handlerGroup,
			object key = null)
		{
			lock (LockObject)
			{
				if (key == null)
				{
					key = new object();
				}

				if (handlerGroups.ContainsKey(key))
				{
					removeList.Add(key);
				}

				addList.Add(key, handlerGroup);
				return key;
			}
		}

		public bool UnregisterHandlerGroup(object key)
		{
			lock (LockObject)
			{
				if (!handlerGroups.TryGetValue(key, out _)) return false;
				removeList.Add(key);
				return true;
			}
		}

		public HandlerGroup<TConv, TSer, TDes, T> GetHandler(object key)
		{
			lock (LockObject)
			{
				handlerGroups.TryGetValue(key, out var handler);
				return handler;
			}
		}

		public bool ActivateHandler(object key)
		{
			lock (LockObject)
			{
				return ModifyActive(key, true);
			}
		}

		public bool DeactivateHandler(object key)
		{
			lock (LockObject)
			{
				return ModifyActive(key, false);
			}
		}

		public void ClearHandlers()
		{
			lock (LockObject)
			{
				foreach (var hg in handlerGroups.Values)
				{
					hg.Nexus = null;
				}

				handlerGroups.Clear();
			}
		}

		private bool ModifyActive(object key, bool isActive)
		{
			lock (LockObject)
			{
				if (!handlerGroups.TryGetValue(key, out var handler)) return false;
				handler.Active = isActive;
				return true;
			}
		}
	}
}