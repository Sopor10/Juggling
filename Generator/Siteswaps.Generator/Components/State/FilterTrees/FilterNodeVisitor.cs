namespace Siteswaps.Generator.Components.State.FilterTrees;

public abstract class FilterNodeVisitor : IFilterVisitor<FilterNode?>
{
    public FilterNode? Visit(AndNode node)
    {
        if (FoundNode(node))
        {
            return node;
        }
        return node.Children.Select(child => child.Visit(this)).FirstOrDefault(x => x != null);
    }

    public FilterNode? Visit(OrNode node)
    {
        if (FoundNode(node))
        {
            return node;
        }
        return node.Children.Select(child => child.Visit(this)).FirstOrDefault(x => x != null);
    }

    public FilterNode? Visit(FilterLeaf node)
    {
        return FoundNode(node) ? node : null;
    }

    protected abstract bool FoundNode(FilterNode node);
}
