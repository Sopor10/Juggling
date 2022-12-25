using System.Linq;

namespace Siteswap.Details.StateDiagram;

public abstract class StateGenerator
{
    public static State CalculateState(int[] siteswap, int? length = null)
    {
        var stable = false;

        var state = State.Empty(new[]{siteswap.Max(), length??0}.Max());

        while (stable is false)
        {
            var previousState = state;
            state = siteswap.Aggregate(state, (current, value) => current.Advance().Throw(value));

            if (state == previousState) stable = true;
        }

        return state;
    }

    public static State CalculateState(Siteswap siteswap, int maxHeight)
    {
        return CalculateState(siteswap.Items.EnumerateValues(1).ToArray(), maxHeight);
    }
}