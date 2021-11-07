using System.Drawing;
using DotNetGraph;
using DotNetGraph.Core;
using DotNetGraph.Edge;
using DotNetGraph.Node;
using Siteswaps.StateDiagram;
using Siteswaps.StateDiagram.Graph;

namespace Siteswaps.Visualization
{
    public class GraphFactory
    {
        public DotGraph Create(StateGraph graph)
        {
            var directedGraph = new DotGraph("Graph", true);

            foreach (var node in graph.Graph.Nodes)
            {
                directedGraph.Elements.Add(ToNode(node));
            }
            
            foreach (var edge in graph.Graph.Edges)
            {
                directedGraph.Elements.Add(ToEdge(edge));
            }
            
            return directedGraph;
        }

        private IDotElement ToEdge(Edge<State,int> edge)
        {
           return new DotEdge(ToNode(edge.N1), ToNode(edge.N2))
           {
               Color = Color.Black,
               FontColor = Color.Black,
               Label = edge.Data.ToString(),
               Style = DotEdgeStyle.Solid,
           };
        }

        private IDotElement ToNode(State node) =>
            new DotNode(node.ToString())
            {
                Shape = DotNodeShape.Ellipse,
                Label = node.ToString(),
                FillColor = Color.LightBlue,
                FontColor = Color.Black,
                Style = DotNodeStyle.Filled,
            };
    }
}