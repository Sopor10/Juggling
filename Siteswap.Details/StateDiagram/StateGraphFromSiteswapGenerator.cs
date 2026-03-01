using Siteswap.Details.StateDiagram.Graph;

namespace Siteswap.Details.StateDiagram;

public static class StateGraphFromSiteswapGenerator
{
    public static StateGraph CalculateGraph(Siteswap siteswap)
    {
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
