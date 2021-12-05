namespace Siteswaps.Generator.Filter;

internal class NoFilter : ISiteswapFilter
{
    public bool CanFulfill(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput)
    {
        return true;
    }
}