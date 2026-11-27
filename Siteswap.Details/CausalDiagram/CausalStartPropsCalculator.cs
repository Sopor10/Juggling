using System.Collections.Immutable;

namespace Siteswap.Details.CausalDiagram;

/// <summary>
/// Calculates cold-start hand inventories from periodic paths through a causal diagram.
/// The older no-incoming-arrow methods remain available for comparison, but cannot account
/// for closed causal loops on their own.
/// </summary>
public static class CausalStartPropsCalculator
{
    /// <summary>Counts periodic prop paths in the causal diagram and assigns each path to its first hand.</summary>
    public static ImmutableDictionary<string, StartProps> CalculateFromPeriodicCausalDiagram(
        DiagramSet diagram
    ) => AnalyzePeriodicCausalDiagram(diagram).StartProps;

    public static CausalStartPropsResult AnalyzePeriodicCausalDiagram(DiagramSet diagram)
    {
        ArgumentNullException.ThrowIfNull(diagram);
        if (diagram.StepCount <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(diagram),
                "Diagram period must be positive."
            );

        var counts = diagram.Jugglers.ToDictionary(
            person => person.Name,
            _ => new StartProps(Left: 0, Right: 0),
            StringComparer.Ordinal
        );
        var totalHeight = diagram.Throws.Sum(node => diagram.HeightOf(node) ?? 0);
        if (totalHeight % diagram.StepCount != 0)
            throw new InvalidOperationException(
                "A periodic causal diagram must have an integral prop count."
            );
        var positiveThrows = diagram.Throws.Where(node => diagram.HeightOf(node) is > 0).ToArray();
        if (positiveThrows.Length == 0)
            return new CausalStartPropsResult(
                counts.ToImmutableDictionary(StringComparer.Ordinal),
                1,
                0
            );

        var period = (decimal)diagram.StepCount;
        var handCount = 2m * diagram.TimeStretchFactor;
        var outgoingArrows = diagram
            .CausalArrows.GroupBy(arrow => Key(arrow.From))
            .ToDictionary(group => group.Key, group => group.ToArray());
        var throwsByHand = positiveThrows
            .Select((node, index) => (Node: node, Index: index))
            .GroupBy(item => HandKey(item.Node.Hand))
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(item => item.Node.Time).ToArray()
            );

        var successor = new int[positiveThrows.Length];
        var periodShift = new int[positiveThrows.Length];
        for (var index = 0; index < positiveThrows.Length; index++)
        {
            var node = positiveThrows[index];
            var key = Key(node);
            var landingHand = GetLandingHand(node, diagram, outgoingArrows);
            var landingTime = GetLandingTime(node, handCount, outgoingArrows);
            if (!throwsByHand.TryGetValue(HandKey(landingHand), out var candidates))
                throw new InvalidOperationException(
                    $"No positive throw exists on landing hand {landingHand.Person.Name}/{landingHand.Name}."
                );

            var next = candidates
                .Select(candidate =>
                {
                    var shift = decimal.ToInt32(
                        decimal.Ceiling((landingTime - candidate.Node.Time) / period)
                    );
                    shift = Math.Max(0, shift);
                    return (
                        candidate.Index,
                        Shift: shift,
                        Time: candidate.Node.Time + shift * period
                    );
                })
                .OrderBy(candidate => candidate.Time)
                .First();
            if (
                candidates.Count(candidate =>
                    candidate.Node.Time
                        + Math.Max(
                            0,
                            decimal.ToInt32(
                                decimal.Ceiling((landingTime - candidate.Node.Time) / period)
                            )
                        ) * period
                    == next.Time
                ) != 1
            )
                throw new InvalidOperationException(
                    $"Landing from {key} has more than one next throw on its target hand."
                );

            successor[index] = next.Index;
            periodShift[index] = next.Shift;
        }

        // Every physical landing feeds one throw and every throw consumes one landing, so the relation is a permutation.
        var indegrees = new int[positiveThrows.Length];
        foreach (var target in successor)
            indegrees[target]++;
        if (indegrees.Any(degree => degree != 1))
            throw new InvalidOperationException(
                "The periodic causal landing relation is not a permutation of positive throws."
            );

        var visited = new bool[positiveThrows.Length];
        var fullPeriodMultiplier = 1;
        var seedingCompletionTime = 0m;
        for (var start = 0; start < positiveThrows.Length; start++)
        {
            if (visited[start])
                continue;

            var cycle = new List<int>();
            var current = start;
            do
            {
                visited[current] = true;
                cycle.Add(current);
                current = successor[current];
            } while (current != start);

            var windings = cycle.Sum(index => periodShift[index]);
            if (windings <= 0)
                throw new InvalidOperationException(
                    "A periodic prop path must advance through time."
                );
            fullPeriodMultiplier = LeastCommonMultiple(fullPeriodMultiplier, windings);

            // A quotient cycle with W period shifts lifts to W prop paths; each starts at its first throw in its W-period window.
            for (var initialShift = 0; initialShift < windings; initialShift++)
            {
                var phase = initialShift;
                var first = cycle
                    .Select(index =>
                    {
                        var eventData = (
                            Index: index,
                            Shift: phase,
                            Time: positiveThrows[index].Time + phase * period
                        );
                        phase = (phase + periodShift[index]) % windings;
                        return eventData;
                    })
                    .MinBy(eventData => eventData.Time);
                seedingCompletionTime = Math.Max(seedingCompletionTime, first.Time);

                var hand = positiveThrows[first.Index].Hand;
                var currentCount = counts[hand.Person.Name];
                counts[hand.Person.Name] =
                    hand.Name == "L"
                        ? currentCount with
                        {
                            Left = currentCount.Left + 1,
                        }
                        : currentCount with
                        {
                            Right = currentCount.Right + 1,
                        };
            }
        }

        var startProps = counts.ToImmutableDictionary(StringComparer.Ordinal);
        var propCount = startProps.Values.Sum(count => count.Left + count.Right);
        if (propCount != totalHeight / diagram.StepCount)
            throw new InvalidOperationException(
                $"Periodic causal paths account for {propCount} props, but throw heights require {totalHeight / diagram.StepCount}."
            );

        return new CausalStartPropsResult(startProps, fullPeriodMultiplier, seedingCompletionTime);
    }

    private static int LeastCommonMultiple(int left, int right) =>
        checked(left / GreatestCommonDivisor(left, right) * right);

    private static int GreatestCommonDivisor(int left, int right)
    {
        while (right != 0)
            (left, right) = (right, left % right);
        return Math.Abs(left);
    }

    private static Hand GetLandingHand(
        DiagramThrow node,
        DiagramSet diagram,
        IReadOnlyDictionary<NodeKey, DiagramArrow[]> outgoingArrows
    )
    {
        if (outgoingArrows.TryGetValue(Key(node), out var arrows) && arrows.Length == 1)
            return arrows[0].To.Hand;

        throw new InvalidOperationException($"Throw at {node.Time} has no causal target hand.");
    }

    private static decimal GetLandingTime(
        DiagramThrow node,
        decimal handCount,
        IReadOnlyDictionary<NodeKey, DiagramArrow[]> outgoingArrows
    ) =>
        outgoingArrows.TryGetValue(Key(node), out var arrows) && arrows.Length == 1
            ? arrows[0].To.Time + handCount
            : throw new InvalidOperationException($"Throw at {node.Time} has no causal target.");

    private static string HandKey(Hand hand) => $"{hand.Person.Name}/{hand.Name}";

    /// <summary>
    /// Completes the causal-root estimate with the remaining clubs, assigned in natural
    /// hand order: jugglers alphabetically from top to bottom, right hand before left.
    /// </summary>
    public static ImmutableDictionary<string, StartProps> CalculateWithNaturalRemainder(
        IReadOnlyList<DiagramThrow> throws,
        IReadOnlyList<DiagramArrow> causalArrows,
        IReadOnlyList<Person> jugglers,
        int totalProps
    )
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalProps);
        var counts = Calculate(throws, causalArrows, jugglers)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var remainder = totalProps - counts.Values.Sum(count => count.Left + count.Right);
        // Preserve overfull root estimates; there is no remainder to distribute.
        if (remainder <= 0)
            return counts.ToImmutableDictionary(StringComparer.Ordinal);

        var hands = jugglers
            .OrderBy(juggler => juggler.Name, StringComparer.Ordinal)
            .SelectMany(juggler =>
                new[] { (juggler.Name, IsRight: true), (juggler.Name, IsRight: false) }
            )
            .ToArray();
        for (var index = 0; index < remainder; index++)
        {
            var (juggler, isRight) = hands[index % hands.Length];
            var current = counts[juggler];
            counts[juggler] = isRight
                ? current with
                {
                    Right = current.Right + 1,
                }
                : current with
                {
                    Left = current.Left + 1,
                };
        }

        return counts.ToImmutableDictionary(StringComparer.Ordinal);
    }

    public static ImmutableDictionary<string, StartProps> Calculate(
        IReadOnlyList<DiagramThrow> throws,
        IReadOnlyList<DiagramArrow> causalArrows,
        IReadOnlyList<Person> jugglers
    )
    {
        ArgumentNullException.ThrowIfNull(throws);
        ArgumentNullException.ThrowIfNull(causalArrows);
        ArgumentNullException.ThrowIfNull(jugglers);

        var incoming = causalArrows
            .Select(arrow => new NodeKey(arrow.To.Time, arrow.ToJuggler.Name, arrow.To.Hand.Name))
            .ToHashSet();
        var outgoing = causalArrows.Select(arrow => Key(arrow.From)).ToHashSet();
        var counts = jugglers.ToDictionary(
            juggler => juggler.Name,
            _ => new StartProps(Left: 0, Right: 0),
            StringComparer.Ordinal
        );

        foreach (var node in throws)
        {
            if (!outgoing.Contains(Key(node)) || incoming.Contains(Key(node)))
                continue;

            var current = counts[node.Hand.Person.Name];
            counts[node.Hand.Person.Name] =
                node.Hand.Name == "L"
                    ? current with
                    {
                        Left = current.Left + 1,
                    }
                    : current with
                    {
                        Right = current.Right + 1,
                    };
        }

        return counts.ToImmutableDictionary(StringComparer.Ordinal);
    }

    private static NodeKey Key(DiagramThrow node) =>
        new(node.Time, node.Hand.Person.Name, node.Hand.Name);

    private readonly record struct NodeKey(decimal Time, string Juggler, string Hand);
}

public sealed record CausalStartPropsResult(
    ImmutableDictionary<string, StartProps> StartProps,
    int FullPeriodMultiplier,
    decimal SeedingCompletionTime
);
