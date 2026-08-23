using System.Collections.Immutable;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Core.Generator.Filter;

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
        new([.. Enumerable.Repeat(StateValue.DontCare, Math.Max(1, length))]);

    internal static StateFilter DefaultStateFilter(int maxThrowHeight) => NewDraft(maxThrowHeight);

    /// <summary>Cycles don't-care to occupied, occupied to free, and free back to don't-care.</summary>
    public static StateValue Cycle(StateValue state) =>
        state switch
        {
            StateValue.DontCare => StateValue.Occupied,
            StateValue.Occupied => StateValue.Free,
            StateValue.Free => StateValue.DontCare,
            _ => throw new ArgumentOutOfRangeException(nameof(state)),
        };

    /// <summary>
    /// Resize to <paramref name="length"/>: pad with don't-care beats or truncate,
    /// keeping values that still fit.
    /// </summary>
    public static StateFilter FitToLength(StateFilter filter, int length)
    {
        length = Math.Max(1, length);
        if (filter.Items.Length == length)
        {
            return filter;
        }

        var items = Enumerable.Repeat(StateValue.DontCare, length).ToArray();
        var copyLength = Math.Min(filter.Items.Length, length);
        filter.Items.AsSpan(0, copyLength).CopyTo(items);
        return new StateFilter([.. items]);
    }

    public sealed record StateFilter(ImmutableArray<StateValue> Items) : IFilterInformation
    {
        public StateFilter(params bool[] items)
            : this([.. items.Select(item => item ? StateValue.Occupied : StateValue.Free)]) { }

        public string Display() => "State: " + string.Join(", ", Items.Select(DisplayValue));

        public string Notation() => string.Join(" ", Items.Select(NotationValue));

        private static string DisplayValue(StateValue value) =>
            value switch
            {
                StateValue.Occupied => "1",
                StateValue.Free => "0",
                StateValue.DontCare => "*",
                _ => throw new ArgumentOutOfRangeException(nameof(value)),
            };

        private static string NotationValue(StateValue value) =>
            value switch
            {
                StateValue.Occupied => "x",
                StateValue.Free => "_",
                StateValue.DontCare => "*",
                _ => throw new ArgumentOutOfRangeException(nameof(value)),
            };
    }
}
