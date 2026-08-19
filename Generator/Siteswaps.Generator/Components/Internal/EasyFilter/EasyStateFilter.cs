using System.Collections.Immutable;
using Siteswaps.Generator.Components.State;

namespace Siteswaps.Generator.Components.Internal.EasyFilter;

/// <summary>Marker holding state-filter model types used by the wizard.</summary>
public static class EasyStateFilter
{
    /// <summary>
    /// Beat count for the state chip grid: max juggler-scaled height among
    /// <paramref name="allowedThrows"/> (same basis as generator MaxHeight).
    /// </summary>
    public static int MaxBeatFromAllowedThrows(
        IReadOnlyList<Throw> allowedThrows,
        int numberOfJugglers,
        bool showThrowNames
    )
    {
        if (allowedThrows.Count == 0)
        {
            return 1;
        }

        var useLiteralValue = showThrowNames is false;
        var jugglers = Math.Max(1, numberOfJugglers);
        var max = allowedThrows
            .SelectMany(t => t.GetHeightForJugglers(jugglers, useLiteralValue))
            .DefaultIfEmpty(1)
            .Max();
        return Math.Max(1, max);
    }

    public static StateFilter NewDraft(int length) =>
        new([.. new bool[Math.Max(1, length)]]);

    /// <summary>
    /// Resize to <paramref name="length"/>: pad with free beats or truncate,
    /// keeping occupied bits that still fit.
    /// </summary>
    public static StateFilter FitToLength(StateFilter filter, int length)
    {
        length = Math.Max(1, length);
        if (filter.Items.Length == length)
        {
            return filter;
        }

        var items = new bool[length];
        var copyLength = Math.Min(filter.Items.Length, length);
        filter.Items.AsSpan(0, copyLength).CopyTo(items);
        return new StateFilter([.. items]);
    }

    public sealed record StateFilter(ImmutableArray<bool> Items) : IFilterInformation
    {
        public string Display() => "State: " + string.Join(", ", Items.Select(x => x ? "1" : "0"));

        public string Notation() => string.Join(" ", Items.Select(x => x ? "x" : "_"));
    }
}
