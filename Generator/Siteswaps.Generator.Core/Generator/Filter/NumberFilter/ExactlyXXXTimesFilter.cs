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
            return CanFulfillSingleNumber(value.AsSpan());
        }

        int exactCount = 0;
        int possibleCount = 0;
        foreach (var x in value.AsSpan())
        {
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

    private bool CanFulfillSingleNumber(Span<int> values)
    {
        int exactCount = 0;
        int possibleCount = 0;
        foreach (var x in values)
        {
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
