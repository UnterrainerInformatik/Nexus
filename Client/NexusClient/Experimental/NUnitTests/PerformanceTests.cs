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
using System.Diagnostics;
using System.Text;
using NexusClient.Experimental.NUnitTests.Mappings;
using NexusClient.Experimental.NUnitTests.Objects;
using NexusClient.Experimental.NUnitTests.ZeroFormatters;
using NUnit.Framework;

namespace NexusClient.Experimental.NUnitTests
{
    [TestFixture]
    public class PerformanceTests
    {
        private const int PERFORMANCE_COUNT = 5000000;
        readonly TimerMapping<Timer> timerMapping = new TimerMapping<Timer>(null, null);

        [Test]
        [Category("Mappers.Performance.Manual")]
        public void PerformanceTestTimerWriteManual()
        {
            var t = Helpers.GetTimer();
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < PERFORMANCE_COUNT; i++)
            {
                Helpers.ToByteArrayManual(t);
            }
            watch.Stop();
            Console.Out.WriteLine($"Execution time: {watch.ElapsedMilliseconds}ms");
        }

        [Test]
        [Category("Mappers.Performance.DtoStruct")]
        public void PerformanceTestTimerWriteDtoStruct()
        {
            var t = Helpers.GetTimer();
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < PERFORMANCE_COUNT; i++)
            {
                Helpers.ToByteArrayDtoStruct(t);
            }
            watch.Stop();
            Console.Out.WriteLine($"Execution time: {watch.ElapsedMilliseconds}ms");
        }

        [Test]
        [Category("Mappers.Performance.Mapping")]
        public void PerformanceTestTimerWriteMapping()
        {
            var t = Helpers.GetTimer();
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < PERFORMANCE_COUNT; i++)
            {
                Helpers.ToByteArrayMapping(t, timerMapping);
            }
            watch.Stop();
            Console.Out.WriteLine($"Execution time: {watch.ElapsedMilliseconds}ms");
        }

        [Test]
        [Category("Mappers.Performance.ZeroFormatter")]
        public void PerformanceTestTimerWriteZeroFormatter()
        {
            ZeroFormatterHelpers.Register();
            var t = Helpers.GetTimer();
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < PERFORMANCE_COUNT; i++)
            {
                Helpers.ToByteArrayZeroFormatter(t);
            }
            watch.Stop();
            Console.Out.WriteLine($"Execution time: {watch.ElapsedMilliseconds}ms");
        }

        [Test]
        [Category("Mappers.Performance.Manual")]
        public void PerformanceTestTimerReadManual()
        {
            var template = Helpers.GetTimer();
            var t = new Timer();
            var bytes = Helpers.ToByteArrayManual(template);
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < PERFORMANCE_COUNT; i++)
            {
                Helpers.FromByteArrayManual(bytes, t);
            }
            watch.Stop();
            Console.Out.WriteLine($"Execution time: {watch.ElapsedMilliseconds}ms");
        }

        [Test]
        [Category("Mappers.Performance.DtoStruct")]
        public void PerformanceTestTimerReadDtoStruct()
        {
            var template = Helpers.GetTimer();
            var t = new Timer();
            var bytes = Helpers.ToByteArrayManual(template);
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < PERFORMANCE_COUNT; i++)
            {
                Helpers.FromByteArrayDtoStruct(bytes, t);
            }
            watch.Stop();
            Console.Out.WriteLine($"Execution time: {watch.ElapsedMilliseconds}ms");
        }

        [Test]
        [Category("Mappers.Performance.Mapping")]
        public void PerformanceTestTimerReadMapping()
        {
            var template = Helpers.GetTimer();
            var t = new Timer();
            var bytes = Helpers.ToByteArrayManual(template);
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < PERFORMANCE_COUNT; i++)
            {
                Helpers.FromByteArrayMapping(bytes, t, timerMapping);
            }
            watch.Stop();
            Console.Out.WriteLine($"Execution time: {watch.ElapsedMilliseconds}ms");
        }

        [Test]
        [Category("Mappers.Performance.ZeroFormatter")]
        public void PerformanceTestTimerReadZeroFormatter()
        {
            ZeroFormatterHelpers.Register();
			var bytes = StringToByteArray("94CA00000000CA41200000CA40A00000C30000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
			var watch = Stopwatch.StartNew();
            for (var i = 0; i < PERFORMANCE_COUNT; i++)
            {
                Helpers.FromByteArrayZeroFormatter(bytes);
            }
            watch.Stop();
            Console.Out.WriteLine($"Execution time: {watch.ElapsedMilliseconds}ms");
        }

		private static byte[] StringToByteArray(string hex)
		{
			var NumberChars = hex.Length;
			var bytes = new byte[NumberChars / 2];
			for (var i = 0; i < NumberChars; i += 2)
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			return bytes;
		}
	}
}