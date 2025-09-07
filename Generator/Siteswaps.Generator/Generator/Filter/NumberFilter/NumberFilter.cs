namespace Siteswaps.Generator.Generator.Filter.NumberFilter;

internal abstract class NumberFilter(IEnumerable<int> number, int amount) : ISiteswapFilter
{
    public bool CanFulfill(PartialSiteswap value)
    {
        if (value.RotationIndex != 0)
        {
            // we check every rotatition and succeed if one matches
            return false;
        }

        return CanFulfillNumberFilter(value);
    }

    private protected abstract bool CanFulfillNumberFilter(PartialSiteswap value);
    protected HashSet<int> Number { get; } = number.ToHashSet();
    protected int Amount { get; } = amount;
    public int Order => 0;
}
