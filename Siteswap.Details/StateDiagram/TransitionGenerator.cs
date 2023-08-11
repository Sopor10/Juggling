using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Siteswap.Details.StateDiagram;

public static class TransitionGenerator
{
    public static List<Transition> CreateTransitions(Siteswap from, Siteswap to, int length)
    {
        var maxHeight = new[] { from.Items.EnumerateValues(1).Max(), to.Items.EnumerateValues(1).Max()}.Max();
        var result = new List<ImmutableList<int>>();

        var fromState = StateGenerator.CalculateState(from, maxHeight);
        var toState = StateGenerator.CalculateState(to, maxHeight);

        result.AddRange(Recurse(fromState, toState, ImmutableList<int>.Empty, length, maxHeight));

        return result.Select(x => new Transition(from,to, x.ToArray())).ToList();
    }

    private static IEnumerable<ImmutableList<int>> Recurse(State fromState, State toState,
        ImmutableList<int> transitionSoFar, int length, int maxHeight)
    {
        foreach (var transition in fromState.Transitions(maxHeight))
        {
            if (toState == transition.N2)
            {
                yield return transitionSoFar.Add(transition.Data) ;
            }

            if (length == 1) continue;
            
            foreach (var immutableList in Recurse(transition.N2, toState, transitionSoFar.Add(transition.Data), length - 1, maxHeight))
            {
                yield return immutableList;
            }
        }

    }

    private static StateGraph CalculateGraph(Siteswap siteswap, int? length)
    {
        return StateGraphFromSiteswapGenerator.CalculateGraph(siteswap.Items.EnumerateValues(1).ToArray(), length);
    }
}
