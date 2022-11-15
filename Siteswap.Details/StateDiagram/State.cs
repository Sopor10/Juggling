using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Siteswap.Details.StateDiagram.Graph;
using MoreLinq;

namespace Siteswap.Details.StateDiagram;

/// <summary>
/// true indicates an object is scheduled to land on this timeslot.
/// false is therefore a free slot.
/// </summary>
/// <param name="Value"></param>
[DebuggerDisplay("{StateRepresentation()}")]
public record State(uint Value, int Length)
    {
        public State(params int[] values):this(values.Select(x => x != 0))
        {
        }

        public State(IEnumerable<bool> values) : 
            this(
                (uint)values.Select((b, i) => (b, i))
                    .Where(x => x.b)
                    .Select(x => (int)Math.Pow(2, x.i))
                    .Sum(), 
                values.Count())
        {
        }
        
        public static State Empty(int length)
        {
            return new((uint)0, length);
        }

        public static State GroundState(int numberOfBalls, int length)
        {
            if (length < numberOfBalls) throw new ArgumentException();

            var mask = 0xffffffff;
            mask >>= 32 - numberOfBalls;
            mask <<= 0;
            return new State(mask, length);
        }

        public string StateRepresentation()
        {
            var repeat = new string(Enumerable.Repeat('0', Length - Convert.ToString(Value, 2).Length).ToArray());
            return string.Concat(Convert.ToString(Value, 2).Reverse().ToArray()) +
                   repeat;
        }

        public override string ToString() => StateRepresentation();

        public State Advance()
        {
            var advance = this with
            {
                Value = Value >> 1
            };
            return advance;
        }

        public State Throw(int i)
        {
            var state = this with
            {
                Value = Value | (uint)(1 << (i - 1))
            };
            return state;
        }
        
        public IEnumerable<Edge<State, int>> Transitions()
        {
            if (IsBitSet(Value,0))
            {
                for (var i = 1; i < Length; i++)
                {
                    if (IsBitSet(Value,i) is false)
                    {
                        yield return new (this, Advance().Throw(i), i);
                    }
                }
            }
            else
            {
                yield return new (this, Advance(), 0);
            }
        }
        
        private bool IsBitSet(uint b, int pos)
        {
           return (b & (1 << pos)) != 0;
        }
    }