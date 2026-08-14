using Siteswaps.Generator.Components.State;

namespace Siteswaps.Generator.Components.WizardPage;

/// <summary>
/// Pure helpers for the pattern-filter UI (don't-care / "frei" defaults).
/// </summary>
internal static class WizardPatternFilterUi
{
    public const string DontCareLabel = "frei";

    public static IEnumerable<Throw> WithDontCarePalette(IEnumerable<Throw> possibleThrows)
    {
        var list = possibleThrows as IList<Throw> ?? possibleThrows.ToList();
        return list.Any(t => t.Height == Throw.Empty.Height)
            ? list
            : list.Prepend(Throw.Empty);
    }

    public static string Label(Throw t) =>
        t.Height == Throw.Empty.Height ? DontCareLabel : t.GetDisplayValue(true);

    public static List<Throw> DefaultSlots(int length) =>
        Enumerable.Repeat(Throw.Empty, Math.Max(1, length)).ToList();
}
