namespace Siteswaps.Generator.Components.State.FilterTrees;

public class GetKeyFilterVisitor(FilterNode searchTarget) : IFilterVisitor<string?>
{
    public string? Visit(AndNode node)
    {
        if (node == searchTarget)
        {
            return "AND";
        }
        return node
            .Children.Select(child => child.Visit(this))
            .OfType<string>()
            .Select(result => "AND_" + result)
            .FirstOrDefault();
    }

    public string? Visit(OrNode node)
    {
        if (node == searchTarget)
        {
            return "OR";
        }
        return node
            .Children.Select(child => child.Visit(this))
            .OfType<string>()
            .Select(result => "OR_" + result)
            .FirstOrDefault();
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
