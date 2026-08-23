using Siteswaps.Generator.Components.State;

namespace Siteswaps.Generator.Components.Internal.EasyFilter;

/// <summary>Marker holding number-filter model types used by the wizard.</summary>
public static class EasyNumberFilter
{
    public record NumberFilter : IFilterInformation
    {
        public required int Amount { get; set; }
        public required NumberFilterType Type { get; set; }
        public required Throw Throw { get; set; }

        /// <summary>
        /// When set, counts only throws by that juggler (0-based).
        /// When null, counts across the whole pattern.
        /// </summary>
        public int? JugglerIndex { get; set; }

        public string Display() =>
            JugglerIndex is { } juggler
                ? $"{Type} {Amount} {Throw.DisplayValue} juggler {(char)('A' + juggler)}"
                : $"{Type} {Amount} {Throw.DisplayValue}";
    }

    public enum NumberFilterType
    {
        Exactly,
        Maximum,
        AtLeast,
    }
}
