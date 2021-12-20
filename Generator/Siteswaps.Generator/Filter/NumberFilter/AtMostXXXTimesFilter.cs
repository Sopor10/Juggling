using System.Linq;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Filter.NumberFilter;

internal class AtMostXXXTimesFilter : NumberFilter
{
    public AtMostXXXTimesFilter(int number, int amount): base(number, amount)
    {
    }

    private protected override bool CanFulfillNumberFilter(IPartialSiteswap value) 
        => value.Items.Count(x => x == Number) <= Amount;
}