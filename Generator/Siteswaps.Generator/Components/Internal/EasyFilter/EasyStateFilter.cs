using System.Collections.Immutable;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Core.Generator.Filter;

namespace Siteswaps.Generator.Components.Internal.EasyFilter;

/// <summary>Marker holding state-filter model types used by the wizard.</summary>
public static class EasyStateFilter
{
    public record StateFilter(ImmutableArray<StateValue> Items) : IFilterInformation
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
