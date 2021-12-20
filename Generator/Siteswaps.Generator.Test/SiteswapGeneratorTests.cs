using Siteswaps.Generator.Api;

namespace Siteswaps.Generator.Test;

public class SiteswapGeneratorTests : SiteswapGeneratorTestSuite
{
    protected override ISiteswapGenerator CreateTestObject() => new SiteswapGeneratorFactory().Create();
}