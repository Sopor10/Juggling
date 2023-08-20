using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator.Generator;

namespace Siteswaps.Generator.Test;

using Shared;

public class SiteswapTests
{
    [Test]
    [TestCase(new sbyte[]{5,3,1}, 0, 2, "513")]
    [TestCase(new sbyte[]{5,3,1}, 1, 2, "351")]
    [TestCase(new sbyte[]{5,1}, 0, 2, "5")]
    [TestCase(new sbyte[]{5,1}, 1, 2, "1")]
    [TestCase(new sbyte[]{10,10,7,5,2,2}, 0, 2, "a72")]
    [TestCase(new sbyte[]{10,10,7,5,2,2}, 1, 2, "a52")]
    public void Local_Siteswaps(sbyte[] siteswap, int juggler, int maxJugglers, string expected)
    {
        Siteswap.CreateFromCorrect(siteswap).GetLocalSiteswap(juggler, maxJugglers).ToString().Should().Be(expected);
    }

    [Test]
    public void CanCreateInterface()
    {
        Siteswap.CreateFromCorrect(10,7,0,6,7).Interface.Values.EnumerateValues(1).Should().BeEquivalentTo(new[]{10,7,0,7,6});
    }
}
