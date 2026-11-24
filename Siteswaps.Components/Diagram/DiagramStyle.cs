using VisNetwork.Blazor.Models;

namespace Siteswaps.Components.Diagram;

/// <summary>Maps shared design decisions to the concrete values required by vis-network.</summary>
internal static class DiagramStyle
{
    public static NetworkOptions Options(VisNetwork.Blazor.Network _, DiagramDesignDecisions decisions) =>
        new()
        {
            Nodes = new NodeOption
            {
                Shape = "circle",
                BorderWidth = 2,
                Color = NodeColor(decisions),
                Font = NodeFont(decisions),
            },
            Edges = new EdgeOption
            {
                Color = EdgeColor(false, decisions),
                Font = EdgeFont(decisions),
                Width = 2,
                SelectionWidth = 3,
            },
            Interaction = new InteractionOptions
            {
                DragNodes = true,
                DragView = true,
                ZoomView = true,
            },
        };

    public static Node CreateStateNode(string id, string label, DiagramDesignDecisions decisions) =>
        new(id, label, 1, "circle")
        {
            BorderWidth = 2,
            Color = NodeColor(decisions),
            Font = NodeFont(decisions),
            Mass = 1,
        };

    public static Edge CreateEdge(
        string from,
        string to,
        string label,
        DiagramDesignDecisions decisions,
        bool highlight = false
    ) =>
        new(from, to, label)
        {
            Label = label,
            Arrows = new Arrows { To = new ArrowsOptions { Enabled = true } },
            Color = EdgeColor(highlight, decisions),
            Font = EdgeFont(decisions),
            Width = highlight ? 3 : 2,
        };

    private static NodeColorType NodeColor(DiagramDesignDecisions decisions) =>
        new()
        {
            Background = decisions.BorderSubtle,
            Border = decisions.BrandAccent,
            Highlight = new NodeColorType.BorderBackgroundColor
            {
                Background = decisions.White,
                Border = decisions.ActionPrimary,
            },
            Hover = new NodeColorType.BorderBackgroundColor
            {
                Background = decisions.White,
                Border = decisions.BrandStrong,
            },
        };

    private static ColorType EdgeColor(bool highlight, DiagramDesignDecisions decisions) =>
        new()
        {
            Color = highlight ? decisions.ActionPrimary : decisions.BrandStrong,
            Highlight = decisions.ActionPrimary,
            Hover = decisions.ActionPrimary,
        };

    private static Font NodeFont(DiagramDesignDecisions decisions) =>
        new()
        {
            Color = decisions.TextStrong,
            Face = decisions.FontBody,
            Size = 14,
        };

    private static Font EdgeFont(DiagramDesignDecisions decisions) =>
        new()
        {
            Color = decisions.TextStrong,
            Face = decisions.FontBody,
            Size = 12,
            StrokeWidth = 3,
            StrokeColor = decisions.White,
        };
}
