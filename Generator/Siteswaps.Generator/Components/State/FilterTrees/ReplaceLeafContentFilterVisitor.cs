using System.Collections.Immutable;

namespace Siteswaps.Generator.Components.State.FilterTrees;

public class ReplaceLeafContentFilterVisitor(
    FilterLeaf actionFilterNumber,
    IFilterInformation actionNewPatternFilterInformation
) : IFilterVisitor<FilterNode?>
{
    public FilterNode? Visit(AndNode node)
    {
        return new AndNode(
            node.Children.Select(child => child.Visit(this))
                .Where(child => child != null)
                .Cast<FilterNode>()
                .ToImmutableList()
        );
    }

    public FilterNode? Visit(OrNode node)
    {
        return new OrNode(
            node.Children.Select(child => child.Visit(this))
                .Where(child => child != null)
                .Cast<FilterNode>()
                .ToImmutableList()
        );
    }

    public FilterNode? Visit(FilterLeaf node)
    {
        return node == actionFilterNumber
            ? new FilterLeaf(actionNewPatternFilterInformation)
            : node;
    }
}
