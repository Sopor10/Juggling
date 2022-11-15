using Siteswap.Details.StateDiagram.Graph;

namespace Siteswap.Details.StateDiagram;

public class StateGraph
{
    public StateGraph(Graph<State, int> graph)
    {
        Graph = graph;
    }

    public Graph<State, int> Graph { get; }

    public StateGraph Combine(StateGraph other) => new(Graph.Combine(other.Graph));
}