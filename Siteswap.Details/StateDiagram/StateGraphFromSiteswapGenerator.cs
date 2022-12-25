using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Siteswap.Details.StateDiagram.Graph;

namespace Siteswap.Details.StateDiagram;

public static class StateGraphFromSiteswapGenerator
{
    public static StateGraph CalculateGraph(int[] siteswap, int? length = null)
    {
        var cyclicArray = siteswap.ToCyclicArray();

        var states = new List<State>();
        var stateToSiteswap = new Dictionary<State, List<Siteswap>>();
        for (var i = 0; i < siteswap.Length; i++)
        {
            var rotate = cyclicArray.Rotate(i);
            var calculateState = StateGenerator.CalculateState(rotate.EnumerateValues(1).ToArray(), length);
            states.Add(calculateState);
            
            if (stateToSiteswap.TryGetValue(calculateState, out var values))
            {
                values.Add(new Siteswap(rotate));
            }
            else
            {
                stateToSiteswap.Add(calculateState, new (){new Siteswap(rotate)});
            }
        }

        var mappedStates = new HashSet<State>(states.Select(state => state).ToList());

        var cyclicArrayStates = new CyclicArray<State>(mappedStates);
        var edges = new HashSet<Edge<State, int>>();
        for (var i = 0; i < cyclicArrayStates.Length; i++)
            edges.Add(new Edge<State, int>(cyclicArrayStates[i], cyclicArrayStates[i + 1],
                siteswap[i]));


        return new StateGraph(new Graph<State, int>(mappedStates, edges), stateToSiteswap);
    }
}


