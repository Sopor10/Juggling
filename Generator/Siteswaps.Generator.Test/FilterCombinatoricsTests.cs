using FluentAssertions;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;
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

    [Test]
    public void AndFilter_Queries_A_RotationAware_Filter_That_Can_Reject_Partial()
    {
        var sut = new AndFilter(new RecordingFilter(false, isRotationAware: true));

        sut.CanFulfillAnyRotation(new PartialSiteswap(new[] { -1 })).Should().BeFalse();
    }

    [Test]
    public void AndFilter_Skips_A_RotationAware_Filter_That_Cannot_Reject_Partial()
    {
        var sut = new AndFilter(new PartialSafeRotationFilter());

        sut.CanFulfillAnyRotation(new PartialSiteswap(new[] { -1 })).Should().BeTrue();
    }

    [Test]
    public void OrFilter_Cannot_Reject_Partial_When_One_Child_Cannot()
    {
        var sut = new OrFilter(new PartialSafeRotationFilter(), new RecordingFilter(false));

        sut.CanRejectPartial.Should().BeFalse();
    }

    private sealed class PartialSafeRotationFilter : ISiteswapFilter
    {
        public bool CanFulfill(PartialSiteswap value)
        {
            if (!value.IsFilled())
                throw new InvalidOperationException("Partial values must be skipped.");

            return true;
        }

        public bool IsRotationAware => true;
        public bool CanRejectPartial => false;
    }
}
