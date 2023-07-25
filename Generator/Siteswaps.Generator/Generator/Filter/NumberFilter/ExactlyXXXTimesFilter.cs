using System.Diagnostics;

namespace Siteswaps.Generator.Generator.Filter.NumberFilter;

[DebuggerDisplay("Exactly {Amount} {Number}s")]
internal class ExactlyXXXTimesFilter : NumberFilter
{
    private protected override bool CanFulfillNumberFilter(PartialSiteswap value)
    {
        if (value.Items.Count(x => x == Number) > Amount)
        {
            return false;
        }
        return value.Items.Count(x => x == Number || x == -1) >= Amount;
    }

    public ExactlyXXXTimesFilter(int number, int amount) : base(number, amount)
    {
    }
}