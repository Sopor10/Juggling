using System.Diagnostics;

namespace Siteswaps.Generator.Generator.Filter.NumberFilter;

[DebuggerDisplay("Exactly {Amount} {Number}s")]
internal class ExactlyXXXTimesFilter : NumberFilter
{
    private protected override bool CanFulfillNumberFilter(PartialSiteswap value)
    {
        if (value.Items.Count(x => Number.Contains(x)) > Amount)
        {
            return false;
        }
        return value.Items.Count(x => Number.Contains(x) || x == -1) >= Amount;
    }

    public ExactlyXXXTimesFilter(IEnumerable<int> number, int amount) : base(number, amount)
    {
    }
}
