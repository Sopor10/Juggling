namespace Siteswaps.Generator.Components.WizardPage.Controls;

/// <summary>
/// Inclusive discrete dual-range geometry. Native <c>input type="range"</c> maps
/// <c>max</c> to a point, so a fill from min→max looks like <c>[min, max)</c>.
/// Slot math gives each integer (including max) equal width on the track.
/// </summary>
internal static class DualRangeInclusive
{
    public static (double LeftPercent, double WidthPercent) Fill(
        int min,
        int max,
        int lowerBound,
        int upperBound
    )
    {
        var slots = upperBound - lowerBound + 1;
        if (slots <= 0)
        {
            return (0, 100);
        }

        var low = Math.Clamp(Math.Min(min, max), lowerBound, upperBound);
        var high = Math.Clamp(Math.Max(min, max), lowerBound, upperBound);
        var left = (low - lowerBound) * 100.0 / slots;
        var width = (high - low + 1) * 100.0 / slots;
        return (left, width);
    }
}
