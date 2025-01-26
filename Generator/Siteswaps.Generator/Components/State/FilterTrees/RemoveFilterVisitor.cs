using System.Collections.Immutable;

namespace Siteswaps.Generator.Components.State.FilterTrees;

public record Unit
{
    public static Unit Value { get; } = new Unit();

    private Unit() { }
};

public class RemoveFilterVisitor(FilterNode nodeToRemove) : IFilterVisitor<FilterNode?>
{
    public FilterNode? Visit(AndNode node)
    {
        if (node == nodeToRemove)
        {
            return null;
        }
        var newChildren = node
            .Children.Select(x => ((IFilterVisitor<FilterNode?>)this).Visit(x))
            .OfType<FilterNode>()
            .ToImmutableList();
        return new AndNode(newChildren);
    }

    public FilterNode? Visit(OrNode node)
    {
        if (node == nodeToRemove)
        {
            return null;
        }
        var newChildren = node
            .Children.Select(x => ((IFilterVisitor<FilterNode?>)this).Visit(x))
            .OfType<FilterNode>()
            .ToImmutableList();
        return new OrNode(newChildren);
    }

    public FilterNode? Visit(FilterLeaf node)
    {
        return node == nodeToRemove ? null : node;
    }
}
