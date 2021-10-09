using System.Collections.Generic;
using Siteswaps.Generator;

namespace Siteswaps.Test
{
    public class SiteswapGeneratorTests : SiteswapGeneratorTestSuite
    {
        protected override ISiteswapGenerator CreateTestObject()
        {
            return new SiteswapGenerator();
        }
    }
}