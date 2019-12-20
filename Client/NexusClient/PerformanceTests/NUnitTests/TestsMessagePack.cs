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

using MessagePack;
using NexusClient.PerformanceTests.NUnitTests.MessagePackFormatters;
using NexusClient.PerformanceTests.NUnitTests.Objects;
using NUnit.Framework;

namespace NexusClient.PerformanceTests.NUnitTests
{
	[TestFixture]
	[Category("Mappers.ZeroFormatter")]
	public class TestsMessagePack
	{
		[SetUp]
		public void Setup()
		{
			MessagePackHelpers.Register();
		}

		[Test]
		public void TestTimer()
		{
			var template = Helpers.GetTimer();

			var b = MessagePackSerializer.Serialize(template);
			var t = MessagePackSerializer.Deserialize<Objects.Timer>(b);

			Assert.IsTrue(Helpers.Equals(template, t));
		}

		[Test]
		public void TestHero()
		{
			var template = Helpers.GetHero();

			var b = MessagePackSerializer.Serialize(template);
			var h = MessagePackSerializer.Deserialize<Hero>(b);

			Assert.IsTrue(Helpers.Equals(template, h));
		}

		[Test]
		public void TestLevel()
		{
			var template = Helpers.GetLevel();

			var b = MessagePackSerializer.Serialize(template);
			var l = MessagePackSerializer.Deserialize<Level>(b);

			Assert.IsTrue(Helpers.Equals(template, l));
		}
	}
}