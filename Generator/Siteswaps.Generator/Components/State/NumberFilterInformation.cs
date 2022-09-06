namespace Siteswaps.Generator.Components.State;

public class NumberFilterInformation : IFilterInformation
{
    public int? Height { get; set; }
    public NumberFilterType Type { get; set; }
    public int? Amount { get; set; }

    public bool IsCompleted => Height is not null && Amount is not null;
    public FilterType FilterType => FilterType.Number;

    public string Display() => $"{Type} {Amount} throws of height {Height}";
}