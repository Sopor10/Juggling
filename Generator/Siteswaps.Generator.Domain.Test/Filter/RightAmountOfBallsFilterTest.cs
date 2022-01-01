using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Domain.Filter;

namespace Siteswaps.Generator.Domain.Test.Filter;

public class RightAmountOfBallsFilterTest
{
    [Test]
    [TestCase(new[]{5,5,5})]
    [TestCase(new[]{4,1,1})]
    public void Number_Of_Balls_Is_Wrong(int[] input)
    {
        var sut = new FilterFactory(new SiteswapGeneratorInput(3, 3, 0, 10)).RightAmountOfBallsFilter();
        var result = sut.CanFulfill(new PartialSiteswap(input));

        result.Should().BeFalse();
    }
        
    [Test]
    [TestCase(new[]{5,3,1})]
    [TestCase(new[]{9,0,0})]
    public void Number_Of_Balls_Is_Correct(int[] input)
    {
        var sut = new FilterFactory(new SiteswapGeneratorInput(3, 3, 0, 5)).RightAmountOfBallsFilter();
        var result = sut.CanFulfill(new PartialSiteswap(input));

        result.Should().BeTrue();
    }
}
