using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain.Filter;

internal class TrivialSiteswapFilter : ISiteswapFilter
{
    public bool CanFulfill(IPartialSiteswap value) => !value.IsFilled() || value.Items.Any(valueItem => valueItem != value.Items[0]);
}