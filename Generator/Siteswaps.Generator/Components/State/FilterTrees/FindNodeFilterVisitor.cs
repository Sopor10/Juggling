namespace Siteswaps.Generator.Components.State.FilterTrees;

public class FindNodeFilterVisitor(FilterTree tree, string key) : FilterNodeVisitor
{
    protected override bool FoundNode(FilterNode node)
    {
        return tree.GetKey(node) == key;
    }
}
