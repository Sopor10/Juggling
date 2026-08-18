using FluentAssertions;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter.Combinatorics;

namespace Siteswaps.Generator.Test;

public class FilterCombinatoricsTests
{
    [Test]
    public void AndFilter_Rejects_When_A_Filter_Rejects()
    {
        var value = new PartialSiteswap(new[] { 1 });
        var sut = new AndFilter(
            new RecordingFilter(true),
            new RecordingFilter(false, isRotationAware: true)
        );

        sut.CanFulfill(value).Should().BeFalse();
    }

    [Test]
    public void AndFilter_Propagates_Rotation_Awareness()
    {
        var sut = new AndFilter(
            new RecordingFilter(false, isRotationAware: true),
            new RecordingFilter(false)
        );

        sut.IsRotationAware.Should().BeTrue();
    }

    [Test]
    public void AndFilter_Accepts_An_Empty_Filter_Set()
    {
        var value = new PartialSiteswap(new[] { 1 });

        new AndFilter().CanFulfill(value).Should().BeTrue();
    }

    [Test]
    public void OrFilter_Accepts_One_Matching_Filter()
    {
        var value = new PartialSiteswap(new[] { 1 });

        new OrFilter(new RecordingFilter(false), new RecordingFilter(true))
            .CanFulfill(value)
            .Should()
            .BeTrue();
    }

    [Test]
    public void OrFilter_Rejects_When_No_Filter_Matches()
    {
        var value = new PartialSiteswap(new[] { 1 });

        new OrFilter(new RecordingFilter(false), new RecordingFilter(false))
            .CanFulfill(value)
            .Should()
            .BeFalse();
    }

    [Test]
    public void OrFilter_Propagates_Rotation_Awareness()
    {
        var sut = new OrFilter(
            new RecordingFilter(false, isRotationAware: true),
            new RecordingFilter(false)
        );

        sut.IsRotationAware.Should().BeTrue();
    }

    [Test]
    public void NotFilter_Accepts_Partial_Values()
    {
        var sut = new NotFilter(new RecordingFilter(false));

        sut.CanFulfill(new PartialSiteswap(new[] { -1 })).Should().BeTrue();
    }

    [Test]
    public void NotFilter_Inverts_A_Full_Value()
    {
        var sut = new NotFilter(new RecordingFilter(true));

        sut.CanFulfill(new PartialSiteswap(new[] { 1 })).Should().BeFalse();
    }

    [Test]
    public void NotFilter_Propagates_Rotation_Awareness()
    {
        var sut = new NotFilter(new RecordingFilter(false, isRotationAware: true));

        sut.IsRotationAware.Should().BeTrue();
    }
}
