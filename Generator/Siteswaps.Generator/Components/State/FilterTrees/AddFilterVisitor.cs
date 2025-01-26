using System.Collections.Immutable;

namespace Siteswaps.Generator.Components.State.FilterTrees;

public class AddFilterVisitor(FilterNode parent, FilterNode newNote) : IFilterVisitor<FilterNode>
{
    public FilterNode Visit(AndNode node)
    {
        if (node == parent)
        {
            return new AndNode(node.Children.Add(newNote));
        }

        return new AndNode(
            node.Children.Select(x => ((IFilterVisitor<FilterNode>)this).Visit(x)).ToImmutableList()
        );
    }

    public FilterNode Visit(OrNode node)
    {
        if (node == parent)
        {
            return new OrNode(node.Children.Add(newNote));
        }

        return new OrNode(
            node.Children.Select(x => ((IFilterVisitor<FilterNode>)this).Visit(x)).ToImmutableList()
        );
    }

    public FilterNode Visit(FilterLeaf node)
    {
        return node;
    }
}
