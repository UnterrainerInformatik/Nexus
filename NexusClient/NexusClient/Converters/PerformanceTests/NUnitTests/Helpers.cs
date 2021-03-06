﻿// ***************************************************************************
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
using System.IO;
using JetBrains.Annotations;
using MessagePack;
using Microsoft.Xna.Framework;
using NexusClient.Converters.PerformanceTests.Mappings;
using NexusClient.Converters.PerformanceTests.NUnitTests.Mappings;
using NexusClient.Converters.PerformanceTests.NUnitTests.Objects;

namespace NexusClient.Converters.PerformanceTests.NUnitTests
{
	[PublicAPI]
	public static class Helpers
	{
		public static Objects.Timer FromByteArrayManual(byte[] bytes, Objects.Timer t)
		{
			using (var s = new MemoryStream(bytes))
			{
				using (var r = new BinaryReader(s))
				{
					t.Min = r.ReadSingle();
					t.Max = r.ReadSingle();
					t.Value = r.ReadSingle();
					t.Active = r.ReadBoolean();
				}

				return t;
			}
		}

		public static Objects.Timer FromByteArrayDtoStruct(byte[] bytes, Objects.Timer t)
		{
			using (var s = new MemoryStream(bytes))
			{
				using (var r = new BinaryReader(s))
				{
					TimerDtoStruct.From(r).To(t);
				}

				return t;
			}
		}

		public static Objects.Timer FromByteArrayMapping(byte[] bytes, Objects.Timer t, TimerMapping<Objects.Timer> mapping)
		{
			using (var s = new MemoryStream(bytes))
			{
				using (var r = new BinaryReader(s))
				{
					t = mapping.ReadFrom(r, t);
				}

				return t;
			}
		}

		public static Objects.Timer FromByteArrayZeroFormatter(byte[] bytes)
		{
			using (var s = new MemoryStream(bytes, 0, bytes.Length, true, true))
			{
				return MessagePackSerializer.Deserialize<Objects.Timer>(s);
			}
		}

		public static byte[] ToByteArrayDtoStruct(Objects.Timer t)
		{
			using (var s = new MemoryStream())
			{
				using (var w = new BinaryWriter(s))
				{
					TimerDtoStruct.From(t).To(w);
				}

				s.Flush();
				return s.GetBuffer();
			}
		}

		public static byte[] ToByteArrayManual(Objects.Timer t)
		{
			using (var s = new MemoryStream())
			{
				using (var w = new BinaryWriter(s))
				{
					To(w, t);
				}

				s.Flush();
				return s.GetBuffer();
			}
		}

		private static void To(BinaryWriter w, Objects.Timer t)
		{
			w.Write(t.Min);
			w.Write(t.Max);
			w.Write(t.Value);
			w.Write(t.Active);
		}

		public static byte[] ToByteArrayMapping(Objects.Timer t, TimerMapping<Objects.Timer> mapping)
		{
			using (var s = new MemoryStream())
			{
				using (var w = new BinaryWriter(s))
				{
					mapping.WriteTo(w, t);
				}

				s.Flush();
				return s.GetBuffer();
			}
		}

		public static byte[] ToByteArrayZeroFormatter(Objects.Timer t)
		{
			using (var s = new MemoryStream())
			{
				MessagePackSerializer.Serialize(s, t);
				s.Flush();
				return s.GetBuffer();
			}
		}

		public static Hero FromByteArrayMapping(byte[] bytes, Hero h, HeroMapping<Hero> mapping)
		{
			using (var s = new MemoryStream(bytes))
			{
				using (var r = new BinaryReader(s))
				{
					h = mapping.ReadFrom(r, h);
				}

				return h;
			}
		}

		public static byte[] ToByteArrayManual(Hero h)
		{
			using (var s = new MemoryStream())
			{
				using (var w = new BinaryWriter(s))
				{
					To(w, h);
				}

				s.Flush();
				return s.GetBuffer();
			}
		}

		private static void To(BinaryWriter w, Hero h)
		{
			w.Write(h.Position.X);
			w.Write(h.Position.Y);
			w.Write(h.Velocity);
			w.Write(h.Direction.X);
			w.Write(h.Direction.Y);
			w.Write(h.Shooting);
			w.Write(h.Running);
			w.Write(h.Building);
			To(w, h.Timer);
		}

		public static Level FromByteArrayMapping(byte[] bytes, Level l, LevelMapping<Level> mapping)
		{
			using (var s = new MemoryStream(bytes))
			{
				using (var r = new BinaryReader(s))
				{
					l = mapping.ReadFrom(r, l);
				}

				return l;
			}
		}

		public static byte[] ToByteArrayManual(Level l)
		{
			using (var s = new MemoryStream())
			{
				using (var w = new BinaryWriter(s))
				{
					To(w, l);
				}

				s.Flush();
				return s.GetBuffer();
			}
		}

		public static byte[] ToByteArrayManual(List<int> l)
		{
			using (var s = new MemoryStream())
			{
				using (var w = new BinaryWriter(s))
				{
					w.Write(l.Count);
					foreach (var i in l)
					{
						w.Write(i);
					}
				}

				s.Flush();
				return s.GetBuffer();
			}
		}

		public static void To(BinaryWriter w, Level l)
		{
			w.Write(l.Number);
			To(w, l.Hero);
		}

		public static byte[] ToByteArrayMapping(Hero h, HeroMapping<Hero> mapping)
		{
			using (var s = new MemoryStream())
			{
				using (var w = new BinaryWriter(s))
				{
					mapping.WriteTo(w, h);
				}

				s.Flush();
				return s.GetBuffer();
			}
		}

		public static List<int> FromByteArrayMapping(byte[] bytes, List<int> l, ListMapping<int, List<int>> mapping)
		{
			using (var s = new MemoryStream(bytes))
			{
				using (var r = new BinaryReader(s))
				{
					l = mapping.ReadFrom(r, l);
				}

				return l;
			}
		}

		public static byte[] ToByteArrayMapping(List<int> l, ListMapping<int, List<int>> mapping)
		{
			using (var s = new MemoryStream())
			{
				using (var w = new BinaryWriter(s))
				{
					mapping.WriteTo(w, l);
				}

				s.Flush();
				return s.GetBuffer();
			}
		}

		public static byte[] ToByteArrayMapping(Level l, LevelMapping<Level> mapping)
		{
			using (var s = new MemoryStream())
			{
				using (var w = new BinaryWriter(s))
				{
					mapping.WriteTo(w, l);
				}

				s.Flush();
				return s.GetBuffer();
			}
		}

		public static Objects.Timer GetTimer()
		{
			var t = new Objects.Timer();
			t.Min = 0;
			t.Value = 5;
			t.Max = 10;
			t.Active = true;
			return t;
		}

		public static bool Equals(Objects.Timer a, Objects.Timer b)
		{
			return Equals(a.Min, b.Min) && Equals(a.Max, b.Max) && Equals(a.Value, b.Value) &&
					Equals(a.Active, b.Active);
		}

		public static Hero GetHero()
		{
			var h = new Hero();
			h.Position = new Vector2(300, 201);
			h.Velocity = 12.5f;
			h.Direction = new Vector2(1, 1);
			h.Direction.Normalize();
			h.Shooting = false;
			h.Running = false;
			h.Building = true;
			h.Timer = GetTimer();
			return h;
		}

		public static bool Equals(Hero a, Hero b)
		{
			return Equals(a.Position, b.Position) && Equals(a.Velocity, b.Velocity) &&
					Equals(a.Direction, b.Direction) &&
					Equals(a.Shooting, b.Shooting) && Equals(a.Running, b.Running) && Equals(a.Building, b.Building) &&
					Equals(a.Timer, b.Timer);
		}

		public static Level GetLevel()
		{
			var l = new Level();
			l.Number = 3;
			l.Hero = GetHero();
			return l;
		}

		public static bool Equals(Level a, Level b)
		{
			return Equals(a.Number, b.Number) && Equals(a.Hero, b.Hero);
		}
	}
}