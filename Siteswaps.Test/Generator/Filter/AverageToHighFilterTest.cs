using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator;
using Siteswaps.Generator.Filter;

namespace Siteswaps.Test.Generator.Filter;

public class AverageToHighFilterTest
{
    [Test]
    [TestCase(new[]{5,5,-1})]
    [TestCase(new[]{8,3,-1})]
    public void Average_Is_To_High(int[] input)
    {
        var sut = new AverageToHighFilter();
        var result = sut.CanFulfill(new PartialSiteswap(input), new SiteswapGeneratorInput(3, 3, 0, 10, new NoFilter()));

        result.Should().BeFalse();
    }
        
    [Test]
    [TestCase(new[]{5,4,-1})]
    [TestCase(new[]{8,0,-1})]
    public void Average_Is_Okay(int[] input)
    {
        var sut = new AverageToHighFilter();
        var result = sut.CanFulfill(new PartialSiteswap(input), new SiteswapGeneratorInput(3, 3, 0, 5, new NoFilter()));

        result.Should().BeTrue();
    }
}