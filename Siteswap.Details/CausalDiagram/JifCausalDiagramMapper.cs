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
        if (sourceJugglers.Count == 0 || sourceLimbs.Count == 0)
            throw new ArgumentException(
                "The JIF needs jugglers and limbs to draw a causal diagram.",
                nameof(jif)
            );
        if (timeStretchFactor <= 0)
            throw new ArgumentException(
                "The JIF time stretch factor must be positive.",
                nameof(jif)
            );

        var jugglers = sourceJugglers
            .Select(
                (juggler, index) =>
                    new Person(juggler.Name ?? juggler.Label ?? ((char)('A' + index)).ToString())
            )
            .ToImmutableList();
        var hands = sourceLimbs
            .Select(limb =>
            {
                var jugglerIndex = limb.Juggler ?? -1;
                if (jugglerIndex < 0 || jugglerIndex >= jugglers.Count)
                    throw new ArgumentException(
                        "Every JIF limb must reference an existing juggler.",
                        nameof(jif)
                    );

                var isLeft =
                    limb.Type?.Contains("left", StringComparison.OrdinalIgnoreCase) == true
                    || limb.Label?.Equals("L", StringComparison.OrdinalIgnoreCase) == true;
                return new Hand(isLeft ? "L" : "R", jugglers[jugglerIndex]);
            })
            .ToImmutableList();

        var nodes = new List<Throw>();
        var arrows = new List<DiagramArrow>();
        foreach (var jifThrow in sourceThrows.OrderBy(throwEvent => throwEvent.Time ?? 0m))
        {
            var duration = jifThrow.Duration ?? 0m;
            if (duration <= 0)
                continue;

            var fromIndex = jifThrow.From ?? -1;
            var toIndex = jifThrow.To ?? -1;
            if (fromIndex < 0 || fromIndex >= hands.Count || toIndex < 0 || toIndex >= hands.Count)
                throw new ArgumentException(
                    "Every JIF throw must reference existing limbs.",
                    nameof(jif)
                );

            if (decimal.Truncate(duration) != duration)
                throw new ArgumentException(
                    "Causal diagrams currently require whole-number throw durations.",
                    nameof(jif)
                );

            var fromHand = hands[fromIndex];
            var toHand = hands[toIndex];
            var time = jifThrow.Time ?? 0m;
            var node = new Throw(fromHand, decimal.ToInt32(duration), time, toIndex);
            nodes.Add(node);

            var causalStep = duration - 2m * timeStretchFactor;
            if (decimal.Truncate(causalStep) != causalStep)
                throw new ArgumentException(
                    "The JIF time scale does not align with whole beat steps.",
                    nameof(jif)
                );

            var step = decimal.ToInt32(causalStep);
            var causalTarget = new Throw(toHand, 0, time + causalStep);
            var toJuggler = toHand.Person;
            arrows.Add(new DiagramArrow(node, causalTarget, step, toJuggler, OrbitIndex: -1));
        }

        if (nodes.Count == 0)
            throw new ArgumentException(
                "The JIF has no positive-height throws to draw.",
                nameof(jif)
            );

        var endTime = nodes.Max(node => node.Time) + 1m;
        var period = jif.Repetition?.Period is > 0
            ? jif.Repetition.Period.Value
            : decimal.ToInt32(decimal.Ceiling(endTime));

        return new DiagramSet(
            Hands: hands,
            Jugglers: jugglers,
            Throws: nodes.ToImmutableList(),
            CausalArrows: arrows.ToImmutableList(),
            LadderArrows: ImmutableList<DiagramArrow>.Empty,
            TimeStretchFactor: timeStretchFactor,
            StepCount: Math.Max(1, period),
            LimbPermutation: Enumerable.Range(0, hands.Count).ToImmutableList(),
            StartProps: ImmutableDictionary<string, StartProps>.Empty
        );
    }
}
