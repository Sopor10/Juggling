using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator;
using Siteswaps.Generator.Filter;

namespace Siteswaps.Test.Generator.Filter;

public class CollisionFilterTest
{
    [Test]
    [TestCase(new[]{5,4,-1})]
    [TestCase(new[]{8,4,-1})]
    public void CollisionFilter_Detects_Collisions(int[] input)
    {
        var sut = new FilterFactory().CollisionFilter();
        var result = sut.CanFulfill(new PartialSiteswap(input), new SiteswapGeneratorInput(3, 3, 0, 10));

        result.Should().BeFalse();
    }
        
    [Test]
    [TestCase(new[]{5,3,-1})]
    [TestCase(new[]{8,3,-1})]
    public void CollisionFilter_Detects_No_Collisions(int[] input)
    {
        var sut = new FilterFactory().CollisionFilter();
        var result = sut.CanFulfill(new PartialSiteswap(input), new SiteswapGeneratorInput(3, 3, 0, 5));

        result.Should().BeTrue();
    }
}