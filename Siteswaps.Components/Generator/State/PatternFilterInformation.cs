namespace Siteswaps.Components.Generator.State;

public class PatternFilterInformation : IFilterInformation
{
    public bool IsCompleted => false;
    public FilterType FilterType => FilterType.Pattern;

    public string Display() => "Pattern";
}