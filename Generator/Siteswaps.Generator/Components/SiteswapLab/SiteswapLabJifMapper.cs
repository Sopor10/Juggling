extern alias JifDetails;

using System.Collections.Immutable;
using System.Text.Json;
using Siteswaps.Jif.Components;

namespace Siteswaps.Generator.Components.SiteswapLab;

public static class SiteswapLabJifMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static ImmutableDictionary<
        string,
        JifDetails::Siteswap.Details.CausalDiagram.StartProps
    > CalculateCausalStartProps(
        IReadOnlyList<int> timeZones,
        IReadOnlyList<IReadOnlyList<int>> heights,
        IReadOnlyList<IReadOnlyList<int>> targets
    )
    {
        if (!heights.SelectMany(row => row).Any(height => height > 0))
            return Enumerable
                .Range(0, timeZones.Count)
                .ToImmutableDictionary(
                    index => ((char)('A' + index)).ToString(),
                    _ => new JifDetails::Siteswap.Details.CausalDiagram.StartProps(0, 0),
                    StringComparer.Ordinal
                );

        try
        {
            return JifDetails::Siteswap
                .Details.CausalDiagram.PassingDiagramBuilder.Build(timeZones, heights, targets)
                .StartProps;
        }
        catch (ArgumentException)
        {
            return ImmutableDictionary<string, JifDetails::Siteswap.Details.CausalDiagram.StartProps>.Empty;
        }
    }

    public static string MapViaCausalDiagram(
        IReadOnlyList<int> timeZones,
        IReadOnlyList<IReadOnlyList<int>> heights,
        IReadOnlyList<IReadOnlyList<int>> targets,
        string propType,
        IReadOnlyList<JugglerLayoutPose> layout
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

        jif = JugglerLayoutPresets.ApplyToJif(jif, layout);
        return JsonSerializer.Serialize(jif, JsonOptions);
    }
}
