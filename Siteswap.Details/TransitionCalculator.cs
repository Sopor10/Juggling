using System.Collections.Immutable;
using Siteswap.Details.StateDiagram;

namespace Siteswap.Details;

public static class TransitionCalculator
{
    public static List<Transition> CreateTransitions(
        Siteswap from,
        Siteswap to,
        int length,
        int? maxHeight = null
    )
    {
        maxHeight ??= new[]
        {
            from.Items.EnumerateValues(1).Max(),
            to.Items.EnumerateValues(1).Max(),
        }.Max();
        var result = new List<ImmutableList<Throw>>();

        var fromState = from.State;
        var toState = to.State;

        if (fromState == toState)
        {
            return [new Transition(from, to, [])];
        }

        if (length > 0)
        {
            result.AddRange(
                Recurse(fromState, toState, ImmutableList<Throw>.Empty, length, maxHeight.Value)
            );
        }

        return result
            .Select(x => new Transition(from, to, x.ToArray()))
            .ToList();
    }

    private static IEnumerable<ImmutableList<Throw>> Recurse(
        State fromState,
        State toState,
        ImmutableList<Throw> transitionSoFar,
        int length,
        int maxHeight
    )
    {
        foreach (var transition in fromState.Transitions(maxHeight))
        {
            if (toState == transition.N2)
            {
                yield return transitionSoFar.Add(
                    new Throw(transition.N1, transition.N2, transition.Data)
                );
            }

            if (length == 1)
                continue;

            foreach (
                var immutableList in Recurse(
                    transition.N2,
                    toState,
                    transitionSoFar.Add(new Throw(transition.N1, transition.N2, transition.Data)),
                    length - 1,
                    maxHeight
                )
            )
            {
                yield return immutableList;
            }
        }
    }
}
