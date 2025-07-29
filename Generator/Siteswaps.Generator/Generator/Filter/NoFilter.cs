namespace Siteswaps.Generator.Generator.Filter;

internal class NoFilter : ISiteswapFilter
{
    public bool CanFulfill(PartialSiteswap value) => true;

    public int Order => 0;
}
