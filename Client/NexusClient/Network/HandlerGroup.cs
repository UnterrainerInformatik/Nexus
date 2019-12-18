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
using Microsoft.Xna.Framework;

namespace NexusClient.Network
{
	public abstract class HandlerGroup
	{
		protected  object LockObject = new object();
		protected delegate void HandleMessageDelegate(MessageApi messageApi);
		protected readonly Dictionary<Enum, HandleMessageDelegate> MessageHandlers =
			new Dictionary<Enum, HandleMessageDelegate>();

		protected HandlerGroup(bool active = true)
		{
			Active = active;
		}

		public bool Active { get; set; }

		/// <summary>
		///     Handles the given message by searching the dictionary for the key of the message and then calling the delegate.
		///     Returns true, if it found a message and called the delegate, false otherwise.
		///     Also sets the IsHandled property of the message to true if it has called a delegate.
		/// </summary>
		/// <param name="messageApi">The given message</param>
		/// <returns>True or false.</returns>
		public bool Handle(MessageApi messageApi)
		{
			if (!Active) return false;

			MessageHandlers.TryGetValue(messageApi.Type, out var func);
			if (func == null) return false;

			func(messageApi);
			messageApi.Handled = true;
			return true;
		}

		public virtual void Update(GameTime gt)
		{
		}
	}
}