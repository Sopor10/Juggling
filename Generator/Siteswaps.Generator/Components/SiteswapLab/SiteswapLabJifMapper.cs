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

        jif = jif with
        {
            Jugglers = ApplyLayout(
                jif.Jugglers
                    ?? throw new InvalidOperationException(
                        "The JIF mapper did not return jugglers."
                    ),
                layout
            ),
        };
        return JsonSerializer.Serialize(jif, JsonOptions);
    }

    private static JifDetails::Siteswap.Details.Jif.JifJuggler[] ApplyLayout(
        IReadOnlyList<JifDetails::Siteswap.Details.Jif.JifJuggler> jugglers,
        IReadOnlyList<JugglerLayoutPose> layout
    )
    {
        if (jugglers.Count != layout.Count)
        {
            throw new ArgumentException(
                "There must be a layout position for each juggler.",
                nameof(layout)
            );
        }

        const double layoutScale = 4.5;
        return jugglers
            .Select(
                (juggler, index) =>
                {
                    var pose = layout[index];
                    var x = pose.X * layoutScale;
                    var z = pose.Z * layoutScale;
                    var radians = pose.Rotation * Math.PI / 180;
                    return juggler with
                    {
                        Position = [RoundLikePassist(x), 0, RoundLikePassist(z)],
                        LookAt =
                        [
                            RoundLikePassist(x + Math.Sin(radians)),
                            0,
                            RoundLikePassist(z - Math.Cos(radians)),
                        ],
                    };
                }
            )
            .ToArray();
    }

    private static decimal RoundLikePassist(double value) =>
        decimal.Round((decimal)value, 3, MidpointRounding.AwayFromZero);
}
