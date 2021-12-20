using Siteswaps.Generator.Api;

namespace Siteswaps.Generator;

public class SiteswapGeneratorFactory : ISiteswapGeneratorFactory
{
    public ISiteswapGenerator Create() => new SiteswapGenerator();
}