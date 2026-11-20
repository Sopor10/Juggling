using System.Collections.Immutable;

namespace Siteswap.Details.CausalDiagram;

/// <summary>Builds the causal event model for a phased, person-to-person passing pattern.</summary>
public static class PassingDiagramBuilder
{
    public static DiagramSet Build(
        IReadOnlyList<int> timeZones,
        IReadOnlyList<IReadOnlyList<int>> heights,
        IReadOnlyList<IReadOnlyList<int>> targets
    )
    {
        ArgumentNullException.ThrowIfNull(timeZones);
        ArgumentNullException.ThrowIfNull(heights);
        ArgumentNullException.ThrowIfNull(targets);
        if (
            timeZones.Count == 0
            || heights.Count != timeZones.Count
            || targets.Count != timeZones.Count
        )
            throw new ArgumentException(
                "Time zones, heights, and targets must describe the same non-empty set of jugglers."
            );

        var period = heights[0].Count;
        if (
            period == 0
            || heights.Any(row => row.Count != period)
            || targets.Any(row => row.Count != period)
        )
            throw new ArgumentException(
                "Every juggler must have the same non-empty number of beats."
            );
        if (timeZones.Any(zone => zone < 0))
            throw new ArgumentOutOfRangeException(
                nameof(timeZones),
                "Time zones cannot be negative."
            );
        if (targets.SelectMany(row => row).Any(target => target < 0 || target >= timeZones.Count))
            throw new ArgumentOutOfRangeException(
                nameof(targets),
                "Every throw must target a known juggler."
            );
        if (heights.SelectMany(row => row).Any(height => height < 0))
            throw new ArgumentOutOfRangeException(
                nameof(heights),
                "Throw heights cannot be negative."
            );

        var distinctZones = timeZones.Distinct().Order().ToArray();
        var timeStretch = Math.Max(1, distinctZones.Length);
        var normalizedTimeZones = timeZones
            .Select(zone => Array.IndexOf(distinctZones, zone))
            .ToArray();
        var jugglers = timeZones
            .Select((_, index) => new Person(((char)('A' + index)).ToString()))
            .ToImmutableList();
        var hands = Enumerable
            .Range(0, timeZones.Count)
            .SelectMany(person =>
                new[] { new Hand("R", jugglers[person]), new Hand("L", jugglers[person]) }
            )
            .ToImmutableList();
        var basePeriod = period * timeStretch;
        var nodes = new List<Throw>();

        for (var person = 0; person < timeZones.Count; person++)
        {
            for (var beat = 0; beat < period; beat++)
            {
                var height = heights[person][beat];
                if (height == 0)
                    continue;

                var targetPerson = targets[person][beat];
                var time = beat * timeStretch + normalizedTimeZones[person];
                var landingTimeZone = (normalizedTimeZones[person] + height) % timeStretch;
                if (landingTimeZone != normalizedTimeZones[targetPerson])
                    throw new ArgumentException(
                        $"Throw from juggler {person} at beat {beat} cannot land in the selected juggler's time zone.",
                        nameof(targets)
                    );

                var landingLocalBeat = beat + (normalizedTimeZones[person] + height) / timeStretch;
                var fromLimb = person * 2 + beat % 2;
                var toLimb = targetPerson * 2 + landingLocalBeat % 2;
                nodes.Add(new Throw(hands[fromLimb], height, time, toLimb));
            }
        }

        if (nodes.Count == 0)
            throw new ArgumentException(
                "The pattern must contain at least one positive-height throw.",
                nameof(heights)
            );

        var causalArrows = new List<DiagramArrow>(nodes.Count);
        var ladderArrows = new List<DiagramArrow>(nodes.Count);
        foreach (var node in nodes)
        {
            var toLimb = node.ToLimb
                ?? throw new InvalidOperationException("Every throw must identify its destination limb.");
            var toHand = hands[toLimb];
            var causalStep = node.Height - 2 * timeStretch;
            causalArrows.Add(
                new DiagramArrow(
                    node,
                    new Throw(toHand, 0, node.Time + causalStep),
                    causalStep,
                    toHand.Person,
                    OrbitIndex: -1
                )
            );
            ladderArrows.Add(
                new DiagramArrow(
                    node,
                    new Throw(toHand, 0, node.Time + node.Height),
                    node.Height,
                    toHand.Person,
                    OrbitIndex: -1
                )
            );
        }

        var limbPermutation = Enumerable
            .Range(0, timeZones.Count * 2)
            .Select(limb => limb / 2 * 2 + (limb % 2 + period % 2) % 2)
            .ToImmutableList();

        return new DiagramSet(
            Hands: hands,
            Jugglers: jugglers,
            Throws: nodes.OrderBy(node => node.Time).ToImmutableList(),
            CausalArrows: causalArrows.ToImmutableList(),
            LadderArrows: ladderArrows.ToImmutableList(),
            TimeStretchFactor: timeStretch,
            StepCount: basePeriod,
            LimbPermutation: limbPermutation,
            StartProps: ImmutableDictionary<string, StartProps>.Empty
        );
    }
}
