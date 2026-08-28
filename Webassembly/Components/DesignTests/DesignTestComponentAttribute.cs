namespace Webassembly.Components.DesignTests;

/// <summary>
/// Marks a Blazor component as a visual design fixture.
/// Default viewports are Full HD (1920) and narrow mobile (360); override via <see cref="Widths"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DesignTestComponentAttribute : Attribute
{
    /// <summary>Default CSS viewport widths when <see cref="Widths"/> is null or empty.</summary>
    public static readonly int[] DefaultWidths = [1920, 360];

    /// <summary>
    /// Viewport widths in CSS pixels. Null or empty → <see cref="DefaultWidths"/>.
    /// Example: <c>[DesignTestComponent(Widths = new[] { 1280 })]</c> for a single custom size.
    /// </summary>
    public int[]? Widths { get; init; }

    public IReadOnlyList<int> ResolveWidths() => Widths is { Length: > 0 } w ? w : DefaultWidths;

    /// <summary>Paired viewport height so responsive layouts have enough vertical room.</summary>
    public static int HeightForWidth(int width) =>
        width switch
        {
            1920 => 1080,
            360 => 800,
            _ => Math.Max(720, (int)Math.Round(width * 9.0 / 16.0)),
        };
}
