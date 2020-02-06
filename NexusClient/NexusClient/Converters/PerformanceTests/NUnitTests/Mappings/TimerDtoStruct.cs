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

using System.IO;

namespace NexusClient.Converters.PerformanceTests.NUnitTests.Mappings
{
	public struct TimerDtoStruct
	{
		public float Value { get; }
		public float Max { get; }
		public float Min { get; }

		public bool Active { get; }

		public TimerDtoStruct(float min, float max, float value, bool active)
		{
			Value = value;
			Max = max;
			Min = min;
			Active = active;
		}

		/// <summary>
		///     The benefit of this method is that you may implement <see cref="From(NexusClient.Converters.PerformanceTests.NUnitTests.Objects.Timer)" />
		///     and <see cref="To(NexusClient.Converters.PerformanceTests.NUnitTests.Objects.Timer)" /> directly within your
		///     object you want to transfer, so you may contain private fields as well without having to expose them.
		/// </summary>
		public static TimerDtoStruct From(Objects.Timer t)
		{
			return new TimerDtoStruct(min: t.Min, max: t.Max, value: t.Value, active: t.Active);
		}

		/// <summary>
		///     The benefit of this method is that you may implement <see cref="From(NexusClient.Converters.PerformanceTests.NUnitTests.Objects.Timer)" />
		///     and <see cref="To(NexusClient.Converters.PerformanceTests.NUnitTests.Objects.Timer)" /> directly within your
		///     object you want to transfer, so you may contain private fields as well without having to expose them.
		/// </summary>
		public void To(Objects.Timer t)
		{
			t.Min = Min;
			t.Max = Max;
			t.Value = Value;
			t.Active = Active;
		}

		public static TimerDtoStruct From(BinaryReader r)
		{
			return new TimerDtoStruct(min: r.ReadSingle(), max: r.ReadSingle(), value: r.ReadSingle(),
				active: r.ReadBoolean());
		}

		public void To(BinaryWriter w)
		{
			w.Write(Min);
			w.Write(Max);
			w.Write(Value);
			w.Write(Active);
		}
	}
}