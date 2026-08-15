namespace Siteswaps.Generator.Components.Feeding;

/// <summary>
/// App-relative feeding deep links (respects &lt;base href&gt; / PathBase).
/// </summary>
public static class FeedingRouteLinks
{
    public static string FromNotation(string notation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(notation);
        return $"feeding?s={Uri.EscapeDataString(notation)}";
    }
}
