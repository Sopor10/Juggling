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

        jif = jif with
        {
            Jugglers = ArrangeJugglersByTimeZone(
                jif.Jugglers
                    ?? throw new InvalidOperationException(
                        "The JIF mapper did not return jugglers."
                    ),
                timeZones
            ),
        };
        return JsonSerializer.Serialize(jif, JsonOptions);
    }

    private static JifDetails::Siteswap.Details.Jif.JifJuggler[] ArrangeJugglersByTimeZone(
        IReadOnlyList<JifDetails::Siteswap.Details.Jif.JifJuggler> jugglers,
        IReadOnlyList<int> timeZones
    )
    {
        const double spacing = 2.0;
        var zones = timeZones.Distinct().Order().ToArray();
        var peopleByZone = zones
            .Select(zone =>
                Enumerable
                    .Range(0, timeZones.Count)
                    .Where(person => timeZones[person] == zone)
                    .ToArray()
            )
            .ToArray();
        var radius = 1.2 + zones.Length * 0.2;
        var positions = new decimal[timeZones.Count][];

        for (var zoneIndex = 0; zoneIndex < zones.Length; zoneIndex++)
        {
            var people = peopleByZone[zoneIndex];
            var angle = zones.Length == 1 ? 0 : 2 * Math.PI * zoneIndex / zones.Length;
            var centerX = zones.Length == 1 ? 0 : radius * Math.Cos(angle);
            var centerZ = zones.Length == 1 ? 0 : radius * Math.Sin(angle);
            var tangentX = -Math.Sin(angle);
            var tangentZ = Math.Cos(angle);

            for (var memberIndex = 0; memberIndex < people.Length; memberIndex++)
            {
                var offset = (memberIndex - (people.Length - 1) / 2.0) * spacing;
                positions[people[memberIndex]] =
                [
                    RoundLikePassist(centerX + tangentX * offset),
                    0,
                    RoundLikePassist(centerZ + tangentZ * offset),
                ];
            }
        }

        return jugglers
            .Select((juggler, index) => juggler with { Position = positions[index] })
            .ToArray();
    }

    private static decimal RoundLikePassist(double value) =>
        decimal.Round((decimal)value, 3, MidpointRounding.AwayFromZero);
}
