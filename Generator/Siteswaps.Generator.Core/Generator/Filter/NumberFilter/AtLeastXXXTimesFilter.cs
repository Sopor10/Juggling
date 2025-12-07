namespace Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

internal class AtLeastXXXTimesFilter(IEnumerable<int> number, int amount)
    : NumberFilter(number, amount)
{
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
