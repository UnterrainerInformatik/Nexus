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
using System.Linq;
using NexusClient.PerformanceTests.Mappings;
using NexusClient.PerformanceTests.NUnitTests.Mappings;
using NexusClient.PerformanceTests.NUnitTests.Objects;
using NUnit.Framework;

namespace NexusClient.PerformanceTests.NUnitTests
{
	[TestFixture]
	[Category("Mappers.Mapping")]
	public class TestsMapping
	{
		private readonly TimerMapping<Objects.Timer> timerMapping = new TimerMapping<Objects.Timer>(null, null);
		private readonly HeroMapping<Hero> heroMapping = new HeroMapping<Hero>(null, null);
		private readonly LevelMapping<Level> levelMapping = new LevelMapping<Level>(null, null);

		[Test]
		public void TestTimerWrite()
		{
			var t = Helpers.GetTimer();
			Assert.IsTrue(Helpers.ToByteArrayMapping(t, timerMapping)
				.SequenceEqual(Helpers.ToByteArrayManual(t)));
		}

		[Test]
		public void TestTimerRead()
		{
			var template = Helpers.GetTimer();
			var t = new Objects.Timer();
			t = Helpers.FromByteArrayMapping(Helpers.ToByteArrayManual(template), t, timerMapping);

			Assert.IsTrue(Helpers.Equals(template, t));
		}

		[Test]
		public void TestHeroWrite()
		{
			var h = Helpers.GetHero();
			Assert.IsTrue(Helpers.ToByteArrayMapping(h, heroMapping)
				.SequenceEqual(Helpers.ToByteArrayManual(h)));
		}

		[Test]
		public void TestHeroRead()
		{
			var template = Helpers.GetHero();
			var h = new Hero();
			h.Timer = new Objects.Timer();

			h = Helpers.FromByteArrayMapping(Helpers.ToByteArrayManual(template), h, heroMapping);

			Assert.IsTrue(Helpers.Equals(template, h));
		}

		[Test]
		public void TestLevelWrite()
		{
			var l = Helpers.GetLevel();
			Assert.IsTrue(Helpers.ToByteArrayMapping(l, levelMapping)
				.SequenceEqual(Helpers.ToByteArrayManual(l)));
		}

		[Test]
		public void TestLevelRead()
		{
			var template = Helpers.GetLevel();
			var l = new Level();
			l.Hero = new Hero();
			l.Hero.Timer = new Objects.Timer();

			l = Helpers.FromByteArrayMapping(Helpers.ToByteArrayManual(template), l, levelMapping);

			Assert.IsTrue(Helpers.Equals(template, l));
		}

		[Test]
		public void TestIntListWrite()
		{
			var mapping = new ListMapping<int, List<int>>(null, null);
			var l = new List<int>(new[] {34, 5, 67, 252});

			Assert.IsTrue(Helpers.ToByteArrayMapping(l, mapping)
				.SequenceEqual(Helpers.ToByteArrayManual(l)));
		}

		[Test]
		public void TestIntListRead()
		{
			var mapping = new ListMapping<int, List<int>>(null, null);
			var template = new List<int>(new[] {34, 5, 67, 252});
			var l = new List<int>();

			l = Helpers.FromByteArrayMapping(Helpers.ToByteArrayManual(template), l, mapping);

			Assert.IsTrue(template.SequenceEqual(l));
		}
	}
}