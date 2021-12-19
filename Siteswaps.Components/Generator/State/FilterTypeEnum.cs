namespace Siteswaps.Components.Generator.State;

public enum FilterType
{
    Number,
    Pattern
}

public interface IFilterInformation
{
    public bool IsCompleted { get; }
    public FilterType FilterType { get; }

    string Display();
}

public class PatternFilterInformation : IFilterInformation
{
    public bool IsCompleted => false;
    public FilterType FilterType => FilterType.Pattern;

    public string Display() => "Pattern";
}

public class NumberFilterInformation : IFilterInformation
{
    public int? Height { get; set; }
    public NumberFilterType Type { get; set; }
    public int? Amount { get; set; }

    public bool IsCompleted => Height is not null && Amount is not null;
    public FilterType FilterType => FilterType.Number;

    public string Display() => $"{Type} {Amount} throws of height {Height}";
}

public enum NumberFilterType
{
    Exactly,
    AtLeast,
    Maximum
}