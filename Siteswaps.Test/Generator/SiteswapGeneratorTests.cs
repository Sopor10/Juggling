using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator;
using Siteswaps.Generator.Filter;

namespace Siteswaps.Test.Generator;

public class SiteswapGeneratorTests : SiteswapGeneratorTestSuite
{
    protected override ISiteswapGenerator CreateTestObject() => new SiteswapGenerator();

    [Test]
    [TestCase(7, 13, 2, 8)]
    [TestCase(10, 10, 2, 8)]

    public void There_Should_Be_No_Multiple_Inserts(int period, int maxHeight, int minHeight, int numberOfObjects)
    {
        var generator = new SiteswapGenerator();
        generator.Generate(new SiteswapGeneratorInput(period, numberOfObjects, minHeight, maxHeight, new NoFilter()));

        generator.Stack.MultipleInserts.Should().BeEmpty();
    }
}