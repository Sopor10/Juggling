namespace Siteswaps.Generator.Generator.Filter.NumberFilter;

internal abstract class NumberFilter : ISiteswapFilter
{
    protected NumberFilter(IEnumerable<int> number, int amount)
    {
        Number = number.ToHashSet();
        Amount = amount;
    }

    public bool CanFulfill(PartialSiteswap value)
    {
        if (value.RotationIndex != 0)
        {
            return true;
        }

        return CanFulfillNumberFilter(value);
    }

    private protected abstract bool CanFulfillNumberFilter(PartialSiteswap value);
    protected HashSet<int> Number { get; }
    protected int Amount { get; }
    public int Order => 0;
}
