using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator;
using Siteswaps.Generator.Filter;

namespace Siteswaps.Test.Generator.Filter
{
    public class AtMostXXXTimesFilterTest
    {
        [Test]
        [TestCase(new[]{5,5,-1})]
        [TestCase(new[]{8,4,-1})]
        [TestCase(new[]{8,4,3})]
        public void At_Most_Two_Fives_In_Siteswap_Is_True(int[] input)
        {
            var sut = new AtMostXXXTimesFilter(5,2);
            var result = sut.CanFulfill(new PartialSiteswap(input), new SiteswapGeneratorInput(3, 3, 0, 10, new NoFilter()));

            result.Should().BeTrue();
        }
        
        [Test]
        [TestCase(new[]{6,5,5,5,-1})]
        [TestCase(new[]{8,7,5,5,5,4,3,7,-1})]
        [TestCase(new[]{8,7,5,7,5,6,5})]
        public void At_Most_Two_Fives_In_Siteswap_Is_False(int[] input)
        {
            var sut = new AtMostXXXTimesFilter(5,2);
            var result = sut.CanFulfill(new PartialSiteswap(input), new SiteswapGeneratorInput(3, 3, 0, 10, new NoFilter()));

            result.Should().BeFalse();
        }
    }
}