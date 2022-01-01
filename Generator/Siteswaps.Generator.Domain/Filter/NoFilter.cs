using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain.Filter;

internal class NoFilter : ISiteswapFilter
{
    public bool CanFulfill(IPartialSiteswap value) => true;
}