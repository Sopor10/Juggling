namespace Siteswaps.Generator.Test;

using FluentAssertions;
using Generator;
using NUnit.Framework;

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
    [TestCase(0,0)]
    [TestCase(1,0)]
    [TestCase(2,1)]
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
    
    [Test]
    [TestCase(0,Hand.Right)]
    [TestCase(1,Hand.Right)]
    [TestCase(2,Hand.Right)]
    public void Test6(int position, Hand hand)
    {
        var localSiteswap = Siteswap.CreateFromCorrect(7,5,6).GetLocalSiteswap(position, 3);

        localSiteswap.GetThrowType(0).Hand.Should().Be(hand);
    }

    [Test]
    [TestCase(0,1)]
    [TestCase(1,0)]
    [TestCase(2,2)]
    public void Test7(int position, int juggler)
    {
        var localSiteswap = Siteswap.CreateFromCorrect(7,5,6).GetLocalSiteswap(position, 3);

        localSiteswap.GetThrowType(0).Juggler.Should().Be(juggler);
    }
}
