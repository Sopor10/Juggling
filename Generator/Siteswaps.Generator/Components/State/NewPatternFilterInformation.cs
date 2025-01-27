using System.Diagnostics;

namespace Siteswaps.Generator.Components.State;

[DebuggerDisplay("{Display}")]
public record NewPatternFilterInformation(
    List<Throw> Pattern,
    PatternRotation PatternRotation,
    bool IsIncludePattern
) : IFilterInformation
{
    public List<Throw> Pattern { get; set; } = Pattern;
    public PatternRotation Rotation { get; set; } = PatternRotation;
    public bool IsIncludePattern { get; set; } = IsIncludePattern;

    public string Display() =>
        (IsIncludePattern ? "include " : "exclude ")
        + PatternRotation.Display + " "
        + string.Join(
            ",",
            Enumerable
                .Reverse(Pattern)
                .SkipWhile(x => x == Throw.Empty)
                .Reverse()
                .Select(Display)
                .ToList()
        );

    private static string Display(Throw i) => i.DisplayValue;
}

[DebuggerDisplay("{Display}")]
public record PatternRotation(int Value)
{
    public static PatternRotation Global => new PatternRotation(-2);
    public static PatternRotation Local => new PatternRotation(-1);
    public static PatternRotation A => new PatternRotation(0);
    public static PatternRotation B => new PatternRotation(1);
    
    public string Display => Value switch
    {
        -2 => "global",
        -1 => "local",
        _ => ((char)('A' + Value)).ToString()
    };
    
}
