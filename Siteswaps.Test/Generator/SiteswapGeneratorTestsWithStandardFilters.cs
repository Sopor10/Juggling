using Siteswaps.Generator;
using Siteswaps.Generator.Filter;

namespace Siteswaps.Test.Generator
{
    public class SiteswapGeneratorTestsWithStandardFilters : SiteswapGeneratorTestSuite
    {
        protected override ISiteswapGenerator CreateTestObject() => new SiteswapGenerator();

        protected override ISiteswapFilter Filter() => ISiteswapFilter.Standard();
    }
}