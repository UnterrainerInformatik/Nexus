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

namespace NexusClient.Experimental
{
    public abstract class Mapping<TField, TObject> : BinarySerializable<TObject>
    {
        public Func<TField, TObject, TObject> WriteDelegate { get; }
        public Func<TObject, TField> ReadDelegate { get; }

        private List<BinarySerializable<TField>> Mappings { get; } = new List<BinarySerializable<TField>>();

        protected Mapping(Func<TObject, TField> readDelegate, Func<TField, TObject, TObject> writeDelegate)
        {
            ReadDelegate = readDelegate;
            WriteDelegate = writeDelegate;
        }

        protected void Add(BinarySerializable<TField> m)
        {
            Mappings.Add(m);
        }
        
        public virtual TObject Read(BinaryReader reader, TObject instance, object field)
        {
            var f = From(reader, instance, (TField)(field??default(TField)));
            if (WriteDelegate == null)
            {
                return (TObject)field;
            }
            return WriteDelegate(f, instance);
        }

        public virtual void Write(BinaryWriter writer, TObject instance, object field)
        {
            TField f;
            if (ReadDelegate == null)
            {
                f = (TField)field;
            }
            else
            {
                f = ReadDelegate(instance);
            }
            To(writer, instance, f);
        }

        protected virtual TField From(BinaryReader reader, TObject instance, TField field)
        {
            foreach (BinarySerializable<TField> t in Mappings)
            {
                t.Read(reader, field, null);
            }
            return field;
        }

        protected virtual void To(BinaryWriter writer, TObject instance, TField field)
        {
            foreach (BinarySerializable<TField> t in Mappings)
            {
                t.Write(writer, field, null);
            }
        }
    }
}