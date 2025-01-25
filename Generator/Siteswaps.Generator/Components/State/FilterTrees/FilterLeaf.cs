namespace Siteswaps.Generator.Components.State.FilterTrees;

public record FilterLeaf(IFilterInformation Filter) : FilterNode
{
    public override T Visit<T>(IFilterVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}