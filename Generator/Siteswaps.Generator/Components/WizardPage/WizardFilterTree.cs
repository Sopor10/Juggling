using System.Collections.Immutable;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.State.FilterTrees;

namespace Siteswaps.Generator.Components.WizardPage;

/// <summary>
/// Leaf payload that keeps a stable edit/delete id while the immutable
/// <see cref="FilterTree"/> nodes are replaced on every mutation.
/// </summary>
public sealed record WizardIdentifiedFilter(int Id, IFilterInformation Inner) : IFilterInformation
{
    public string Display() => Inner.Display();
}

/// <summary>
/// Immutable-tree helpers for the wizard filter UI (nested And/Or boxes).
/// </summary>
internal static class WizardFilterTree
{
    public static bool IsEmpty(FilterTree tree) => tree.Root is null;

    public static IEnumerable<FilterLeaf> Leaves(FilterTree tree) => tree.All.OfType<FilterLeaf>();

    public static FilterLeaf? FindLeaf(FilterTree tree, int id) =>
        Leaves(tree)
            .FirstOrDefault(leaf =>
                leaf.Filter is WizardIdentifiedFilter identified && identified.Id == id
            );

    public static IFilterInformation Unwrap(IFilterInformation filter) =>
        filter is WizardIdentifiedFilter identified ? identified.Inner : filter;

    public static FilterTree AddLeaf(FilterTree tree, FilterNode? parentGroup, FilterLeaf leaf)
    {
        if (tree.Root is null)
        {
            return new FilterTree(leaf);
        }

        if (parentGroup is null)
        {
            return AppendToRoot(tree, leaf);
        }

        if (parentGroup is AndNode or OrNode)
        {
            return Normalize(tree.Add(parentGroup, leaf));
        }

        return AppendToRoot(tree, leaf);
    }

    public static FilterTree ReplaceLeaf(FilterTree tree, int id, IFilterInformation inner)
    {
        var leaf = FindLeaf(tree, id);
        if (leaf is null || leaf.Filter is not WizardIdentifiedFilter identified)
        {
            return tree;
        }

        return Normalize(
            tree.ReplaceLeafContent(leaf, new WizardIdentifiedFilter(identified.Id, inner))
        );
    }

    public static FilterTree RemoveLeaf(FilterTree tree, int id)
    {
        var leaf = FindLeaf(tree, id);
        return leaf is null ? tree : Normalize(tree.Remove(leaf));
    }

    public static FilterTree ToggleGroupOperator(FilterTree tree, FilterNode group)
    {
        if (group is not (AndNode or OrNode))
        {
            return tree;
        }

        return Normalize(ReplaceNode(tree, group, SwapOperator(group)));
    }

    public static FilterTree WrapAdjacentChildren(
        FilterTree tree,
        FilterNode parentGroup,
        int childIndex
    )
    {
        if (parentGroup is not AndNode and not OrNode)
        {
            return tree;
        }

        var children = ChildrenOf(parentGroup);
        if (childIndex < 0 || childIndex + 1 >= children.Count)
        {
            return tree;
        }

        var nested = CreateGroup(
            InvertOperator(parentGroup),
            ImmutableList.Create(children[childIndex], children[childIndex + 1])
        );
        var next = children
            .Take(childIndex)
            .Append(nested)
            .Concat(children.Skip(childIndex + 2))
            .ToImmutableList();
        return Normalize(ReplaceNode(tree, parentGroup, CreateGroup(parentGroup, next)));
    }

    public static FilterTree Normalize(FilterTree tree) => new(NormalizeNode(tree.Root));

    private static FilterTree AppendToRoot(FilterTree tree, FilterLeaf leaf)
    {
        return tree.Root switch
        {
            null => new FilterTree(leaf),
            FilterLeaf existing => new FilterTree(
                new AndNode(ImmutableList.Create<FilterNode>(existing, leaf))
            ),
            AndNode andNode => Normalize(tree.Add(andNode, leaf)),
            OrNode orNode => Normalize(tree.Add(orNode, leaf)),
            _ => tree,
        };
    }

    private static FilterNode SwapOperator(FilterNode group) =>
        group switch
        {
            AndNode andNode => new OrNode(andNode.Children),
            OrNode orNode => new AndNode(orNode.Children),
            _ => group,
        };

    private static bool InvertOperator(FilterNode group) => group is AndNode;

    private static FilterNode CreateGroup(FilterNode like, ImmutableList<FilterNode> children) =>
        like is OrNode ? new OrNode(children) : new AndNode(children);

    private static FilterNode CreateGroup(bool asOr, ImmutableList<FilterNode> children) =>
        asOr ? new OrNode(children) : new AndNode(children);

    private static ImmutableList<FilterNode> ChildrenOf(FilterNode group) =>
        group switch
        {
            AndNode andNode => andNode.Children,
            OrNode orNode => orNode.Children,
            _ => ImmutableList<FilterNode>.Empty,
        };

    private static FilterNode? NormalizeNode(FilterNode? node) =>
        node switch
        {
            null => null,
            FilterLeaf leaf => leaf,
            AndNode andNode => NormalizeGroup(andNode.Children, asOr: false),
            OrNode orNode => NormalizeGroup(orNode.Children, asOr: true),
            _ => node,
        };

    private static FilterNode? NormalizeGroup(ImmutableList<FilterNode> children, bool asOr)
    {
        var normalized = children.Select(NormalizeNode).OfType<FilterNode>().ToImmutableList();
        return normalized.Count switch
        {
            0 => null,
            1 => normalized[0],
            _ => asOr ? new OrNode(normalized) : new AndNode(normalized),
        };
    }

    private static FilterTree ReplaceNode(
        FilterTree tree,
        FilterNode target,
        FilterNode replacement
    )
    {
        if (tree.Root is null)
        {
            return tree;
        }

        if (ReferenceEquals(tree.Root, target))
        {
            return new FilterTree(replacement);
        }

        return new FilterTree(ReplaceIn(tree.Root, target, replacement));
    }

    private static FilterNode ReplaceIn(FilterNode node, FilterNode target, FilterNode replacement)
    {
        if (ReferenceEquals(node, target))
        {
            return replacement;
        }

        return node switch
        {
            AndNode andNode => new AndNode(
                andNode
                    .Children.Select(child => ReplaceIn(child, target, replacement))
                    .ToImmutableList()
            ),
            OrNode orNode => new OrNode(
                orNode
                    .Children.Select(child => ReplaceIn(child, target, replacement))
                    .ToImmutableList()
            ),
            _ => node,
        };
    }
}
