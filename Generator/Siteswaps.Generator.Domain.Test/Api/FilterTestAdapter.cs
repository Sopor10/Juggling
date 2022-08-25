using System;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;
using Siteswaps.Generator.Api.Test.Filter;
using Siteswaps.Generator.Domain.Filter;

namespace Siteswaps.Generator.Domain.Test.Api;

public class FilterTestAdapter : FilterTestSuite
{
    protected override IPartialSiteswap AsPartialSiteswap(sbyte[] values) => new PartialSiteswap(values);

    protected override IFilterBuilder FilterBuilder => new FilterBuilderFactory().Create(Input ?? throw new InvalidOperationException("Please set a Input via ConfigureSiteswapGeneratorInput"));
    protected override void ConfigureSiteswapGeneratorInput(SiteswapGeneratorInput input) => Input = input;

    private SiteswapGeneratorInput? Input { get; set; } = new(3, 3, 0, 10);
}