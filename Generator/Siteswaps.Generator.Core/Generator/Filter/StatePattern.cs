using System.Collections.Immutable;

namespace Siteswaps.Generator.Core.Generator.Filter;

public enum StateValue
{
    Free,
    Occupied,
    DontCare,
}

/// <summary>
/// A partially specified siteswap state. DontCare leaves a position unconstrained.
/// </summary>
public record StatePattern(ImmutableArray<StateValue> Items)
{
    public bool Matches(State state)
    {
        for (var index = 0; index < Items.Length; index++)
        {
            var isOccupied = state.IsOccupiedAt(index);
            if (
                Items[index] is StateValue.Occupied && !isOccupied
                || Items[index] is StateValue.Free && isOccupied
            )
            {
                return false;
            }
        }

        return true;
    }
}
