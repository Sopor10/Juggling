using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;

namespace Siteswaps.Visualization.SVG.Renderer;

public class StateNode(string id) : Node(id)
{
    private new Label Label { get; } = new(id);

    public void CreateBoundary()
    {
        var height = Label.Height;
        var width = 45d;

        GeometryNode.BoundaryCurve = CurveFactory.CreateRectangle(width, height, new Point(0, 0));
    }
}
