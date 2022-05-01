using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Siteswap.Details.StateDiagram.Graph;

namespace Siteswap.Details.StateDiagram;

/// <summary>
/// true indicates an object is scheduled to land on this timeslot.
/// false is therefore a free slot.
/// </summary>
/// <param name="Value"></param>
[DebuggerDisplay("{DebuggerDisplay()}")]
public record State(ImmutableArray<bool> Value)
{
    public IEnumerable<Edge<State, int>> Transitions()
    {
        if (Value[0])
        {
            for (var i = 1; i < Value.Length; i++)
            {
                if (!Value[i])
                {
                    yield return new (this, OneBeatLater().Fill(i - 1), i);
                }
            }
        }
        else
        {
            yield return new (this, OneBeatLater(), 0);
        }
    }

    private State Fill(int i)
    {
        return new(Value.SetItem(i, true));
    }

    private State OneBeatLater()
    {
        var immutableArray = Value.Add(false).Skip(1).Take(Value.Length).ToImmutableArray();
        return new State(immutableArray);
    }

    public State(params int[] values):this(values.Select(x => x != 0).ToImmutableArray())
    {
    }

    public State(IEnumerable<bool> values):this(values.ToImmutableArray())
    {
    }

    public override string ToString()
    {
        return $"{string.Join("", Value.Select(x => x?1:0))}";
    }

    private string DebuggerDisplay() => ToString();

}