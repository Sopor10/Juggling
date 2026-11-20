extern alias JifDetails;

using System.Text.Json;

namespace Siteswaps.Generator.Components.SiteswapLab;

internal static class SiteswapLabJifMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static string MapViaCausalDiagram(
        IReadOnlyList<int> timeZones,
        IReadOnlyList<IReadOnlyList<int>> heights,
        IReadOnlyList<IReadOnlyList<int>> targets,
        string propType
    )
    {
        var period = heights.Count == 0 ? 0 : heights[0].Count;
        var causalDiagram = JifDetails::Siteswap.Details.CausalDiagram.PassingDiagramBuilder.Build(
            timeZones,
            heights,
            targets
        );
        var jif = JifDetails::Siteswap.Details.Jif.JifMapper.Map(
            causalDiagram,
            propType,
            name: "Siteswap Lab passing pattern",
            description: $"{timeZones.Count} jugglers, period {period}"
        );
        return JsonSerializer.Serialize(jif, JsonOptions);
    }
}
