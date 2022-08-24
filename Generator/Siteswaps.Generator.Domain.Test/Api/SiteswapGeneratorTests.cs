using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Test;
using Siteswaps.Generator.Domain.Filter;
using static VerifyNUnit.Verifier;

namespace Siteswaps.Generator.Domain.Test.Api;

public class NewSiteswapGeneratorTests : SiteswapGeneratorTestSuite
{
    protected override ISiteswapGenerator CreateTestObject(SiteswapGeneratorInput input) => 
        new SiteswapGeneratorFactory(new FilterBuilderFactory())
            .WithInput(input)
            .Create();


    [Test]
    public async Task PatternFilterTest()
    {
        var siteswaps = await new SiteswapGeneratorFactory(new FilterBuilderFactory())
            .WithInput(new SiteswapGeneratorInput(10, 6, 2, 10)
            {
                StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60),1000 )
            })
            .ConfigureFilter(x => x.Pattern(new[]{2,-1,6,-1,5,-1,-1,-1,-1,-1}, 2))
            .Create()
            .GenerateAsync();
        await Verify(siteswaps.Select(x => x.ToString()).ToList());
    }
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