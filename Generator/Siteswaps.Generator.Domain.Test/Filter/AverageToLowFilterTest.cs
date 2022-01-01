using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Domain.Filter;

namespace Siteswaps.Generator.Domain.Test.Filter;

public class AverageToLowFilterTest
{
    [Test]
    [TestCase(new[]{3,0,-1})]
    [TestCase(new[]{1,1,-1})]
    public void Average_Is_To_Low(int[] input)
    {
        var sut = new FilterFactory(new SiteswapGeneratorInput(3, 3, 0, 5)).AverageToLowFilter();
        var result = sut.CanFulfill(new PartialSiteswap(input));

        result.Should().BeFalse();
    }
        
    [Test]
    [TestCase(new[]{5,4,-1})]
    [TestCase(new[]{8,0,-1})]
    [TestCase(new[]{5,1,-1})]
    public void Average_Is_Okay(int[] input)
    {
        var sut = new FilterFactory(new SiteswapGeneratorInput(3, 3, 0, 5)).AverageToLowFilter();
        var result = sut.CanFulfill(new PartialSiteswap(input));

        result.Should().BeTrue();
    }
}