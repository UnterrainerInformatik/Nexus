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

namespace NexusClient.PerformanceTests
{
	public abstract class Mapping<TField, TObject> : BinarySerializable<TObject>
	{
		public Func<TField, TObject, TObject> Save { get; }
		public Func<TObject, TField> Load { get; }

		private List<BinarySerializable<TField>> Mappings { get; } = new List<BinarySerializable<TField>>();

		protected Mapping(Func<TObject, TField> load, Func<TField, TObject, TObject> save)
		{
			Load = load;
			Save = save;
		}

		protected void Add(BinarySerializable<TField> m)
		{
			Mappings.Add(m);
		}

		public TObject ReadFrom(BinaryReader reader, object obj)
		{
			return ReadFrom(reader, default(TObject), obj);
		}

		public void WriteTo(BinaryWriter writer, object obj)
		{
			WriteTo(writer, default(TObject), obj);
		}

		public virtual TObject ReadFrom(BinaryReader reader, TObject obj, object parent)
		{
			TField f;
			if (parent != null)
			{
				f = (TField) parent;
			}
			else
			{
				if (Load == null)
				{
					f = default(TField);
				}
				else
				{
					f = Load(obj);
				}
			}

			f = From(reader, obj, f);
			if (Save == null)
			{
				return (TObject) parent;
			}

			if (obj == null)
			{
				return default(TObject);
			}

			return Save(f, obj);
		}

		public virtual void WriteTo(BinaryWriter writer, TObject obj, object parent)
		{
			TField f;
			if (Load == null)
			{
				f = (TField) parent;
			}
			else
			{
				if (obj != null)
				{
					f = Load(obj);
				}
				else
				{
					f = default(TField);
				}
			}

			To(writer, obj, f);
		}

		protected virtual TField From(BinaryReader reader, TObject instance, TField field)
		{
			foreach (var t in Mappings)
			{
				t.ReadFrom(reader, field, null);
			}

			return field;
		}

		protected virtual void To(BinaryWriter writer, TObject instance, TField field)
		{
			foreach (var t in Mappings)
			{
				t.WriteTo(writer, field, null);
			}
		}
	}
}