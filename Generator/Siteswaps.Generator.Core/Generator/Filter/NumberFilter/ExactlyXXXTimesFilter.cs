using System.Diagnostics;

namespace Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

[DebuggerDisplay("Exactly {Amount} {Number}s")]
public class ExactlyXXXTimesFilter(IEnumerable<int> number, int amount)
    : NumberFilter(number, amount)
{
    private protected override bool CanFulfillNumberFilter(PartialSiteswap value)
    {
        if (value.Items.Count(x => Number.Contains(x)) > Amount)
        {
            return false;
        }
        return value.Items.Count(x => Number.Contains(x) || x == -1) >= Amount;
    }
}
