namespace Siteswaps.Generator.Api;

public class SiteswapGeneratorFactory
{
    public ISiteswapGenerator Create() => new SiteswapGenerator();
}