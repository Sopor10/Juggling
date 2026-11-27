using System.Collections.Immutable;
using Siteswap.Details.Jif;

namespace Siteswap.Details.CausalDiagram;

/// <summary>Projects a complete JIF pattern into the causal diagram view model.</summary>
public static class JifCausalDiagramMapper
{
    public static DiagramSet Map(JifPattern jif)
    {
        ArgumentNullException.ThrowIfNull(jif);

        var sourceJugglers = jif.Jugglers ?? [];
        var sourceLimbs = jif.Limbs ?? [];
        var sourceThrows = jif.Throws ?? [];
        var timeStretchFactor = jif.TimeStretchFactor ?? 1m;
        ValidatePattern(sourceJugglers, sourceLimbs, timeStretchFactor, nameof(jif));

        var jugglers = CreateJugglers(sourceJugglers);
        var hands = CreateHands(sourceLimbs, jugglers, nameof(jif));
        var period = jif.Repetition?.Period is > 0
            ? jif.Repetition.Period.Value
            : GetSourcePeriod(sourceThrows);
        var (nodes, arrows) = CreateThrows(sourceThrows, hands, period, nameof(jif));
        if (nodes.Count == 0)
            throw new ArgumentException(
                "The JIF has no positive-height throws to draw.",
                nameof(jif)
            );

        return new DiagramSet(
            Hands: hands,
            Jugglers: jugglers,
            Throws: nodes,
            CausalArrows: arrows,
            LadderArrows: ImmutableList<DiagramArrow>.Empty,
            TimeStretchFactor: timeStretchFactor,
            StepCount: Math.Max(1, period),
            LimbPermutation: Enumerable.Range(0, hands.Count).ToImmutableList(),
            StartProps: ImmutableDictionary<string, StartProps>.Empty,
            JugglerBeatDurations: jugglers.ToImmutableDictionary(
                juggler => juggler.Name,
                _ => timeStretchFactor,
                StringComparer.Ordinal
            )
        );
    }

    private static void ValidatePattern(
        IReadOnlyCollection<JifJuggler> jugglers,
        IReadOnlyCollection<JifLimb> limbs,
        decimal timeStretchFactor,
        string parameterName
    )
    {
        if (jugglers.Count == 0 || limbs.Count == 0)
            throw new ArgumentException(
                "The JIF needs jugglers and limbs to draw a causal diagram.",
                parameterName
            );
        if (timeStretchFactor <= 0)
            throw new ArgumentException(
                "The JIF time stretch factor must be positive.",
                parameterName
            );
    }

    private static ImmutableList<Person> CreateJugglers(IReadOnlyList<JifJuggler> sourceJugglers) =>
        sourceJugglers
            .Select(
                (juggler, index) =>
                    new Person(juggler.Name ?? juggler.Label ?? ((char)('A' + index)).ToString())
            )
            .ToImmutableList();

    private static ImmutableList<Hand> CreateHands(
        IReadOnlyList<JifLimb> sourceLimbs,
        ImmutableList<Person> jugglers,
        string parameterName
    ) => sourceLimbs.Select(limb => CreateHand(limb, jugglers, parameterName)).ToImmutableList();

    private static int GetSourcePeriod(IReadOnlyList<JifThrow> sourceThrows) =>
        sourceThrows.Count == 0
            ? 1
            : decimal.ToInt32(decimal.Ceiling(sourceThrows.Max(node => node.Time ?? 0m) + 1m));

    private static Hand CreateHand(
        JifLimb limb,
        ImmutableList<Person> jugglers,
        string parameterName
    )
    {
        var jugglerIndex = limb.Juggler ?? -1;
        if (jugglerIndex < 0 || jugglerIndex >= jugglers.Count)
            throw new ArgumentException(
                "Every JIF limb must reference an existing juggler.",
                parameterName
            );

        var isLeft =
            limb.Type?.Contains("left", StringComparison.OrdinalIgnoreCase) == true
            || limb.Label?.Equals("L", StringComparison.OrdinalIgnoreCase) == true;
        return new Hand(isLeft ? "L" : "R", jugglers[jugglerIndex]);
    }

    private static (
        ImmutableList<DiagramThrow> Nodes,
        ImmutableList<DiagramArrow> Arrows
    ) CreateThrows(
        IReadOnlyList<JifThrow> sourceThrows,
        ImmutableList<Hand> hands,
        int period,
        string parameterName
    )
    {
        var throwCountsByJuggler = sourceThrows
            .Where(throwEvent =>
                throwEvent.Duration is > 0
                && throwEvent.From is int from
                && from >= 0
                && from < hands.Count
            )
            .GroupBy(throwEvent => hands[throwEvent.From.GetValueOrDefault()].Person)
            .ToDictionary(group => group.Key, group => group.Count());
        var nodes = ImmutableList.CreateBuilder<DiagramThrow>();
        var arrows = ImmutableList.CreateBuilder<DiagramArrow>();
        foreach (var jifThrow in sourceThrows.OrderBy(throwEvent => throwEvent.Time ?? 0m))
        {
            var duration = jifThrow.Duration ?? 0m;
            if (duration <= 0)
                continue;
            if (decimal.Truncate(duration) != duration)
                throw new ArgumentException(
                    "Causal diagrams currently require whole-number throw durations.",
                    parameterName
                );

            var fromIndex = jifThrow.From ?? -1;
            var toIndex = jifThrow.To ?? -1;
            if (fromIndex < 0 || fromIndex >= hands.Count || toIndex < 0 || toIndex >= hands.Count)
                throw new ArgumentException(
                    "Every JIF throw must reference existing limbs.",
                    parameterName
                );

            var fromHand = hands[fromIndex];
            var toHand = hands[toIndex];
            var time = jifThrow.Time ?? 0m;
            var node = new DiagramThrow(fromHand, time, toIndex);
            nodes.Add(node);

            if (!throwCountsByJuggler.TryGetValue(toHand.Person, out var targetThrowCount))
                throw new ArgumentException(
                    "Every JIF target juggler must have throws in the repetition period.",
                    parameterName
                );
            var targetBeatDuration = (decimal)period / targetThrowCount;
            var causalStep = duration - 2m * targetBeatDuration;
            if (decimal.Truncate(causalStep) != causalStep)
                throw new ArgumentException(
                    "The JIF time scale does not align with whole beat steps.",
                    parameterName
                );

            var step = decimal.ToInt32(causalStep);
            var causalTarget = new DiagramThrow(toHand, time + causalStep);
            arrows.Add(new DiagramArrow(node, causalTarget, step, toHand.Person, OrbitIndex: -1));
        }

        return (nodes.ToImmutable(), arrows.ToImmutable());
    }
}
