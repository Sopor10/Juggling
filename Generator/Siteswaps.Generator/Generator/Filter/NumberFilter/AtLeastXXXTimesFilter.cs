namespace Siteswaps.Generator.Generator.Filter.NumberFilter;

internal class AtLeastXXXTimesFilter : NumberFilter
{
    public AtLeastXXXTimesFilter(IEnumerable<int> number, int amount)
        : base(number, amount) { }

    private protected override bool CanFulfillNumberFilter(PartialSiteswap value)
    {
        int matches = 0;
        foreach (var x in value.Items)
        {
            if (x == -1 || Number.Contains(x))
            {
                if (++matches >= Amount)
                    return true;
            }
        }
        return false;
    }
}
