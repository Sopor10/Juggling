namespace Siteswaps.Generator.Components.State;

using Generator;

public record InterfaceFilterInformation(IEnumerable<Throw> Pattern, string From = "") : IFilterInformation
{
    public FilterType FilterType => FilterType.Interface;

    public string Display() => "interface " + this.From + " " + string.Join(" ", Pattern.Reverse().SkipWhile(x => x == Throw.Empty).Reverse().Select(Display).ToList());

    private static string Display(Throw i) => i.DisplayValue;
    
}
