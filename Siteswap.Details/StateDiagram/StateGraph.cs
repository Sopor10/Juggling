using System.Collections.Generic;
using System.Linq;
using Siteswap.Details.StateDiagram.Graph;

namespace Siteswap.Details.StateDiagram;

public class StateGraph
{
    public StateGraph(Graph<State, int> graph)
    {
        Graph = graph;
        SiteswapInState = new();
    }
    
    public StateGraph(Graph<State, int> graph, Dictionary<State, List<Siteswap>> siteswapInState)
    {
        Graph = graph;
        SiteswapInState = siteswapInState;
    }

    public Graph<State, int> Graph { get; }

    public Dictionary<State, List<Siteswap>> SiteswapInState { get; }

    public StateGraph Combine(StateGraph other) => new(Graph.Combine(other.Graph), SiteswapInState.Union(other.SiteswapInState).ToDictionary(x => x.Key, x => x.Value));
    
    
}