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

using Microsoft.Xna.Framework;
using NexusClient.NUnitTests.Infrastructure;
using NexusClient.Utils;
using NUnit.Framework;
using Serilog;

namespace HandlerTests
{
	[TestFixture]
	class TestHandlerTests : HandlerTestBase
	{
		[SetUp]
		public void Setup()
		{
			Initialize(2, (clientName, handlerDictionary) => { handlerDictionary.Add(new TestHandlerGroup(server)); });
		}

		[Test]
		public void AddingHandlersWorksTest()
		{
			var gt = new TestGameTime();
			Update(gt.Value());

			Log.Debug(
				$"--- t = {gt.Value().TotalGameTime} seconds----------------------------------------------------------");
			gt.Advance(1);
			Update(gt.Value());
		}

		private void Update(GameTime gt)
		{
			foreach (var nexusObject in clients.Values) nexusObject.Nexus.Update(gt);
		}
	}
}