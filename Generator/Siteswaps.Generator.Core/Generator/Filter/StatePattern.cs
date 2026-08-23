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
    private readonly uint occupiedMask = CreateMask(Items, StateValue.Occupied);
    private readonly uint freeMask = CreateMask(Items, StateValue.Free);

    public bool Matches(State state)
    {
        return Matches(state.Value);
    }

    public bool Matches(uint stateValue)
    {
        return (stateValue & occupiedMask) == occupiedMask && (stateValue & freeMask) == 0;
    }

    private static uint CreateMask(ImmutableArray<StateValue> items, StateValue value)
    {
        uint mask = 0;
        for (var index = 0; index < items.Length; index++)
        {
            if (items[index] == value)
                mask |= 1u << index;
        }

        return mask;
    }
}
