using Siteswaps.Generator;
using Siteswaps.Generator.Filter;

namespace Siteswaps.Test.Generator;

public class SiteswapGeneratorTestsWithNoFilter : SiteswapGeneratorTestSuite
{
    protected override ISiteswapGenerator CreateTestObject() => new SiteswapGenerator();

    protected virtual ISiteswapFilter Filter() => new NoFilter();
}