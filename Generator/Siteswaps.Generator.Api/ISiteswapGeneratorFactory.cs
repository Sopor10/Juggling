using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Api;

public interface ISiteswapGeneratorFactory
{
    public ISiteswapGeneratorFactory ConfigureFilter(Func<IFilterBuilder, IFilterBuilder> builder);
    ISiteswapGeneratorFactory WithInput(SiteswapGeneratorInput input);
    public ISiteswapGenerator Create();
}