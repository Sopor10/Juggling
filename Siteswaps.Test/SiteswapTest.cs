using FluentAssertions;
using NUnit.Framework;

namespace Siteswaps.Test
{
    public class SiteswapTest
    {
        [Test]
        [TestCase(4,4,1)]
        [TestCase(5,3,1)]
        [TestCase(1)]
        [TestCase(9,7,5,3,1)]
        [TestCase(7,5,6,6)]
        public void ValidSiteswaps(params int[] siteswap)
        {
            var result = Siteswap.TryCreate(siteswap, out var sut);

            result.Should().BeTrue();
        }
        
        [Test]
        [TestCase(4,3)]
        [TestCase(2,1)]
        public void InvalidSideswaps(params int[] siteswap)
        {
            var result = Siteswap.TryCreate(siteswap, out var sut);

            result.Should().BeFalse();
        }
    }
}