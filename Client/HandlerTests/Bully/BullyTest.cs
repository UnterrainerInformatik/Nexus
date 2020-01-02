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

using NexusClient.HandlerGroups.Bully;
using NexusClient.NUnitTests.Infrastructure;
using NUnit.Framework;
using Serilog;

namespace HandlerTests.Bully
{
	[TestFixture]
	class BullyTest : HandlerTestBase
	{
		[SetUp]
		public void Setup()
		{
			BullyHandlerGroup handler = null;
			Initialize(20, (clientName, handlerDictionary) =>
			{
				handler = new BullyHandlerGroup();
				handlerDictionary.Add(handler);
			});
		}

		public void Iterate(int numberOfIterations)
		{
			for (var i = 0; i < numberOfIterations; i++)
			{
				DebugLogGameTime();
				PrintElectionInProgress();
				PrintLeaders();
				Update();
				AdvanceFrame();
			}
		}

		[Test]
		public void WhenCallerIsLowestIdCallerWinsTest()
		{
			clients["user1"].Handlers.Get<BullyHandlerGroup>().StartBullyElection();
			Iterate(5);
			AdvanceMillis(5000);
			Iterate(2);
		}

		[Test]
		public void WhenCallerIsSecondLowestIdCallerGetsUpdateTest()
		{
			clients["user2"].Handlers.Get<BullyHandlerGroup>().StartBullyElection();
			Iterate(5);
			AdvanceMillis(5000);
			Iterate(2);
		}

		[Test]
		public void WhenCallerIsHighestIdCallerGetsUpdateTest()
		{
			clients["user9"].Handlers.Get<BullyHandlerGroup>().StartBullyElection();
			Iterate(10);
			AdvanceMillis(5000);
			Iterate(2);
		}

		private void PrintLeaders()
		{
			var s = "### Leaders: [ ";
			foreach (var nexusObject in clients.Values)
			{
				if (!"user1".Equals(nexusObject.UserId)) s += " | ";
				s += nexusObject.Handlers.Get<BullyHandlerGroup>().LeaderId ?? "null";
			}

			s += " ] ###";
			Log.Debug(s);
		}

		private void PrintElectionInProgress()
		{
			var s = "### In-progress: [ ";
			foreach (var nexusObject in clients.Values)
			{
				if (!"user1".Equals(nexusObject.UserId)) s += " | ";
				if (nexusObject.Handlers.Get<BullyHandlerGroup>().ElectionInProgress) s += "yes";
				else s += "no ";
			}

			s += " ] ###";
			Log.Debug(s);
		}
	}
}