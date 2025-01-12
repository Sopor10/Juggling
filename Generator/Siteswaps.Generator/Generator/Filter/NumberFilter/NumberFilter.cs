namespace Siteswaps.Generator.Generator.Filter.NumberFilter;

internal abstract class NumberFilter : ISiteswapFilter
{
    protected NumberFilter(IEnumerable<int> number, int amount)
    {
        Number = number;
        Amount = amount;
    }
    public bool CanFulfill(PartialSiteswap value)
    {
        return CanFulfillNumberFilter(value);
    }

    private protected abstract bool CanFulfillNumberFilter(PartialSiteswap value);
    protected IEnumerable<int> Number { get; }
    protected int Amount { get; }
}
