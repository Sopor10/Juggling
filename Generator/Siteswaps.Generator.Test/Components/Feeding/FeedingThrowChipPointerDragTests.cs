using FluentAssertions;
using Siteswaps.Generator.Components.Feeding;

namespace Siteswaps.Generator.Test.Components.Feeding;

public class FeedingThrowChipPointerDragTests
{
    [Test]
    public void IsTouchLikePointer_Recognizes_Touch_And_Pen()
    {
        FeedingThrowChipPointerDrag.IsTouchLikePointer("touch").Should().BeTrue();
        FeedingThrowChipPointerDrag.IsTouchLikePointer("pen").Should().BeTrue();
        FeedingThrowChipPointerDrag.IsTouchLikePointer("mouse").Should().BeFalse();
        FeedingThrowChipPointerDrag.IsTouchLikePointer(null).Should().BeFalse();
    }

    [Test]
    public void ExceedsStartThreshold_Uses_10px_Movement_On_Either_Axis()
    {
        FeedingThrowChipPointerDrag.ExceedsStartThreshold(0, 0, 9, 0).Should().BeFalse();
        FeedingThrowChipPointerDrag.ExceedsStartThreshold(0, 0, 10, 0).Should().BeTrue();
        FeedingThrowChipPointerDrag.ExceedsStartThreshold(0, 0, 0, 10).Should().BeTrue();
        FeedingThrowChipPointerDrag.ExceedsStartThreshold(100, 100, 89, 100).Should().BeTrue();
    }
}
