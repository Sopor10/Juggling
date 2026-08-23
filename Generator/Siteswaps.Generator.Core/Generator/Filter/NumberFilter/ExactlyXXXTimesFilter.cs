using System.Diagnostics;

namespace Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

[DebuggerDisplay("Exactly {Amount} {Number}s")]
public class ExactlyXXXTimesFilter(IEnumerable<int> number, int amount)
    : NumberFilter(number, amount)
{
    private protected override bool CanFulfillNumberFilter(PartialSiteswap value)
    {
        if (HasSingleNumber)
        {
            return CanFulfillSingleNumber(value);
        }

        int exactCount = 0;
        int possibleCount = 0;
        for (var index = 0; index < value.Length; index++)
        {
            var x = value.Items[index];
            if (ContainsNumber(x))
            {
                exactCount++;
                possibleCount++;
            }
            else if (x == -1)
            {
                possibleCount++;
            }
        }
        return exactCount <= Amount && possibleCount >= Amount;
    }

    private bool CanFulfillSingleNumber(PartialSiteswap value)
    {
        int exactCount = 0;
        int possibleCount = 0;
        for (var index = 0; index < value.Length; index++)
        {
            var x = value.Items[index];
            if (x == SingleNumber)
            {
                exactCount++;
                possibleCount++;
            }
            else if (x == -1)
            {
                possibleCount++;
            }
        }
        return exactCount <= Amount && possibleCount >= Amount;
    }
}
