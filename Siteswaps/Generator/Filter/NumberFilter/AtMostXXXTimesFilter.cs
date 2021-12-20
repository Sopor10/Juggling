using System.Linq;
using Siteswaps.Generator.Api;

namespace Siteswaps.Generator.Filter.NumberFilter;

internal class AtMostXXXTimesFilter : NumberFilter
{
    public AtMostXXXTimesFilter(int number, int amount): base(number, amount)
    {
    }

    private protected override bool CanFulfillNumberFilter(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput)
    {
        return value.Items.Count(x => x == Number) <= Amount;
    }
}