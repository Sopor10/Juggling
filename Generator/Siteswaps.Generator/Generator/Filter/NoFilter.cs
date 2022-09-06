namespace Siteswaps.Generator.Generator.Filter;

internal class NoFilter : ISiteswapFilter
{
    public bool CanFulfill(PartialSiteswap value) => true;
}
