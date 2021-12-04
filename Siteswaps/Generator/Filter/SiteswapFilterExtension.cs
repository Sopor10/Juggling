namespace Siteswaps.Generator.Filter;

public static class SiteswapFilterExtension
{
    public static ISiteswapFilter Combine(this ISiteswapFilter source, ISiteswapFilter other) => new FilterList(source, other);
}