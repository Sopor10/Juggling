using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator;
using Siteswaps.Generator.Filter;

namespace Siteswaps.Test.Generator;

public class SiteswapGeneratorTests : SiteswapGeneratorTestSuite
{
    protected override ISiteswapGenerator CreateTestObject() => new SiteswapGenerator();
}