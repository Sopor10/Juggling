namespace Siteswaps.Generator.Components.Feeding;

/// <summary>
/// Touch/pen pointer drag gesture helpers for throw chips (HTML5 DnD does not work on most mobile browsers).
/// </summary>
public static class FeedingThrowChipPointerDrag
{
    public const double StartThresholdPx = 10;

    public static bool IsTouchLikePointer(string? pointerType) => pointerType is "touch" or "pen";

    public static bool ExceedsStartThreshold(
        double startX,
        double startY,
        double clientX,
        double clientY
    ) =>
        Math.Abs(clientX - startX) >= StartThresholdPx
        || Math.Abs(clientY - startY) >= StartThresholdPx;
}
