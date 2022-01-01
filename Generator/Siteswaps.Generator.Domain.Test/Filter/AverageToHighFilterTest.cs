using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Domain.Filter;

namespace Siteswaps.Generator.Domain.Test.Filter;

public class AverageToHighFilterTest
{
    [Test]
    [TestCase(new[]{5,5,-1})]
    [TestCase(new[]{8,3,-1})]
    public void Average_Is_To_High(int[] input)
    {
        var sut = new FilterFactory(new SiteswapGeneratorInput(3, 3, 0, 10)).AverageToHighFilter();
        var result = sut.CanFulfill(new PartialSiteswap(input));

        result.Should().BeFalse();
    }
        
    [Test]
    [TestCase(new[]{5,4,-1})]
    [TestCase(new[]{8,0,-1})]
    public void Average_Is_Okay(int[] input)
    {
        var sut = new FilterFactory(new SiteswapGeneratorInput(3, 3, 0, 5)).AverageToHighFilter();
        var result = sut.CanFulfill(new PartialSiteswap(input));

        result.Should().BeTrue();
    }
}