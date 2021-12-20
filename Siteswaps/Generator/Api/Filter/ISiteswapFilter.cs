namespace Siteswaps.Generator.Api.Filter;

public interface ISiteswapFilter
{
    public bool CanFulfill(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput);
}