using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain.Filter.NumberFilter;

internal class ExactlyXXXTimesFilter : NumberFilter
{
    private protected override bool CanFulfillNumberFilter(IPartialSiteswap value)
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