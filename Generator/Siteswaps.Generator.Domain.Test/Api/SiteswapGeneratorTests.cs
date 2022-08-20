using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Test;
using Siteswaps.Generator.Domain.Filter;

namespace Siteswaps.Generator.Domain.Test.Api;

public class NewSiteswapGeneratorTests : SiteswapGeneratorTestSuite
{
    protected override ISiteswapGenerator CreateTestObject(SiteswapGeneratorInput input) => 
        new SiteswapGeneratorFactory(new FilterBuilderFactory())
            .WithInput(input)
            .Create();
}

public class PartialSiteswapTests
{

    [Test]
    [TestCase(new sbyte[]{4,4,4}, 2, 3, ExpectedResult = false)]
    [TestCase(new sbyte[]{4,4,-1}, 2, 3, ExpectedResult = false)]
    [TestCase(new sbyte[]{5,3,4}, 2, 1, ExpectedResult = true)]
    [TestCase(new sbyte[]{5,3,4}, 2, 3, ExpectedResult = false)]
    public bool METHOD(sbyte[] items, sbyte lastFilledPosition, sbyte throwHeight)
    {
        return new PartialSiteswap(items, lastFilledPosition)
            .FillCurrentPosition(throwHeight);
    }

    [Test]
    public void PartialSum_Should_Be_Correct()
    {
        var sut = new PartialSiteswap(new sbyte[] { 5, 3, 1 }, 2);
        sut.FillCurrentPosition(3);
        sut.FillCurrentPosition(1);
        sut.PartialSum.Should().Be(9);
    }

    [Test]
    public void ByteTest()
    {
        byte byte1 = 1;
        byte byte2 = 2;
        (byte1 + byte2).Should().Be(3);

        (byte1 + 1).Should().Be(2);
    }
}