using FluentAssertions;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;

namespace Siteswaps.Generator.Test;

public class NumberOfPassesFilterTests
{
    [Test]
    public void Rejects_A_Partial_Value_When_The_Pass_Limit_Is_Exceeded()
    {
        var sut = new NumberOfPassesFilter(1, 2, new SiteswapGeneratorInput(3, 2, 0, 10));
        var value = new PartialSiteswap([1, 1, -1]);

        sut.CanFulfill(value).Should().BeFalse();
    }

    [Test]
    public void Accepts_A_Partial_Value_When_The_Pass_Limit_Is_Not_Exceeded()
    {
        var sut = new NumberOfPassesFilter(1, 2, new SiteswapGeneratorInput(3, 2, 0, 10));
        var value = new PartialSiteswap([1, 0, -1]);

        sut.CanFulfill(value).Should().BeTrue();
    }
}
