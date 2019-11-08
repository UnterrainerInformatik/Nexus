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
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace NexusClient.Experimental.Mappings
{
    [PublicAPI]
    public class ListMapping<TItems, T> : Mapping<List<TItems>, T>
    {
        public ListMapping(Func<T, List<TItems>> load, Func<List<TItems>, T, T> save) : base(load, save)
        {
        }

        protected override List<TItems> From(BinaryReader reader, T instance, List<TItems> field)
        {
            field.Clear();
            var c = reader.ReadInt32();
            var t = typeof(TItems);
            for (var i = 0; i < c; i++)
            {
                field.Add((TItems) Read(reader, t));
            }
            return field;
        }

        protected override void To(BinaryWriter writer, T instance, List<TItems> field)
        {
            writer.Write(field.Count);
            foreach (var item in field)
            {
                Write(writer, item);
            }
        }

        private object Read(BinaryReader r, Type t)
        {
            if (t == typeof(ushort))
            {
                return r.ReadUInt16();
            }
            if (t == typeof(short))
            {
                return r.ReadInt16();
            }
            if (t == typeof(bool))
            {
                return r.ReadBoolean();
            }
            if (t == typeof(byte))
            {
                return r.ReadByte();
            }
            if (t == typeof(char))
            {
                return r.ReadChar();
            }
            if (t == typeof(decimal))
            {
                return r.ReadDecimal();
            }
            if (t == typeof(double))
            {
                return r.ReadDouble();
            }
            if (t == typeof(float))
            {
                return r.ReadSingle();
            }
            if (t == typeof(int))
            {
                return r.ReadInt32();
            }
            if (t == typeof(long))
            {
                return r.ReadInt64();
            }
            if (t == typeof(sbyte))
            {
                return r.ReadSByte();
            }
            if (t == typeof(string))
            {
                return r.ReadString();
            }
            if (t == typeof(uint))
            {
                return r.ReadUInt32();
            }
            if (t == typeof(ulong))
            {
                return r.ReadUInt64();
            }
            return null;
        }

        private void Write(BinaryWriter w, object o)
        {
            var t = o.GetType();
            if (t == typeof(ushort))
            {
                w.Write((ushort) o);
            }
            else if (t == typeof(short))
            {
                w.Write((short) o);
            }
            else if (t == typeof(bool))
            {
                w.Write((bool) o);
            }
            else if (t == typeof(byte))
            {
                w.Write((byte) o);
            }
            else if (t == typeof(char))
            {
                w.Write((char) o);
            }
            else if (t == typeof(decimal))
            {
                w.Write((decimal) o);
            }
            else if (t == typeof(double))
            {
                w.Write((double) o);
            }
            else if (t == typeof(float))
            {
                w.Write((float) o);
            }
            else if (t == typeof(int))
            {
                w.Write((int) o);
            }
            else if (t == typeof(long))
            {
                w.Write((long) o);
            }
            else if (t == typeof(sbyte))
            {
                w.Write((sbyte) o);
            }
            else if (t == typeof(string))
            {
                w.Write((string) o);
            }
            else if (t == typeof(uint))
            {
                w.Write((uint) o);
            }
            else if (t == typeof(ulong))
            {
                w.Write((ulong) o);
            }
        }
    }
}