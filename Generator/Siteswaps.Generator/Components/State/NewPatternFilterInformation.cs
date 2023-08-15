namespace Siteswaps.Generator.Components.State;

using Generator;

public record NewPatternFilterInformation(IEnumerable<Throw> Pattern, bool IsGlobalPattern) : IFilterInformation
{
    public FilterType FilterType => FilterType.NewPattern;

    public string Display() => "include " + (IsGlobalPattern ? "global " : "local ") + string.Join(" ", Pattern.Reverse().SkipWhile(x => x == Throw.Empty).Reverse().Select(Display).ToList());

    private static string Display(Throw i) => i.DisplayValue;
}
