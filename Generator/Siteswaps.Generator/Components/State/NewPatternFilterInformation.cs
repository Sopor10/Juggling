using System.Diagnostics;

namespace Siteswaps.Generator.Components.State;

[DebuggerDisplay("{Display}")]
public record NewPatternFilterInformation(
    List<Throw> Pattern,
    PatternRotation PatternRotation,
    bool IsIncludePattern,
    bool IsValidLocally
) : IFilterInformation
{
    public List<Throw> Pattern { get; set; } = Pattern;
    public PatternRotation Rotation { get; set; } = PatternRotation;
    public bool IsIncludePattern { get; set; } = IsIncludePattern;
    public bool IsValidLocally { get; set; } = IsValidLocally;

    public string Display() =>
        (IsIncludePattern ? "include " : "exclude ")
        + (IsValidLocally ? "valid " : "")
        + PatternRotation.Display
        + " "
        + string.Join(",", FilledPattern.Select(Display).ToList());

    public IEnumerable<Throw> FilledPattern =>
        Enumerable.Reverse(Pattern).SkipWhile(x => x == Throw.Empty).Reverse();

    private static string Display(Throw i) => i.DisplayValue;
}
