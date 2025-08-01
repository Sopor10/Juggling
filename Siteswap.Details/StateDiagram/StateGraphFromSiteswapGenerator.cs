using Siteswap.Details.StateDiagram.Graph;

namespace Siteswap.Details.StateDiagram;

public static class StateGraphFromSiteswapGenerator
{
    public static StateGraph CalculateGraph(int[] siteswap)
    {
        var cyclicArray = siteswap.ToCyclicArray();

        var states = new List<State>();
        var stateToSiteswap = new Dictionary<State, List<Siteswap>>();
        for (var i = 0; i < siteswap.Length; i++)
        {
            var rotate = cyclicArray.Rotate(i);
            var calculateState = StateGenerator.CalculateState(rotate.EnumerateValues(1).ToArray());
            states.Add(calculateState);

            if (stateToSiteswap.TryGetValue(calculateState, out var values))
                values.Add(new Siteswap(rotate));
            else
                stateToSiteswap.Add(calculateState, new List<Siteswap> { new(rotate) });
        }

        var mappedStates = new HashSet<State>(states.Select(state => state).ToList());

        var cyclicArrayStates = new CyclicArray<State>(mappedStates);
        var edges = new HashSet<Edge<State, int>>();
        for (var i = 0; i < cyclicArrayStates.Length; i++)
            edges.Add(
                new Edge<State, int>(cyclicArrayStates[i], cyclicArrayStates[i + 1], siteswap[i])
            );

        return new StateGraph(new Graph<State, int>(mappedStates, edges));
    }

    public static StateGraph CalculateGraph(Siteswap siteswap)
    {
        var stateToSiteswap = new Dictionary<State, List<Siteswap>>();
        var edges = new HashSet<Edge<State, int>>();
        var mappedStates = new HashSet<State>();

        var nSiteswap = siteswap;

        foreach (var valueTuple in siteswap.Items.Enumerate(1))
        {
            (nSiteswap, var nThrow) = nSiteswap.Throw();
            mappedStates.Add(nThrow.StartingState);
            mappedStates.Add(nThrow.EndingState);
            edges.Add(new Edge<State, int>(nThrow.StartingState, nThrow.EndingState, nThrow.Value));
        }

        return new StateGraph(new Graph<State, int>(mappedStates, edges));
    }
}
