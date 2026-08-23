using FluentAssertions;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

namespace Siteswaps.Generator.Test;

public class PersonalizedNumberFilterTests
{
    [Test]
    public void AtLeast_Accepts_When_Enough_Possible_Matches_Remain()
    {
        var sut = new PersonalizedNumberFilter(
            2,
            0,
            10,
            [6],
            2,
            PersonalizedNumberFilter.Type.AtLeast,
            0
        );
        var value = new PartialSiteswap([6, 0, -1, -1]);

        sut.CanFulfill(value).Should().BeTrue();
    }

    [Test]
    public void AtMost_Rejects_When_The_Limit_Is_Exceeded()
    {
        var sut = new PersonalizedNumberFilter(
            2,
            0,
            10,
            [6],
            1,
            PersonalizedNumberFilter.Type.AtMost,
            0
        );
        var value = new PartialSiteswap([6, 0, 6, -1]);

        sut.CanFulfill(value).Should().BeFalse();
    }

    [Test]
    public void Exact_Rejects_When_The_Exact_Count_Is_Exceeded()
    {
        var sut = new PersonalizedNumberFilter(
            2,
            0,
            10,
            [6],
            1,
            PersonalizedNumberFilter.Type.Exact,
            0
        );
        var value = new PartialSiteswap([6, 0, 6, -1]);

        sut.CanFulfill(value).Should().BeFalse();
    }
}
