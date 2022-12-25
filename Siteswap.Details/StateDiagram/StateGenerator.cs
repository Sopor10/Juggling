using System.Linq;

namespace Siteswap.Details.StateDiagram;

public static class StateGenerator
{
    public static State CalculateState(params int[] siteswap)
    {
        var stable = false;

        var state = State.Empty(siteswap.Max());

        while (stable is false)
        {
            var previousState = state;
            state = siteswap.Aggregate(state, (current, value) => current.Advance().Throw(value));

            if (state == previousState) stable = true;
        }

        return state;
    }
}