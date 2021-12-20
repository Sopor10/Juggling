using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Api;

public interface ISiteswapGeneratorFactory
{
    public ISiteswapGenerator Create(ISiteswapFilter filter);
}