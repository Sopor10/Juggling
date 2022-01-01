using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain.Filter.NumberFilter;

internal class AtMostXXXTimesFilter : NumberFilter
{
    public AtMostXXXTimesFilter(int number, int amount): base(number, amount)
    {
    }

    private protected override bool CanFulfillNumberFilter(IPartialSiteswap value) 
        => value.Items.Count(x => x == Number) <= Amount;
}