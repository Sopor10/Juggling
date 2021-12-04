namespace Siteswaps.Generator.Filter;

public class NoFilter : ISiteswapFilter
{
    public bool CanFulfill(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput)
    {
        return true;
    }
}