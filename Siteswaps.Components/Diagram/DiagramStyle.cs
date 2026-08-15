using VisNetwork.Blazor.Models;

namespace Siteswaps.Components.Diagram;

/// <summary>Passing Zone brand colors for vis-network state/transition graphs.</summary>
internal static class DiagramStyle
{
    public const string Purple950 = "#241a3d";
    public const string Purple700 = "#3c286d";
    public const string Purple500 = "#552f8c";
    public const string Purple100 = "#e8e1f7";
    public const string Orange = "#f9a500";
    public const string White = "#ffffff";

    public static NetworkOptions Options(VisNetwork.Blazor.Network _) =>
        new()
        {
            Nodes = new NodeOption
            {
                Shape = "circle",
                BorderWidth = 2,
                Color = NodeColor(),
                Font = NodeFont(),
            },
            Edges = new EdgeOption
            {
                Color = EdgeColor(false),
                Font = EdgeFont(),
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

    public static Node CreateStateNode(string id, string label) =>
        new(id, label, 1, "circle")
        {
            BorderWidth = 2,
            Color = NodeColor(),
            Font = NodeFont(),
            Mass = 1,
        };

    public static Edge CreateEdge(string from, string to, string label, bool highlight = false) =>
        new(from, to, label)
        {
            Label = label,
            Arrows = new Arrows { To = new ArrowsOptions { Enabled = true } },
            Color = EdgeColor(highlight),
            Font = EdgeFont(),
            Width = highlight ? 3 : 2,
        };

    private static NodeColorType NodeColor() =>
        new()
        {
            Background = Purple100,
            Border = Purple500,
            Highlight = new NodeColorType.BorderBackgroundColor
            {
                Background = White,
                Border = Orange,
            },
            Hover = new NodeColorType.BorderBackgroundColor
            {
                Background = White,
                Border = Purple700,
            },
        };

    private static ColorType EdgeColor(bool highlight) =>
        new()
        {
            Color = highlight ? Orange : Purple700,
            Highlight = Orange,
            Hover = Orange,
        };

    private static Font NodeFont() =>
        new()
        {
            Color = Purple950,
            Face = "Nunito, system-ui, sans-serif",
            Size = 14,
        };

    private static Font EdgeFont() =>
        new()
        {
            Color = Purple950,
            Face = "Nunito, system-ui, sans-serif",
            Size = 12,
            StrokeWidth = 3,
            StrokeColor = White,
        };
}
