using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Siteswap.Details.StateDiagram.Graph;
using MoreLinq;

namespace Siteswap.Details.StateDiagram;

using System.Collections;

/// <summary>
/// true indicates an object is scheduled to land on this timeslot.
/// false is therefore a free slot.
/// </summary>
/// <param name="Value"></param>
[DebuggerDisplay("{StateRepresentation()}")]
public record State(uint Value)
    {
        public State(params int[] values):this(values.Select(x => x != 0))
        {
        }

        public State(IEnumerable<bool> values) : 
            this(
                (uint)values.Select((b, i) => (b, i))
                    .Where(x => x.b)
                    .Select(x => (int)Math.Pow(2, x.i))
                    .Sum())
        {
        }

        public IEnumerable<bool> Positions
        {
            get
            {
                var mask = new BitArray(BitConverter.GetBytes(this.Value));
                var foundTrue = false;
                for (var index = mask.Count - 1; index >= 0; index--)
                {
                    if (mask[index])
                    {
                        foundTrue = true;
                    }

                    if (foundTrue)
                    {
                        yield return mask[index];
                    }
                }
            }
        }

        public static State Empty()
        {
            return new((uint)0);
        }

        public static State GroundState(int numberOfBalls)
        {
            var mask = 0xffffffff;
            mask >>= 32 - numberOfBalls;
            mask <<= 0;
            return new State(mask);
        }

        public string StateRepresentation()
        {
            return string.Concat(Convert.ToString(Value, 2).Reverse().ToArray());
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
        
        public IEnumerable<Edge<State, int>> Transitions(int maxHeight)
        {
            if (IsBitSet(Value,0))
            {
                for (var i = 1; i <= maxHeight; i++)
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
