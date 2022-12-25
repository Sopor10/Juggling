using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Miscellaneous;
using Siteswap.Details.StateDiagram.Graph;

namespace Siteswaps.Visualization.SVG.Renderer;

public class Diagram {

    private Graph DrawingGraph { get;  }

    public Diagram(Graph drawingGraph) {
        DrawingGraph = drawingGraph;
    }

    // Abstract the creation of the GeometryGraph and the node.CreateBoundary calls away in
    // a single call on the Diagram.
    public Graph<DiagramNode, string> Run() {
        DrawingGraph.CreateGeometryGraph();

        foreach (var node in DrawingGraph.Nodes) {
            if (node is StateNode stateNode) stateNode.CreateBoundary();
            
        }

        var routingSettings = new Microsoft.Msagl.Core.Routing.EdgeRoutingSettings {
            BendPenalty = 100,
            EdgeRoutingMode = Microsoft.Msagl.Core.Routing.EdgeRoutingMode.StraightLine
        };
        var settings = new SugiyamaLayoutSettings {
            ClusterMargin = 50,
            PackingAspectRatio = 3,
            PackingMethod = Microsoft.Msagl.Core.Layout.PackingMethod.Columns,
            RepetitionCoefficientForOrdering = 0,
            EdgeRoutingSettings = routingSettings,
            NodeSeparation = 50,
            LayerSeparation = 150
        };
        LayoutHelpers.CalculateLayout(DrawingGraph.GeometryGraph, settings, null);

        var nodes = DrawingGraph.Nodes.OfType<StateNode>()
            .Select(node => new DiagramNode(node.GeometryNode.Center.X, node.GeometryNode.Center.Y, node.Id))
            .ToHashSet();

        var edges = new HashSet<Edge<DiagramNode, string>>();

        foreach (var drawingGraphEdge in DrawingGraph.Edges)
        {
            edges.Add(new Edge<DiagramNode, string>(
                nodes.Single(x => x.Id == drawingGraphEdge.Source),
                nodes.Single(x => x.Id == drawingGraphEdge.Target),
                drawingGraphEdge.LabelText));
        }
        return new Graph<DiagramNode, string>(nodes, edges);
    }


    public record DiagramNode(double X, double Y, string Id);
}