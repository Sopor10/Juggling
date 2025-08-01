using System.Diagnostics;
using Siteswap.Details.StateDiagram.Graph;

namespace Siteswap.Details.StateDiagram;

/// <summary>
/// true indicates an object is scheduled to land on this timeslot.
/// false is therefore a free slot.
/// </summary>
/// <param name="Value"></param>
[DebuggerDisplay("{StateRepresentation()}")]
public record State(uint Value)
{
    public State(params int[] values)
        : this(values.Select(x => x != 0)) { }

    public State(IEnumerable<bool> values)
        : this(
            (uint)
                values
                    .Select((b, i) => (b, i))
                    .Where(x => x.b)
                    .Select(x => (int)Math.Pow(2, x.i))
                    .Sum()
        ) { }

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
        var advance = this with { Value = Value >> 1 };
        return advance;
    }

    public State Throw(int i)
    {
        if (i == 0)
        {
            return this;
        }
        var state = new State(Value: Value | (uint)(1 << (i - 1)));
        return state;
    }

    public IEnumerable<Edge<State, int>> Transitions(int maxHeight)
    {
        if (IsBitSet(Value, 0))
        {
            for (var i = 1; i <= maxHeight; i++)
            {
                if (IsBitSet(Value, i) is false)
                {
                    yield return new(this, Advance().Throw(i), i);
                }
            }
        }
        else
        {
            yield return new(this, Advance(), 0);
        }
    }

    private bool IsBitSet(uint b, int pos)
    {
        return (b & (1 << pos)) != 0;
    }
}
