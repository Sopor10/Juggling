namespace Siteswaps.Generator.Components.State;

public record NewPatternFilterInformation(List<Throw> Pattern, bool IsGlobalPattern) : IFilterInformation
{
    public FilterType FilterType => FilterType.NewPattern;
    public int MissingLength(int period) => period - Pattern.Count;

    public string Display() => "include " + (IsGlobalPattern ? "global " : "local ") + string.Join(" ", Pattern.Select(Display).ToList());

    private static string Display(Throw i) => i.Name;
}