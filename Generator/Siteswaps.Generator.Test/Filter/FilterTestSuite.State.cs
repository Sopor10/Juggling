using FluentAssertions;
using Siteswaps.Generator.Generator;
using Siteswaps.Generator.Generator.Filter;

namespace Siteswaps.Generator.Test.Filter;

public partial class FilterTestSuite
{
    [Test]
    [TestCase(4, 4, 1)]
    [TestCase(3)]
    [TestCase(5, 3, 1)]
    public void GroundState_Can_Be_Filtered(params int[] input)
    {
        var sut = FilterBuilder.WithState(State.GroundState(3)).Build();
        sut.CanFulfill(new PartialSiteswap(input)).Should().BeTrue();
    }

    [Test]
    [TestCase(4, 1, 4)]
    [TestCase(5, 1)]
    [TestCase(3, 1, 5)]
    public void AreNotGroundState(params int[] input)
    {
        var sut = FilterBuilder.WithState(State.GroundState(3)).Build();
        sut.CanFulfill(new PartialSiteswap(input)).Should().BeFalse();
    }

    [Test]
    [TestCase(9, 3, 4, 5, 6, 7, 8)]
    public void Bla(params int[] input)
    {
        var sut = FilterBuilder.WithState(new State(1, 1, 1, 1, 0, 1, 0, 1)).Build();
        sut.CanFulfill(new PartialSiteswap(input)).Should().BeTrue();
    }
}
