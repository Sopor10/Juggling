using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator.Generator;

namespace Siteswaps.Generator.Test;

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
}


public class LocalSiteswapTests
{
    [Test]
    [TestCase(0,1)]
    [TestCase(1,0)]
    [TestCase(2,1)]
    public void Test1(int position, int juggler)
    {
        var localSiteswap = Siteswap.CreateFromCorrect(7, 5, 6).GetLocalSiteswap(0, 2);

        localSiteswap.GetThrowType(position).Juggler.Should().Be(juggler);
    }
    
    [Test]
    [TestCase(0,Hand.Left)]
    [TestCase(1,Hand.Right)]
    [TestCase(2,Hand.Right)]
    public void Test2(int position, Hand hand)
    {
        var localSiteswap = Siteswap.CreateFromCorrect(7, 5, 6).GetLocalSiteswap(0, 2);

        localSiteswap.GetThrowType(position).Hand.Should().Be(hand);

    }
    
    [Test]
    [TestCase(0,1)]
    [TestCase(1,1)]
    [TestCase(2,0)]
    public void Test3(int position, int juggler)
    {
        var localSiteswap = Siteswap.CreateFromCorrect(7, 5, 6).GetLocalSiteswap(1, 2);

        localSiteswap.GetThrowType(position).Juggler.Should().Be(juggler);
    }
    
    [Test]
    [TestCase(0,Hand.Left)]
    [TestCase(1,Hand.Left)]
    [TestCase(2,Hand.Left)]
    public void Test4(int position, Hand hand)
    {
        var localSiteswap = Siteswap.CreateFromCorrect(7, 5, 6).GetLocalSiteswap(1, 2);

        localSiteswap.GetThrowType(position).Hand.Should().Be(hand);
    }
    [Test]
    [TestCase(0,Hand.Right)]
    [TestCase(1,Hand.Left)]
    [TestCase(2,Hand.Right)]
    [TestCase(3,Hand.Right)]
    [TestCase(4,Hand.Left)]
    public void Test5(int position, Hand hand)
    {
        var localSiteswap = Siteswap.CreateFromCorrect(9,7,5,3,1).GetLocalSiteswap(0, 2);

        localSiteswap.GetThrowType(position).Hand.Should().Be(hand);
    }

}
