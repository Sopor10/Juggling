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
        var period = ValidateDimensions(timeZones, heights, targets);
        ValidateValues(timeZones, heights, targets);

        var distinctZones = timeZones.Distinct().Order().ToArray();
        var timeStretch = Math.Max(1, distinctZones.Length);
        var diagramPatternPeriod = period % 2 == 0 ? period : period * 2;
        var normalizedTimeZones = NormalizeTimeZones(timeZones, distinctZones);
        var jugglers = CreateJugglers(timeZones.Count);
        var hands = CreateHands(jugglers);
        var nodes = CreateThrows(
            timeZones,
            heights,
            targets,
            normalizedTimeZones,
            timeStretch,
            period,
            diagramPatternPeriod,
            hands
        );
        if (nodes.Count == 0)
            throw new ArgumentException(
                "The pattern must contain at least one positive-height throw.",
                nameof(heights)
            );

        var (causalArrows, ladderArrows) = CreateArrows(nodes, hands, timeStretch);
        var basePeriod = diagramPatternPeriod * timeStretch;
        var limbPermutation = Enumerable
            .Range(0, timeZones.Count * 2)
            .Select(limb => limb / 2 * 2 + (limb % 2 + diagramPatternPeriod % 2) % 2)
            .ToImmutableList();

        return new DiagramSet(
            Hands: hands,
            Jugglers: jugglers,
            Throws: nodes.OrderBy(node => node.Time).ToImmutableList(),
            CausalArrows: causalArrows,
            LadderArrows: ladderArrows,
            TimeStretchFactor: timeStretch,
            StepCount: basePeriod,
            LimbPermutation: limbPermutation,
            StartProps: ImmutableDictionary<string, StartProps>.Empty
        );
    }

    private static int ValidateDimensions(
        IReadOnlyList<int> timeZones,
        IReadOnlyList<IReadOnlyList<int>> heights,
        IReadOnlyList<IReadOnlyList<int>> targets
    )
    {
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

        return period;
    }

    private static void ValidateValues(
        IReadOnlyList<int> timeZones,
        IReadOnlyList<IReadOnlyList<int>> heights,
        IReadOnlyList<IReadOnlyList<int>> targets
    )
    {
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
    }

    private static int[] NormalizeTimeZones(IReadOnlyList<int> timeZones, int[] distinctZones) =>
        timeZones.Select(zone => Array.IndexOf(distinctZones, zone)).ToArray();

    private static ImmutableList<Person> CreateJugglers(int count) =>
        Enumerable
            .Range(0, count)
            .Select(index => new Person(((char)('A' + index)).ToString()))
            .ToImmutableList();

    private static ImmutableList<Hand> CreateHands(ImmutableList<Person> jugglers) =>
        Enumerable
            .Range(0, jugglers.Count)
            .SelectMany(person =>
                new[] { new Hand("R", jugglers[person]), new Hand("L", jugglers[person]) }
            )
            .ToImmutableList();

    private static List<Throw> CreateThrows(
        IReadOnlyList<int> timeZones,
        IReadOnlyList<IReadOnlyList<int>> heights,
        IReadOnlyList<IReadOnlyList<int>> targets,
        IReadOnlyList<int> normalizedTimeZones,
        int timeStretch,
        int period,
        int diagramPatternPeriod,
        ImmutableList<Hand> hands
    )
    {
        var nodes = new List<Throw>();
        for (var person = 0; person < timeZones.Count; person++)
        {
            for (var beat = 0; beat < diagramPatternPeriod; beat++)
            {
                var height = heights[person][beat % period];
                if (height == 0)
                    continue;
                nodes.Add(
                    CreateThrow(
                        person,
                        beat,
                        height,
                        targets,
                        normalizedTimeZones,
                        timeStretch,
                        hands
                    )
                );
            }
        }

        return nodes;
    }

    private static Throw CreateThrow(
        int person,
        int beat,
        int height,
        IReadOnlyList<IReadOnlyList<int>> targets,
        IReadOnlyList<int> normalizedTimeZones,
        int timeStretch,
        ImmutableList<Hand> hands
    )
    {
        var targetPerson = targets[person][beat % targets[person].Count];
        var time = beat * timeStretch + normalizedTimeZones[person];
        var landingTimeZone = (normalizedTimeZones[person] + height) % timeStretch;
        if (landingTimeZone != normalizedTimeZones[targetPerson])
            throw new ArgumentException(
                $"Throw from juggler {person} at beat {beat} cannot land in the selected juggler's time zone.",
                nameof(targets)
            );

        var landingLocalBeat = beat
            + (normalizedTimeZones[person] + height - normalizedTimeZones[targetPerson]) / timeStretch;
        var fromLimb = person * 2 + beat % 2;
        var toLimb = targetPerson * 2 + landingLocalBeat % 2;
        return new Throw(hands[fromLimb], height, time, toLimb);
    }

    private static (
        ImmutableList<DiagramArrow> Causal,
        ImmutableList<DiagramArrow> Ladder
    ) CreateArrows(IEnumerable<Throw> nodes, ImmutableList<Hand> hands, int timeStretch)
    {
        var causalArrows = ImmutableList.CreateBuilder<DiagramArrow>();
        var ladderArrows = ImmutableList.CreateBuilder<DiagramArrow>();
        foreach (var node in nodes)
        {
            var toLimb =
                node.ToLimb
                ?? throw new InvalidOperationException(
                    "Every throw must identify its destination limb."
                );
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

        return (causalArrows.ToImmutable(), ladderArrows.ToImmutable());
    }
}
