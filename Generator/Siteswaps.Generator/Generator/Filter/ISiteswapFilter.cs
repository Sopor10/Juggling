namespace Siteswaps.Generator.Generator.Filter;

public interface ISiteswapFilter
{
    public bool CanFulfill(PartialSiteswap value);
}