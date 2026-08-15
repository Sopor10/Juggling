using System.Collections.Immutable;
using Siteswaps.Generator.Components.State;

namespace Siteswaps.Generator.Components.Internal.EasyFilter;

/// <summary>Marker holding state-filter model types used by the wizard.</summary>
public static class EasyStateFilter
{
    public record StateFilter(ImmutableArray<bool> Items) : IFilterInformation
    {
        public string Display() => "State: " + string.Join(", ", Items.Select(x => x ? "1" : "0"));

        public string Notation() => string.Join(" ", Items.Select(x => x ? "x" : "_"));
    }
}
