using System;
using Siteswaps.Generator.Generator;
using Siteswaps.Generator.Generator.Filter;

namespace Siteswaps.Generator.Test.Filter;

public class FilterTestSuiteBase 
{
    protected IFilterBuilder FilterBuilder => new FilterBuilderFactory().Create(Input ?? throw new InvalidOperationException("Please set a Input via ConfigureSiteswapGeneratorInput"));

    protected SiteswapGeneratorInput? Input { get; set; } = new(3, 3, 0, 10);
}
