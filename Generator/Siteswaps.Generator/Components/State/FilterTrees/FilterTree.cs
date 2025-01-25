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
    
    
    public FilterTree Add(FilterNode parent, FilterNode node)
    {
        var visitor = new AddFilterVisitor(parent, node);
        return new FilterTree(Root?.Visit(visitor));
    }

    public string? GetKey(FilterNode node)
    {
        var visitor = new GetKeyFilterVisitor(node);
        return Root?.Visit(visitor);
    }
}

public class GetKeyFilterVisitor(FilterNode searchTarget) : IFilterVisitor<string?>
{
    public string? Visit(AndNode node)
    {
        if (node == searchTarget)
        {
            return "AND";
        }
        return node.Children
            .Select(child => ((IFilterVisitor<string?>)this).Visit(child))
            .OfType<string>()
            .Select(result => "AND_" + result).FirstOrDefault();
    }

    public string? Visit(OrNode node)
    {
        if (node == searchTarget)
        {
            return "OR";
        }
        return node.Children
            .Select(child => ((IFilterVisitor<string?>)this).Visit(child))
            .OfType<string>()
            .Select(result => "OR_" + result).FirstOrDefault();
    }

    public string? Visit(FilterLeaf node)
    {
        if (node == searchTarget)
        {
            return node.Filter.Display();
        }

        return null;
    }
}

public class AddFilterVisitor(FilterNode parent, FilterNode newNote) : IFilterVisitor<FilterNode>
{

    public FilterNode Visit(AndNode node)
    {
        if (node == parent)
        {
            return new AndNode(node.Children.Add(newNote));
        }

        return new AndNode(node.Children.Select(x => ((IFilterVisitor<FilterNode>)this).Visit(x)).ToImmutableList());
    }

    public FilterNode Visit(OrNode node)
    {
        if (node == parent)
        {
            return new OrNode(node.Children.Add(newNote));
        }

        return new OrNode(node.Children.Select(x => ((IFilterVisitor<FilterNode>)this).Visit(x)).ToImmutableList());
    }

    public FilterNode Visit(FilterLeaf node)
    {
        throw new ArgumentException("Cannot add to leaf node");
    }

}