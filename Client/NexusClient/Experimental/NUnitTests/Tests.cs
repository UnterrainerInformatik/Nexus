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
using System.Linq;
using Microsoft.Xna.Framework;
using NexusClient.Experimental.NUnitTests.Mappings;
using NUnit.Framework;
using Ploeh.SemanticComparison;

namespace NexusClient.Experimental.NUnitTests
{
    [TestFixture]
    public class Tests
    {
        private const int PERFORMANCE_COUNT = 5000000;
        readonly TimerMapping<Timer> timerMapping = new TimerMapping<Timer>(null, null);

        [Test]
        public void TestTimerWrite()
        {
            var t = GetTimer();
            Assert.IsTrue(ToByteArrayMapping(t).SequenceEqual(ToByteArrayManual(t)));
        }

        [Test]
        [Category("Mappers.Performance.Manual")]
        public void PerformanceTestTimerWriteManual()
        {
            var t = GetTimer();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < PERFORMANCE_COUNT; i++)
            {
                ToByteArrayManual(t);
            }
            watch.Stop();
            Console.Out.WriteLine($"Execution time: {watch.ElapsedMilliseconds}ms");
        }

        [Test]
        [Category("Mappers.Performance.DtoStruct")]
        public void PerformanceTestTimerWriteDtoStruct()
        {
            var t = GetTimer();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < PERFORMANCE_COUNT; i++)
            {
                ToByteArrayDtoStruct(t);
            }
            watch.Stop();
            Console.Out.WriteLine($"Execution time: {watch.ElapsedMilliseconds}ms");
        }

        [Test]
        [Category("Mappers.Performance.Mapping")]
        public void PerformanceTestTimerWriteMapping()
        {
            var t = GetTimer();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < PERFORMANCE_COUNT; i++)
            {
                ToByteArrayMapping(t);
            }
            watch.Stop();
            Console.Out.WriteLine($"Execution time: {watch.ElapsedMilliseconds}ms");
        }

        [Test]
        public void TestTimerRead()
        {
            var template = GetTimer();
            var t = new Timer();
            t = FromByteArrayMapping(ToByteArrayManual(template), t);

            var l = new Likeness<Timer, Timer>(template);
            Assert.AreEqual(l, t);
        }

        [Test]
        [Category("Mappers.Performance.Manual")]
        public void PerformanceTestTimerReadManual()
        {
            var template = GetTimer();
            var t = new Timer();
            var bytes = ToByteArrayManual(template);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < PERFORMANCE_COUNT; i++)
            {
                FromByteArrayManual(bytes, t);
            }
            watch.Stop();
            Console.Out.WriteLine($"Execution time: {watch.ElapsedMilliseconds}ms");
        }

        [Test]
        [Category("Mappers.Performance.DtoStruct")]
        public void PerformanceTestTimerReadDtoStruct()
        {
            var template = GetTimer();
            var t = new Timer();
            var bytes = ToByteArrayManual(template);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < PERFORMANCE_COUNT; i++)
            {
                FromByteArrayDtoStruct(bytes, t);
            }
            watch.Stop();
            Console.Out.WriteLine($"Execution time: {watch.ElapsedMilliseconds}ms");
        }

        [Test]
        [Category("Mappers.Performance.Mapping")]
        public void PerformanceTestTimerReadMapping()
        {
            var template = GetTimer();
            var t = new Timer();
            var bytes = ToByteArrayManual(template);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < PERFORMANCE_COUNT; i++)
            {
                FromByteArrayMapping(bytes, t);
            }
            watch.Stop();
            Console.Out.WriteLine($"Execution time: {watch.ElapsedMilliseconds}ms");
        }

        private Timer FromByteArrayManual(byte[] bytes, Timer t)
        {
            using (MemoryStream s = new MemoryStream(bytes))
            {
                using (BinaryReader r = new BinaryReader(s))
                {
                    t.Min = r.ReadSingle();
                    t.Max = r.ReadSingle();
                    t.Value = r.ReadSingle();
                    t.Active = r.ReadBoolean();
                }
                return t;
            }
        }

        private Timer FromByteArrayDtoStruct(byte[] bytes, Timer t)
        {
            using (MemoryStream s = new MemoryStream(bytes))
            {
                using (BinaryReader r = new BinaryReader(s))
                {
                    TimerDtoStruct.From(r).To(t);
                }
                return t;
            }
        }

        private Timer FromByteArrayMapping(byte[] bytes, Timer t)
        {
            using (MemoryStream s = new MemoryStream(bytes))
            {
                using (BinaryReader r = new BinaryReader(s))
                {
                    t = timerMapping.Read(r, null, t);
                }
                return t;
            }
        }

        private byte[] ToByteArrayDtoStruct(Timer t)
        {
            using (MemoryStream s = new MemoryStream())
            {
                using (BinaryWriter w = new BinaryWriter(s))
                {
                    TimerDtoStruct.From(t).To(w);
                }
                s.Flush();
                return s.GetBuffer();
            }
        }

        private byte[] ToByteArrayManual(Timer t)
        {
            using (MemoryStream s = new MemoryStream())
            {
                using (BinaryWriter w = new BinaryWriter(s))
                {
                    w.Write(t.Min);
                    w.Write(t.Max);
                    w.Write(t.Value);
                    w.Write(t.Active);
                }
                s.Flush();
                return s.GetBuffer();
            }
        }

        private byte[] ToByteArrayMapping(Timer t)
        {
            using (MemoryStream s = new MemoryStream())
            {
                using (BinaryWriter w = new BinaryWriter(s))
                {
                    timerMapping.Write(w, null, t);
                }
                s.Flush();
                return s.GetBuffer();
            }
        }

        private Timer GetTimer()
        {
            var t = new Timer();
            t.Min = 0;
            t.Value = 5;
            t.Max = 10;
            t.Active = true;
            return t;
        }

        private Hero GetHero()
        {
            var h = new Hero();
            h.Position = new Vector2(300, 201);
            h.Velocity = 12.5f;
            h.Direction = new Vector2(1, 1);
            h.Direction.Normalize();
            h.Shooting = false;
            h.Running = false;
            h.Building = true;
            return h;
        }
    }
}