namespace Siteswaps.Generator.Components.State.FilterTrees;

public abstract record FilterNode
{
    public abstract T Visit<T>(IFilterVisitor<T> visitor);
}