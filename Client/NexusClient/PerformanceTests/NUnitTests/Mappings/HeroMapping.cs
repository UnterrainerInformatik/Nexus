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
using NexusClient.PerformanceTests.Mappings;
using NexusClient.PerformanceTests.NUnitTests.Objects;

namespace NexusClient.PerformanceTests.NUnitTests.Mappings
{
	public class HeroMapping<TParent> : Mapping<Hero, TParent>
	{
		public HeroMapping(Func<TParent, Hero> load, Func<Hero, TParent, TParent> save) : base(load, save)
		{
			Add(new Vector2Mapping<Hero>(o => o.Position, (v, o) =>
			{
				o.Position = v;
				return o;
			}));
			Add(new FloatMapping<Hero>(o => o.Velocity, (v, o) =>
			{
				o.Velocity = v;
				return o;
			}));
			Add(new Vector2Mapping<Hero>(o => o.Direction, (v, o) =>
			{
				o.Direction = v;
				return o;
			}));
			Add(new BoolMapping<Hero>(o => o.Shooting, (v, o) =>
			{
				o.Shooting = v;
				return o;
			}));
			Add(new BoolMapping<Hero>(o => o.Running, (v, o) =>
			{
				o.Running = v;
				return o;
			}));
			Add(new BoolMapping<Hero>(o => o.Building, (v, o) =>
			{
				o.Building = v;
				return o;
			}));
			Add(new TimerMapping<Hero>(o => o.Timer, (v, o) =>
			{
				o.Timer = v;
				return o;
			}));
		}
	}
}