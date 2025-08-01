using FluentAssertions;
using Siteswap.Details.StateDiagram;

namespace Siteswaps.Test;

public class SiteswapTest
{
    [Test]
    public void UniqueRepresentationOfSiteswap()
    {
        var s1 = Siteswap.Details.Siteswap.ToUniqueRepresentation(new[] { 4, 4, 1 });
        var s2 = Siteswap.Details.Siteswap.ToUniqueRepresentation(new[] { 4, 1, 4 });
        s1.EnumerateValues(1).Should().Equal(s2.EnumerateValues(1));
    }

    [Test]
    public void UniqueRepresentationOfSiteswap2()
    {
        var s1 = Siteswap.Details.Siteswap.ToUniqueRepresentation(new[] { 13, 8, 13, 9, 3, 6, 4 });
        var s2 = Siteswap.Details.Siteswap.ToUniqueRepresentation(new[] { 13, 9, 3, 6, 4, 13, 8 });

        s1.EnumerateValues(1).Should().Equal(s2.EnumerateValues(1));
    }

    [Test]
    public void DifferentUniqueRepresentation()
    {
        var s1 = Siteswap.Details.Siteswap.ToUniqueRepresentation(new[] { 3, 1, 5 });

        s1.EnumerateValues(1).Should().NotEqual(new[] { 3, 1, 5 });
    }

    [Test]
    public void UniqueRepresentationOfSiteswapShouldWork()
    {
        var s1 = Siteswap.Details.Siteswap.ToUniqueRepresentation(new[] { 13, 8, 13, 9, 3, 6, 4 });

        var enumerateValues = s1.EnumerateValues(1).ToList();
        enumerateValues.First().Should().Be(13);
        enumerateValues.Skip(1).First().Should().Be(9);
    }

    [Test]
    [TestCase(4, 4, 1)]
    [TestCase(5, 3, 1)]
    [TestCase(1)]
    [TestCase(9, 7, 5, 3, 1)]
    [TestCase(7, 5, 6, 6)]
    public void ValidSiteswaps(params int[] siteswap)
    {
        var result = Siteswap.Details.Siteswap.TryCreate(siteswap, out var sut);

        result.Should().BeTrue();
    }

    [Test]
    [TestCase(4, 3)]
    [TestCase(2, 1)]
    public void InvalidSideswaps(params int[] siteswap)
    {
        var result = Siteswap.Details.Siteswap.TryCreate(siteswap, out var sut);

        result.Should().BeFalse();
    }

    [Test]
    [TestCase("441")]
    [TestCase("531")]
    [TestCase("1")]
    [TestCase("97531")]
    [TestCase("7566")]
    [TestCase("aaa00")]
    public void ValidSiteswaps(string siteswap)
    {
        var result = Siteswap.Details.Siteswap.TryCreate(siteswap, out var sut);

        result.Should().BeTrue();
    }

    [Test]
    [TestCase("43")]
    [TestCase("21")]
    [TestCase("")]
    public void InvalidSideswaps(string siteswap)
    {
        var result = Siteswap.Details.Siteswap.TryCreate(siteswap, out var sut);

        result.Should().BeFalse();
    }

    [Test]
    public void Calculate_Transitions()
    {
        var sut = new Siteswap.Details.Siteswap(3);
        var to = new Siteswap.Details.Siteswap(5, 3, 1);
        var transitions = sut.PossibleTransitions(to, 3, 5);

        transitions.Should().HaveCount(9);
    }

    [Test]
    public void Create_Specific_Transition_Length_1()
    {
        var from = new Siteswap.Details.Siteswap(5, 3, 1);
        var to = new Siteswap.Details.Siteswap(4, 1, 4);
        var length = 1;

        var transitions = from.PossibleTransitions(to, length);

        transitions
            .Should()
            .ContainSingle()
            .Which.Throws.Should()
            .ContainSingle()
            .Which.Should()
            .Be(4);
    }

    [Test]
    public void Create_Specific_Transition_Length_2()
    {
        var from = new Siteswap.Details.Siteswap(5, 3, 1);
        var to = new Siteswap.Details.Siteswap(4, 1, 4);
        var length = 2;

        var transitions = from.PossibleTransitions(to, length);

        transitions.Where(x => x.Throws.Length == 2).Should().HaveCount(2);
    }

    [Test]
    public void Create_Specific_Transition__Should_Find_All()
    {
        var from = new Siteswap.Details.Siteswap(5, 3, 1);
        var to = new Siteswap.Details.Siteswap(4, 1, 4);
        var length = 3;

        var transitions = from.PossibleTransitions(to, length);

        transitions.Should().HaveCount(7);
    }

    [Test]
    public void State_Works()
    {
        var sut = new Siteswap.Details.Siteswap(0, 3, 0).State;
        sut.Should().Be(new State(0, 1));
    }

    [Test]
    public void State_Should_Be_Able_To_Throw_A_0()
    {
        var sut = new Siteswap.Details.Siteswap(0, 3, 0).State;

        var state = sut.Advance().Throw(0);
        state.Should().Be(new State(1));
    }
}
