using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator;

public class SiteswapGeneratorFactory : ISiteswapGeneratorFactory
{
    public ISiteswapGenerator Create(ISiteswapFilter filter) => new SiteswapGenerator(filter);
}