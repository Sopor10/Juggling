namespace Siteswaps.Generator.Api.Filter;

public interface ISiteswapFilter
{
    public bool CanFulfill(IPartialSiteswap value);
}