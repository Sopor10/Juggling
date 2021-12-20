using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Filter.NumberFilter;

internal class AtLeastXXXTimesFilter : NumberFilter
{
    public AtLeastXXXTimesFilter(int number, int amount) : base(number, amount)
    {
    }

    private protected override bool CanFulfillNumberFilter(IPartialSiteswap value) 
        => value.Items.Count(x => x == Number || x == PartialSiteswap.Free) >= Amount;
}