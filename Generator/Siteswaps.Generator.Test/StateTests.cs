using FluentAssertions;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;

namespace Siteswaps.Generator.Test;

public class StateTests
{
    [Test]
    public void State_Uses_Bit_Positions_In_Its_String_Representation()
    {
        new State(0, 1, 0, 1).ToString().Should().Be("0101");
    }

    [Test]
    public void GroundState_Contains_One_Ball_In_Each_Position()
    {
        State.GroundState(3).ToString().Should().Be("111");
    }

    [TestCase(new[] { 3, 3, 3 }, 3, "111")]
    [TestCase(new[] { 4, 4, 4 }, 4, "1111")]
    public void CalculateState_Reaches_A_Stable_State(
        int[] siteswap,
        int maxHeight,
        string expected
    )
    {
        State
            .CalculateState(new PartialSiteswap(siteswap), maxHeight)
            .ToString()
            .Should()
            .Be(expected);
    }

    [Test]
    public void CalculateState_Uses_The_Current_Rotation()
    {
        var siteswap = new PartialSiteswap([3, 4, 3]) { RotationIndex = 1 };

        State.CalculateState(siteswap, 4).ToString().Should().Be("011");
    }

    [Test]
    public void StateFilter_Advertises_Rotation_Awareness()
    {
        var input = new SiteswapGeneratorInput(3, 3, 0, 10);

        new FilterBuilder(input)
            .WithState(State.GroundState(3))
            .Build()
            .IsRotationAware.Should()
            .BeTrue();
    }

    [Test]
    public void NumberFilter_Advertises_Rotation_Awareness()
    {
        var input = new SiteswapGeneratorInput(3, 3, 0, 10);

        new FilterBuilder(input)
            .MinimumOccurence(new[] { 1 }, 1)
            .Build()
            .IsRotationAware.Should()
            .BeTrue();
    }
}
