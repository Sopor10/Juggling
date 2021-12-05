using System.Linq;

namespace Siteswaps.Generator.Filter.NumberFilter;

internal class ExactlyXXXTimesFilter : NumberFilter
{
    private protected override bool CanFulfillNumberFilter(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput)
    {
        if (value.Items.Count(x => x == Number) > Amount)
        {
            return false;
        }
        return value.Items.Count(x => x == Number || x == PartialSiteswap.Free) >= Amount;
    }

    public ExactlyXXXTimesFilter(int number, int amount) : base(number, amount)
    {
    }
}