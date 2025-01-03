
using DotNetGraph.Core;
using Siteswap.Details.StateDiagram;
using Siteswap.Details.StateDiagram.Graph;

namespace Siteswaps.Visualization;

public class GraphFactory
{
    public DotGraph Create(StateGraph graph)
    {
        var directedGraph = new DotGraph
        {
            Identifier = new("G"),
            Strict = true
        };

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
        return new DotEdge
        {
            From = new (edge.N1.ToString()),
            To = new (edge.N2.ToString()),
            Color = DotColor.Black,
            FontColor = DotColor.Black,
            Label = edge.Data.ToString(),
            Style = DotEdgeStyle.Solid,
        };
    }

    private IDotElement ToNode(State node) =>
        new DotNode
        {
            Identifier = new(node.ToString()),
            Shape = DotNodeShape.Ellipse,
            Label = node.ToString(),
            FillColor = DotColor.LightBlue,
            FontColor = DotColor.Black,
            Style = DotNodeStyle.Filled,
        };
}
