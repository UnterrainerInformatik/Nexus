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

using JetBrains.Annotations;
using MessagePack;
using MessagePack.Formatters;
using Microsoft.Xna.Framework;

namespace NexusClient.PerformanceTests.NUnitTests.MessagePackFormatters
{
	[PublicAPI]
	public class PointFormatter : IMessagePackFormatter<Point>
	{
		public void Serialize(ref MessagePackWriter writer, Point value, MessagePackSerializerOptions options)
		{
			writer.Write(value.X);
			writer.Write(value.Y);
		}

		public Point Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
		{
			return reader.TryReadNil() ? Point.Zero : new Point(reader.ReadInt32(), reader.ReadInt32());
		}
	}
}