namespace Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

internal sealed class AtLeastXXXTimesFilter(IEnumerable<int> number, int amount)
    : NumberFilter(number, amount)
{
    private protected override bool CanFulfillNumberFilter(PartialSiteswap value)
    {
        int matches = 0;
        for (var index = 0; index < value.Length; index++)
        {
            var x = value.Items[index];
            if (x == -1 || ContainsNumber(x))
            {
                if (++matches >= Amount)
                    return true;
            }
        }
        return false;
    }
}
