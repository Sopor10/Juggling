using System.Collections.Immutable;

namespace Siteswaps.Generator.Components.State.FilterTrees;

public class FilterTree(FilterNode? root)
{
    public FilterNode? Root { get; } = root;

    public FilterTree Remove(FilterNode node)
    {
        var visitor = new RemoveFilterVisitor(node);
        var result = Root?.Visit(visitor);
        return new FilterTree(result);
    }

    public FilterTree RemoveAll(Func<FilterNode, bool> predicate)
    {
        return All.Where(predicate).Aggregate(this, (current, node) => current.Remove(node));
    }

    public FilterTree Add(FilterNode parent, FilterNode node)
    {
        var visitor = new AddFilterVisitor(parent, node);
        return new FilterTree(Root?.Visit(visitor));
    }

    public FilterNode? FindNode(string? key)
    {
        if (key is null)
        {
            return null;
        }

        var visitor = new FindNodeFilterVisitor(this, key);
        return Root?.Visit(visitor);
    }

    public string? GetKey(FilterNode node)
    {
        var visitor = new GetKeyFilterVisitor(node);
        return Root?.Visit(visitor);
    }

    public FilterTree ReplaceLeafContent(
        FilterLeaf actionFilterNumber,
        IFilterInformation actionNewPatternFilterInformation
    )
    {
        var visitor = new ReplaceLeafContentFilterVisitor(
            actionFilterNumber,
            actionNewPatternFilterInformation
        );
        return new FilterTree(Root?.Visit(visitor));
    }

    public ImmutableList<FilterNode> All =>
        Root?.Visit(new AllFilterVisitor()) ?? ImmutableList<FilterNode>.Empty;
}
