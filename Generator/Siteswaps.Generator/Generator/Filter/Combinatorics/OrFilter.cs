namespace Siteswaps.Generator.Generator.Filter.Combinatorics;

internal class OrFilter(params IEnumerable<ISiteswapFilter> filters) : ISiteswapFilter
{
    private List<ISiteswapFilter> Filters { get; } = filters.ToList();

    public bool CanFulfill(PartialSiteswap value) => Filters.Any(x => x.CanFulfill(value));

    public int Order => 0;
}
