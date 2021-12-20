namespace Siteswaps.Generator.Components.State;

public interface IFilterInformation
{
    public bool IsCompleted { get; }
    public FilterType FilterType { get; }

    string Display();
}