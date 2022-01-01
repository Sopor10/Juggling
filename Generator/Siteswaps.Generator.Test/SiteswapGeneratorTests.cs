using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Test;
using Siteswaps.Generator.Filter;

namespace Siteswaps.Generator.Test;

public class SiteswapGeneratorTests : SiteswapGeneratorTestSuite
{
    protected override ISiteswapGenerator CreateTestObject(SiteswapGeneratorInput input) => 
        new SiteswapGeneratorFactory(new FilterBuilderFactory())
            .WithInput(input)
            .Create();
}