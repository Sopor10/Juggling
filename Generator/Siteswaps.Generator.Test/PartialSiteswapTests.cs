using FluentAssertions;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test;

public class PartialSiteswapTests
{
    [Test]
    public void Landing_Search_Uses_The_Cyclic_Occupancy_State()
    {
        var sut = new PartialSiteswap([1, 1, -1, -1]);

        sut.FindFreeLandingAtOrBefore(3, 0).Should().Be(3);
        sut.FindFreeLandingAtOrBefore(2, 0).Should().Be(0);
        sut.FindFreeLandingAtOrAfter(2, 3).Should().Be(3);
        sut.FindFreeLandingAtOrAfter(2, 2).Should().Be(int.MaxValue);
    }

    [Test]
    public void Landing_Slots_Track_Filled_Throws()
    {
        var sut = new PartialSiteswap([-1, -1, -1, -1]);

        sut.IsLandingFree(3).Should().BeTrue();
        sut.FillCurrentPosition(3).Should().BeTrue();
        sut.IsLandingFree(3).Should().BeFalse();

        sut.ResetCurrentPosition();

        sut.IsLandingFree(3).Should().BeTrue();
    }
}
