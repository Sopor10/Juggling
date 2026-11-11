using System.Collections.Immutable;
using System.Text.Json;
using Siteswaps.Generator.Components.Internal.EasyFilter;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.State.FilterTrees;
using Siteswaps.Generator.Components.WizardPage;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;

namespace Siteswaps.Generator.Components.GenerationWorkflow;

/// <summary>Compact host state for generation inputs that can be shared in a URL.</summary>
internal static class GenerationWorkflowUrlState
{
    private const int MaxPayloadLength = 48_000;
    private static readonly JsonSerializerOptions Options = new() { MaxDepth = 32 };

    private sealed record Snapshot(
        int MinClubs,
        int MaxClubs,
        int[] Throws,
        Node? Filters,
        bool ShowThrowNames
    );

    private sealed record Node(
        string Kind,
        int Id = 0,
        Node[]? Children = null,
        Filter? Value = null
    );

    private sealed record Filter(
        string Kind,
        int[]? Heights = null,
        int[]? Landing = null,
        int[]? ThrowMask = null,
        int Rotation = 0,
        bool Include = false,
        bool LocallyValid = false,
        bool AllowRotation = false,
        int Amount = 0,
        int NumberType = 0,
        int? Juggler = null,
        int[]? State = null
    );

    public static string? Encode(GenerationWorkflowSession session)
    {
        try
        {
            var state = session.State;
            var snapshot = new Snapshot(
                state.Clubs.MinNumber,
                state.Clubs.MaxNumber,
                state.AllowedThrows.Select(x => x.Height).ToArray(),
                ToNode(session.EditableFilterTree.Root),
                state.ShowThrowNames
            );
            var json = JsonSerializer.Serialize(snapshot, Options);
            return json.Length <= MaxPayloadLength ? json : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static bool Apply(string? json, GenerationWorkflowSession session)
    {
        if (string.IsNullOrWhiteSpace(json) || json.Length > MaxPayloadLength)
        {
            return false;
        }

        try
        {
            var snapshot = JsonSerializer.Deserialize<Snapshot>(json, Options);
            if (!Valid(snapshot))
                return false;

            var min = Math.Clamp(snapshot!.MinClubs, WizardState.MinClubs, WizardState.MaxClubs);
            var max = Math.Clamp(snapshot.MaxClubs, WizardState.MinClubs, WizardState.MaxClubs);
            session.State.Clubs =
                min <= max
                    ? new Between { MinNumber = min, MaxNumber = max }
                    : new Between { MinNumber = max, MaxNumber = max };
            session.State.AllowedThrows.Clear();
            session.State.AllowedThrows.AddRange(
                snapshot.Throws.Distinct().Select(ThrowByHeight).OfType<Throw>()
            );
            session.State.ShowThrowNames = snapshot.ShowThrowNames;

            var maxId = 0;
            var tree = new FilterTree(FromNode(snapshot.Filters, ref maxId));
            session.ReplaceEditableFilterTree(tree);
            while (session.State.NextFilterId() <= maxId) { }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static Node? ToNode(FilterNode? node) =>
        node switch
        {
            null => null,
            AndNode and => new Node(
                "and",
                Children: and.Children.Select(ToNode).OfType<Node>().ToArray()
            ),
            OrNode or => new Node(
                "or",
                Children: or.Children.Select(ToNode).OfType<Node>().ToArray()
            ),
            FilterLeaf leaf when leaf.Filter is WizardIdentifiedFilter identified => new Node(
                "leaf",
                identified.Id,
                Value: ToFilter(identified.Inner)
            ),
            FilterLeaf leaf => new Node("leaf", Value: ToFilter(leaf.Filter)),
            _ => null,
        };

    private static Filter? ToFilter(IFilterInformation info) =>
        info switch
        {
            NewPatternFilterInformation pattern => new Filter(
                "pattern",
                Heights: pattern.Pattern.Select(x => x.Height).ToArray(),
                Rotation: pattern.Rotation.Value,
                Include: pattern.IsIncludePattern,
                LocallyValid: pattern.IsValidLocally
            ),
            InterfaceFilterInformation iface => new Filter(
                "interface",
                Landing: iface.Landing.Select(x => x.Height).ToArray(),
                ThrowMask: iface.Throws?.Select(x => x.Height).ToArray(),
                AllowRotation: iface.AllowRotation
            ),
            EasyNumberFilter.NumberFilter number => new Filter(
                "number",
                Heights: [number.Throw.Height],
                Amount: number.Amount,
                NumberType: (int)number.Type,
                Juggler: number.JugglerIndex
            ),
            EasyStateFilter.StateFilter state => new Filter(
                "state",
                State: state.Items.Select(x => (int)x).ToArray()
            ),
            _ => null,
        };

    private static FilterNode? FromNode(Node? node, ref int maxId)
    {
        if (node is null)
            return null;
        return node.Kind switch
        {
            "and" when node.Children is { Length: > 0 and <= 100 } => new AndNode(
                ToChildren(node.Children, ref maxId)
            ),
            "or" when node.Children is { Length: > 0 and <= 100 } => new OrNode(
                ToChildren(node.Children, ref maxId)
            ),
            "leaf" when node.Value is not null && FromFilter(node.Value) is { } info => MakeLeaf(
                node.Id,
                info,
                ref maxId
            ),
            _ => null,
        };
    }

    private static FilterNode MakeLeaf(int id, IFilterInformation info, ref int maxId)
    {
        if (id is > 0 and <= 100_000)
        {
            maxId = Math.Max(maxId, id);
            return new FilterLeaf(new WizardIdentifiedFilter(id, info));
        }
        return new FilterLeaf(info);
    }

    private static ImmutableList<FilterNode> ToChildren(Node[] nodes, ref int maxId)
    {
        var result = ImmutableList.CreateBuilder<FilterNode>();
        foreach (var node in nodes)
        {
            if (FromNode(node, ref maxId) is { } child)
                result.Add(child);
        }
        return result.ToImmutable();
    }

    private static IFilterInformation? FromFilter(Filter value) =>
        value.Kind switch
        {
            "pattern" when ValidHeights(value.Heights) && value.Rotation is >= -3 and <= 7 =>
                new NewPatternFilterInformation(
                    value.Heights!.Select(ThrowByHeight).OfType<Throw>().ToList(),
                    new PatternRotation(value.Rotation),
                    value.Include,
                    value.LocallyValid
                ),
            "interface"
                when ValidHeights(value.Landing)
                    && (value.ThrowMask is null || ValidHeights(value.ThrowMask)) =>
                new InterfaceFilterInformation(
                    value.Landing!.Select(ThrowByHeight).OfType<Throw>().ToList(),
                    value.AllowRotation,
                    value.ThrowMask?.Select(ThrowByHeight).OfType<Throw>().ToList()
                ),
            "number"
                when ValidHeights(value.Heights)
                    && value.Amount is >= 0 and <= 1000
                    && Enum.IsDefined((EasyNumberFilter.NumberFilterType)value.NumberType) =>
                new EasyNumberFilter.NumberFilter
                {
                    Amount = value.Amount,
                    Type = (EasyNumberFilter.NumberFilterType)value.NumberType,
                    Throw = ThrowByHeight(value.Heights![0])!,
                    JugglerIndex = value.Juggler,
                },
            "state"
                when value.State is { Length: > 0 and <= 100 }
                    && value.State.All(x => Enum.IsDefined((StateValue)x)) =>
                new EasyStateFilter.StateFilter([.. value.State.Select(x => (StateValue)x)]),
            _ => null,
        };

    private static bool Valid(Snapshot? value) =>
        value is not null
        && value.MinClubs is >= WizardState.MinClubs and <= WizardState.MaxClubs
        && value.MaxClubs is >= WizardState.MinClubs and <= WizardState.MaxClubs
        && value.Throws is { Length: <= 51 }
        && value.Throws.All(x => ThrowByHeight(x) is not null)
        && ValidNode(value.Filters);

    private static bool ValidNode(Node? node) =>
        node switch
        {
            null => true,
            { Kind: "and" or "or", Children: { Length: > 0 and <= 100 } children } => children.All(
                ValidNode
            ),
            { Kind: "leaf", Value: not null } leaf => FromFilter(leaf.Value) is not null,
            _ => false,
        };

    private static bool ValidHeights(int[]? values) =>
        values is { Length: > 0 and <= 100 } && values.All(x => ThrowByHeight(x) is not null);

    private static Throw? ThrowByHeight(int height) =>
        Throw
            .Everything(WizardState.AbsoluteMaxThrowHeight)
            .FirstOrDefault(x => x.Height == height);
}
