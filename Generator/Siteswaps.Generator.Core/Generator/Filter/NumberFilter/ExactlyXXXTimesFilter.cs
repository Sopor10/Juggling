using System.Diagnostics;

namespace Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

[DebuggerDisplay("Exactly {Amount} {Number}s")]
public class ExactlyXXXTimesFilter(IEnumerable<int> number, int amount)
    : NumberFilter(number, amount)
{
    private protected override bool CanFulfillNumberFilter(PartialSiteswap value)
    {
        int exactCount = 0;
        int possibleCount = 0;
        foreach (var x in value.AsSpan())
        {
            if (Number.Contains(x))
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
