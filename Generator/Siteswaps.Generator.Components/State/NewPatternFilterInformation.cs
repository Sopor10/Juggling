namespace Siteswaps.Generator.Components.State;

public record NewPatternFilterInformation(List<Throw> Pattern, int Period, bool IsGlobalPattern) : IFilterInformation
{
    public bool IsCompleted => false;
    public FilterType FilterType => FilterType.NewPattern;
    public int MissingLength => Period - Pattern.Count;

    public string Display() => "include " + (IsGlobalPattern ? "global " : "local ") + string.Join(" ", Pattern.Select(Display).ToList());

    private static string Display(Throw i) => i.Name;
}