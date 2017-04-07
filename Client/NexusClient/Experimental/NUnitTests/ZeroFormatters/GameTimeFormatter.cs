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
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using ZeroFormatter;
using ZeroFormatter.Formatters;
using ZeroFormatter.Internal;

namespace NexusClient.Experimental.NUnitTests.ZeroFormatters
{
    [PublicAPI]
    public class GameTimeFormatter<TTypeResolver> : Formatter<TTypeResolver, GameTime>
        where TTypeResolver : ITypeResolver, new()
    {
        private const int BYTE_SIZE = 16;

        public override int? GetLength()
        {
            // If size is variable, return null.
            return BYTE_SIZE;
        }

        public override int Serialize(ref byte[] bytes, int offset, GameTime value)
        {
            // Formatter<T> can get child serializer.
            BinaryUtil.WriteDouble(ref bytes, offset, value.ElapsedGameTime.TotalMilliseconds);
            offset += 8;
            BinaryUtil.WriteDouble(ref bytes, offset, value.TotalGameTime.TotalMilliseconds);
            return BYTE_SIZE;
        }

        public override GameTime Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = BYTE_SIZE;
            return new GameTime(TimeSpan.FromMilliseconds(BinaryUtil.ReadDouble(ref bytes, offset)),
                TimeSpan.FromMilliseconds(BinaryUtil.ReadDouble(ref bytes, offset + 8)));
        }
    }
}