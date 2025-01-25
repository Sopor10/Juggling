namespace Siteswaps.Generator.Components.State;

public record NewPatternFilterInformation(
    List<Throw> Pattern,
    bool IsGlobalPattern,
    bool IsIncludePattern
) : IFilterInformation
{
    
    public List<Throw> Pattern { get; set; } = Pattern;
    public bool IsGlobalPattern { get; set; } = IsGlobalPattern;
    public bool IsIncludePattern { get; set; } = IsIncludePattern;

    public string Display() =>
        (IsIncludePattern ? "include " : "exclude ")
        + (IsGlobalPattern ? "global " : "local ")
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
