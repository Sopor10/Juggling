namespace Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

internal class AtMostXXXTimesFilter(IEnumerable<int> number, int amount)
    : NumberFilter(number, amount)
{
    private protected override bool CanFulfillNumberFilter(PartialSiteswap value)
    {
        int count = 0;
        foreach (var x in value.AsSpan())
        {
            if (Number.Contains(x))
            {
                if (++count > Amount)
                    return false;
            }
        }
        return true;
    }
}
