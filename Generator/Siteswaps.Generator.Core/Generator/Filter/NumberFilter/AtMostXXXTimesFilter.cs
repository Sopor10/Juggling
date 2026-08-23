namespace Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

internal sealed class AtMostXXXTimesFilter(IEnumerable<int> number, int amount)
    : NumberFilter(number, amount)
{
    private protected override bool CanFulfillNumberFilter(PartialSiteswap value)
    {
        int count = 0;
        for (var index = 0; index < value.Length; index++)
        {
            var x = value.Items[index];
            if (ContainsNumber(x))
            {
                if (++count > Amount)
                    return false;
            }
        }
        return true;
    }
}
