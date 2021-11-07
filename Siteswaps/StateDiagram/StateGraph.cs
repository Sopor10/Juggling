using Siteswaps.StateDiagram.Graph;

namespace Siteswaps.StateDiagram
{
    public class StateGraph
    {
        public StateGraph(Graph<State, int> graph)
        {
            Graph = graph;
        }

        public Graph<State, int> Graph { get; }
    }
}