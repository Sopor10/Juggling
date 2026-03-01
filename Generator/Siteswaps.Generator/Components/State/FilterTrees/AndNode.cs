using System.Collections.Immutable;

namespace Siteswaps.Generator.Components.State.FilterTrees;

public record AndNode(params ImmutableList<FilterNode> Children) : FilterNode
{
    public override T Visit<T>(IFilterVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
