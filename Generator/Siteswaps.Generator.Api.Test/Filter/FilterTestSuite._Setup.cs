using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Api.Test.Filter;

public abstract partial class FilterTestSuite
{
    protected abstract IPartialSiteswap AsPartialSiteswap(sbyte[] values);
    protected abstract IFilterBuilder FilterBuilder { get; }
    protected abstract void ConfigureSiteswapGeneratorInput(SiteswapGeneratorInput input);
}
