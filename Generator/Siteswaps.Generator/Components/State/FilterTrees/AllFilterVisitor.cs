using System.Collections.Immutable;

namespace Siteswaps.Generator.Components.State.FilterTrees;

public class AllFilterVisitor : IFilterVisitor<ImmutableList<FilterNode>>
{
    public ImmutableList<FilterNode> Visit(AndNode node) =>
        node.Children.SelectMany(child => child.Visit(this)).Prepend(node).ToImmutableList();

    public ImmutableList<FilterNode> Visit(OrNode node) =>
        node.Children.SelectMany(child => child.Visit(this)).Prepend(node).ToImmutableList();

    public ImmutableList<FilterNode> Visit(FilterLeaf node) => [node];
}
