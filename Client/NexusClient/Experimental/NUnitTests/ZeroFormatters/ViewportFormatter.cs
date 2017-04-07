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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ZeroFormatter;
using ZeroFormatter.Formatters;

namespace NexusClient.Experimental.NUnitTests.ZeroFormatters
{
    [PublicAPI]
    public class ViewportFormatter<TTypeResolver> : Formatter<TTypeResolver, Viewport>
        where TTypeResolver : ITypeResolver, new()
    {
        public override int? GetLength()
        {
            return Formatter<TTypeResolver, Rectangle>.Default.GetLength();
        }

        public override int Serialize(ref byte[] bytes, int offset, Viewport value)
        {
            // Formatter<T> can get child serializer.
            return Formatter<TTypeResolver, Rectangle>.Default.Serialize(ref bytes, offset,
                new Rectangle(value.X, value.Y, value.Width, value.Height));
        }

        public override Viewport Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            return
                new Viewport(Formatter<TTypeResolver, Rectangle>.Default.Deserialize(ref bytes, offset, tracker,
                    out byteSize));
        }
    }
}