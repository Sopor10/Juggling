using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain.Filter;

public static class SiteswapFilterExtension
{
    public static ISiteswapFilter Combine(this ISiteswapFilter source, ISiteswapFilter? other) => new AndFilter(source, other);
}