namespace Siteswaps.Generator.Core.Generator.Filter;

public class NoFilter : ISiteswapFilter
{
    public bool CanFulfill(PartialSiteswap value) => true;

    public int Order => 0;
}
