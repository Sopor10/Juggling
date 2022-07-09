using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain.Filter;

internal class TrivialSiteswapFilter : ISiteswapFilter
{
    public bool CanFulfill(IPartialSiteswap value)
    {
        if (value.IsFilled())
        {
            return value.Items[0] != value.Items[^1];
        }

        return true;
    }
}