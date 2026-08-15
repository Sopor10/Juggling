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

        public string Display() => $"{Type} {Amount} {Throw.DisplayValue}";
    }

    public enum NumberFilterType
    {
        Exactly,
        Maximum,
        AtLeast,
    }
}
