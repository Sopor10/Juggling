using Microsoft.Msagl.Drawing;
using Siteswap.Details.StateDiagram;
using Siteswap.Details.StateDiagram.Graph;
using Siteswaps.Visualization.SVG.Renderer;

namespace Siteswaps.Visualization.SVG;

public class GraphLayoutFactory
{
    public Graph<Diagram.DiagramNode, string> Create(StateGraph stateGraph)
    {
        var drawingGraph = new Graph();

        foreach (var state in stateGraph.Graph.Nodes)
        {
            var node = new StateNode(state.StateRepresentation());

            drawingGraph.AddNode(node);
        }

        foreach (var graphEdge in stateGraph.Graph.Edges)
        {
            drawingGraph.AddEdge(
                graphEdge.N1.StateRepresentation(),
                graphEdge.Data.ToString(),
                graphEdge.N2.StateRepresentation()
            );
        }

        var doc = new Diagram(drawingGraph);
        return doc.Run();
    }
}
