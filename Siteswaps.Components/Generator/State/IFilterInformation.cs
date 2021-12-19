namespace Siteswaps.Components.Generator.State;

public interface IFilterInformation
{
    public bool IsCompleted { get; }
    public FilterType FilterType { get; }

    string Display();
}