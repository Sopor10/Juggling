using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Test;
using Siteswaps.Generator.Domain.Filter;

namespace Siteswaps.Generator.Domain.Test.Api;

public class SiteswapGeneratorTests : SiteswapGeneratorTestSuite
{
    protected override ISiteswapGenerator CreateTestObject(SiteswapGeneratorInput input) => 
        new SiteswapGeneratorFactory(new FilterBuilderFactory())
            .WithInput(input)
            .Create();
}