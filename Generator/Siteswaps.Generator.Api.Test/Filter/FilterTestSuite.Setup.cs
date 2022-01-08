using System;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Api.Test.Filter;

public abstract partial class FilterTestSuite
{
    protected abstract IPartialSiteswap AsPartialSiteswap(int[] values);
    protected abstract IFilterBuilder FilterBuilder { get; }
    protected abstract void ConfigureSiteswapGeneratorInput(SiteswapGeneratorInput input);
}
