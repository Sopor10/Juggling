namespace Siteswaps.Generator.Components.State;

public interface IFilterInformation
{
    public FilterType FilterType { get; }

    string Display();
}