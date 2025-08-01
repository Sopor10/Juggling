using Siteswap.Details.StateDiagram.Graph;

namespace Siteswap.Details.StateDiagram;

public class StateGraph(Graph<State, int> graph)
{
    public Graph<State, int> Graph { get; } = graph;
}
