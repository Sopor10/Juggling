using System.Collections.Immutable;

namespace Siteswap.Details.CausalDiagram;

/// <summary>
/// Evaluates the proposed no-incoming-arrow heuristic for a diagram.
/// This is not a complete cyclic inventory calculation: closed causal loops have no roots,
/// even though each loop still needs a club to seed it.
/// </summary>
public static class CausalStartPropsCalculator
{
    /// <summary>
    /// Completes the causal-root estimate with the remaining clubs, assigned in natural
    /// hand order: jugglers alphabetically from top to bottom, right hand before left.
    /// </summary>
    public static ImmutableDictionary<string, StartProps> CalculateWithNaturalRemainder(
        IReadOnlyList<Throw> throws,
        IReadOnlyList<DiagramArrow> causalArrows,
        IReadOnlyList<Person> jugglers,
        int totalProps
    )
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalProps);
        var counts = Calculate(throws, causalArrows, jugglers).ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal
        );
        var remainder = totalProps - counts.Values.Sum(count => count.Left + count.Right);
        // Preserve overfull root estimates; there is no remainder to distribute.
        if (remainder <= 0)
            return counts.ToImmutableDictionary(StringComparer.Ordinal);

        var hands = jugglers
            .OrderBy(juggler => juggler.Name, StringComparer.Ordinal)
            .SelectMany(juggler => new[] { (juggler.Name, IsRight: true), (juggler.Name, IsRight: false) })
            .ToArray();
        for (var index = 0; index < remainder; index++)
        {
            var (juggler, isRight) = hands[index % hands.Length];
            var current = counts[juggler];
            counts[juggler] = isRight
                ? current with { Right = current.Right + 1 }
                : current with { Left = current.Left + 1 };
        }

        return counts.ToImmutableDictionary(StringComparer.Ordinal);
    }

    public static ImmutableDictionary<string, StartProps> Calculate(
        IReadOnlyList<Throw> throws,
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
        var counts = jugglers.ToDictionary(
            juggler => juggler.Name,
            _ => new StartProps(Left: 0, Right: 0),
            StringComparer.Ordinal
        );

        foreach (var node in throws)
        {
            if (node.Height <= 0 || incoming.Contains(Key(node)))
                continue;

            var current = counts[node.Hand.Person.Name];
            counts[node.Hand.Person.Name] = node.Hand.Name == "L"
                ? current with { Left = current.Left + 1 }
                : current with { Right = current.Right + 1 };
        }

        return counts.ToImmutableDictionary(StringComparer.Ordinal);
    }

    private static NodeKey Key(Throw node) =>
        new(node.Time, node.Hand.Person.Name, node.Hand.Name);

    private readonly record struct NodeKey(decimal Time, string Juggler, string Hand);
}
