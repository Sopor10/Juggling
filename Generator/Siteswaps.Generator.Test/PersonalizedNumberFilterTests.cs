using FluentAssertions;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

namespace Siteswaps.Generator.Test;

public class PersonalizedNumberFilterTests
{
    private static PersonalizedNumberFilter CreateFilter(
        PersonalizedNumberFilter.Type type,
        int amount
    ) => new(2, 0, 10, [6], amount, type, 0);

    [Test]
    public void AtLeast_Accepts_When_Enough_Possible_Matches_Remain()
    {
        var sut = CreateFilter(PersonalizedNumberFilter.Type.AtLeast, 2);
        var value = new PartialSiteswap([6, 0, -1, -1]);

        sut.CanFulfill(value).Should().BeTrue();
    }

    [Test]
    public void AtMost_Rejects_When_The_Limit_Is_Exceeded()
    {
        var sut = CreateFilter(PersonalizedNumberFilter.Type.AtMost, 1);
        var value = new PartialSiteswap([6, 0, 6, -1]);

        sut.CanFulfill(value).Should().BeFalse();
    }

    [Test]
    public void Exact_Rejects_When_The_Exact_Count_Is_Exceeded()
    {
        var sut = CreateFilter(PersonalizedNumberFilter.Type.Exact, 1);
        var value = new PartialSiteswap([6, 0, 6, -1]);

        sut.CanFulfill(value).Should().BeFalse();
    }
}
