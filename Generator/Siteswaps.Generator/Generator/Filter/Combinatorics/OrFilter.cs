namespace Siteswaps.Generator.Generator.Filter.Combinatorics;

internal class OrFilter : ISiteswapFilter
{
    private List<ISiteswapFilter> Filters { get; }

    public OrFilter(params IEnumerable<ISiteswapFilter> filters)
    {
        Filters = filters.ToList();
    }

    public bool CanFulfill(PartialSiteswap value) => Filters.Any(x => x.CanFulfill(value));

    public int Order => 0;
}
