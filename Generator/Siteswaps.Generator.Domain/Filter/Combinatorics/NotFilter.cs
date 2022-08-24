using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain.Filter.Combinatorics;

internal class NotFilter : ISiteswapFilter{
    public NotFilter(ISiteswapFilter filter)
    {
        Filter = filter;
    }

    public bool CanFulfill(IPartialSiteswap value)
    {
        return !Filter.CanFulfill(value);
    }

    private ISiteswapFilter Filter { get; }
}