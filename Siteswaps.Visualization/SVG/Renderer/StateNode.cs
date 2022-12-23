using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;

namespace Siteswaps.Visualization.SVG.Renderer;

public class StateNode : Node
{
    public StateNode(string id) : base(id)
    {
        Label = new Label(id);
    }

    private Label Label { get; }

    public void CreateBoundary()
    {
        var height = Label.Height;
        var width = 45d;

        GeometryNode.BoundaryCurve = CurveFactory.CreateRectangle(width, height, new Point(0, 0));
    }
}