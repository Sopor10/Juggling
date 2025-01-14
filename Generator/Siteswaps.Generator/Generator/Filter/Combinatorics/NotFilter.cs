namespace Siteswaps.Generator.Generator.Filter.Combinatorics;

internal class NotFilter : ISiteswapFilter
{
    public NotFilter(ISiteswapFilter filter)
    {
        Filter = filter;
    }

    public bool CanFulfill(PartialSiteswap value)
    {
        return !Filter.CanFulfill(value);
    }

    private ISiteswapFilter Filter { get; }
}
