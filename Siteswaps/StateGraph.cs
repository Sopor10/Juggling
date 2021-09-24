using Siteswaps.Graph;

namespace Siteswaps
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