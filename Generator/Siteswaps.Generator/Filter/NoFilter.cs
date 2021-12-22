using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Filter;

internal class NoFilter : ISiteswapFilter
{
    public bool CanFulfill(IPartialSiteswap value) => true;
}