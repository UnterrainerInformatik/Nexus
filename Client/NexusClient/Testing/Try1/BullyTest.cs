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
using System.IO;
using NUnit.Framework;
using Serilog;
using StateMachine;

namespace NexusClient.Testing.Try1
{
	enum State
	{
		IDLE,
		CLOSED
	}

	enum Trigger
	{
		OPEN,
		CLOSE
	}

	[TestFixture]
	class BullyTest
	{
		[Test]
		public void Test()
		{
			var m = Fsm<State, Trigger>.Builder(State.NO)
				.State(State.OPENED)
				.TransitionTo(State.OPENED).On(Trigger.OPEN)
				.TransitionTo(State.CLOSED).On(Trigger.CLOSE)
				.State(State.CLOSED).ClearsStack()
				.TransitionTo(State.CLOSED).On(Trigger.CLOSE)
				.TransitionTo(State.OPENED).On(Trigger.OPEN)
				.After(TimeSpan.FromSeconds(1)).TransitionTo()
				.Build();

			m.Update(TimeSpan.FromSeconds(1));
		}

		private void SetupLogging(string[] args)
		{
			var logFile =
				Path.Combine(
					SubFolder(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NexusClient"),
					"debug.log");
			
			var debugFlag = false;
			foreach (var t in args)
			{
				var a = t.ToLower().Trim();
				if (a.Equals("-debug") || a.Equals("debug"))
				{
					debugFlag = true;
				}
			}

			var config = new LoggerConfiguration();
#if DEBUG
			debugFlag = true;
			config.WriteTo.Console();
#endif
			if (debugFlag)
			{
				config.MinimumLevel.Debug();
			}
			else
			{
				config.MinimumLevel.Error();
			}
			config.WriteTo.File(logFile);
				Log.Logger = config.CreateLogger();
		}

		/// <summary>
		///     Gets you the string for the specified sub-directory in the given folder.
		///     Creates it, if it does not exist.
		/// </summary>
		/// <param name="targetPath">The target path.</param>
		/// <param name="subDir">The sub dir.</param>
		/// <returns></returns>
		private static string SubFolder(string targetPath, string subDir)
		{
			var result = Path.Combine(targetPath, subDir + "\\");
			if (!Directory.Exists(result))
			{
				Directory.CreateDirectory(result);
			}

			return result;
		}
	}
}