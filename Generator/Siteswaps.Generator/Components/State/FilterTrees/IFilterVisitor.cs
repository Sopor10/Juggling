namespace Siteswaps.Generator.Components.State.FilterTrees;

public interface IFilterVisitor<T>
{
    public T Visit(FilterNode node) =>
        node switch
        {
            AndNode andNode => Visit(andNode),
            OrNode orNode => Visit(orNode),
            FilterLeaf filterLeaf => Visit(filterLeaf),
            _ => throw new ArgumentOutOfRangeException(nameof(node), node, null),
        };

    public T Visit(AndNode node);
    public T Visit(OrNode node);
    public T Visit(FilterLeaf node);
}
