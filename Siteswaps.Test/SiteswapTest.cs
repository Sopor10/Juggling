using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Siteswaps.Test
{
    public class SiteswapTest
    {
        [Test]
        public void UniqueRepresentationOfSiteswap()
        {
            Siteswap.TryCreate(new[]{4, 4, 1}, out var siteswap1);
            Siteswap.TryCreate(new[]{4, 1, 4}, out var siteswap2);

            siteswap1.Should().Be(siteswap2);
        }

        [Test]
        public void Siteswap_Unique_Representation()
        {
            Siteswap.TryCreate(new[]{4, 1, 4}, out var siteswap1);

            siteswap1.Items.Enumerate(1).Select(x => x.value).Should().BeEquivalentTo(new[]{4,4,1});
        }

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