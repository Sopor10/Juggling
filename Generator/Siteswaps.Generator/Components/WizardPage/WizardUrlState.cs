using System.Collections.Immutable;
using System.IO.Compression;
using System.Text.Json;
using Siteswaps.Generator.Components.Internal.EasyFilter;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.State.FilterTrees;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;
using StateFilter = Siteswaps.Generator.Components.Internal.EasyFilter.EasyStateFilter.StateFilter;

namespace Siteswaps.Generator.Components.WizardPage;

/// <summary>Compact, versioned URL representation of the shareable wizard state.</summary>
internal static class WizardUrlState
{
    private const string Prefix = "1.";
    private const int MaxEncodedLength = 24000;
    private const int MaxJsonLength = 100000;
    private static readonly JsonSerializerOptions JsonOptions = new() { MaxDepth = 32 };

    internal sealed record Snapshot(
        int Step,
        bool Results,
        int Jugglers,
        int Period,
        int MinClubs,
        int MaxClubs,
        bool ShowThrowNames,
        int MaxThrowHeight,
        int[] Throws,
        NodeDto? Filter
    );

    internal sealed record NodeDto(
        string Kind,
        NodeDto[]? Children = null,
        FilterDto? Value = null
    );

    internal sealed record FilterDto(
        string Kind,
        int[]? Heights = null,
        int[]? Landing = null,
        int[]? Throws = null,
        int Rotation = 0,
        bool Include = false,
        bool LocallyValid = false,
        bool AllowRotation = false,
        int Amount = 0,
        int NumberType = 0,
        int? Juggler = null,
        int[]? State = null
    );

    public static string? Encode(WizardState state)
    {
        try
        {
            var snapshot = new Snapshot(
                Math.Clamp(state.CurrentStep, 0, WizardState.TotalSteps - 1),
                state.Phase != WizardPhase.Editing,
                state.NumberOfJugglers,
                state.Period.Value,
                state.Clubs.MinNumber,
                state.Clubs.MaxNumber,
                state.ShowThrowNames,
                state.MaxThrowHeight,
                state.AllowedThrows.Select(x => x.Height).ToArray(),
                ToDto(state.FilterTree.Root)
            );
            var json = JsonSerializer.SerializeToUtf8Bytes(snapshot, JsonOptions);
            if (json.Length > MaxJsonLength)
            {
                return null;
            }

            using var output = new MemoryStream();
            using (
                var compressor = new DeflateStream(
                    output,
                    CompressionLevel.Fastest,
                    leaveOpen: true
                )
            )
            {
                compressor.Write(json);
            }

            var encoded =
                Prefix
                + Convert
                    .ToBase64String(output.ToArray())
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');
            return encoded.Length <= MaxEncodedLength ? encoded : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static Snapshot? Decode(string? value)
    {
        if (
            string.IsNullOrWhiteSpace(value)
            || value.Length > MaxEncodedLength
            || !value.StartsWith(Prefix, StringComparison.Ordinal)
        )
        {
            return null;
        }

        try
        {
            var base64 = value[Prefix.Length..].Replace('-', '+').Replace('_', '/');
            base64 = base64.PadRight((base64.Length + 3) / 4 * 4, '=');
            using var compressed = new MemoryStream(Convert.FromBase64String(base64));
            using var decompressor = new DeflateStream(compressed, CompressionMode.Decompress);
            using var output = new MemoryStream();
            var buffer = new byte[4096];
            int read;
            while ((read = decompressor.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (output.Length + read > MaxJsonLength)
                {
                    return null;
                }
                output.Write(buffer, 0, read);
            }

            var snapshot = JsonSerializer.Deserialize<Snapshot>(output.ToArray(), JsonOptions);
            return IsValid(snapshot) ? snapshot : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static void Apply(Snapshot snapshot, WizardState state)
    {
        state.CurrentStep = Math.Clamp(snapshot.Step, 0, WizardState.TotalSteps - 1);
        state.Phase = snapshot.Results ? WizardPhase.Results : WizardPhase.Editing;
        state.NumberOfJugglers = Math.Clamp(
            snapshot.Jugglers,
            WizardState.MinJugglers,
            WizardState.MaxJugglersExact
        );
        state.Period = new Period(
            Math.Clamp(snapshot.Period, WizardState.MinPeriod, WizardState.MaxPeriod)
        );
        state.Clubs = new Between
        {
            MinNumber = Math.Clamp(snapshot.MinClubs, WizardState.MinClubs, WizardState.MaxClubs),
            MaxNumber = Math.Clamp(snapshot.MaxClubs, WizardState.MinClubs, WizardState.MaxClubs),
        };
        if (state.Clubs.MinNumber > state.Clubs.MaxNumber)
        {
            state.Clubs = state.Clubs with { MinNumber = state.Clubs.MaxNumber };
        }
        state.ShowThrowNames = snapshot.ShowThrowNames;
        state.ApplyMaxThrowHeight(snapshot.MaxThrowHeight);
        state.AllowedThrows.Clear();
        state.AllowedThrows.AddRange(
            snapshot.Throws.Distinct().Select(ThrowByHeight).Where(x => x is not null).Cast<Throw>()
        );
        var nextId = 1;
        state.FilterTree = new FilterTree(FromDto(snapshot.Filter, ref nextId));
        for (var id = 1; id < nextId; id++)
            state.NextFilterId();
        state.VisitedSteps.Clear();
        for (var step = 0; step <= state.CurrentStep; step++)
            state.VisitedSteps.Add(step);
        if (state.Phase == WizardPhase.Results)
            state.MarkVisited(WizardState.ResultsStepIndex);
    }

    private static NodeDto? ToDto(FilterNode? node) =>
        node switch
        {
            null => null,
            AndNode and => new NodeDto(
                "and",
                and.Children.Select(ToDto).OfType<NodeDto>().ToArray()
            ),
            OrNode or => new NodeDto("or", or.Children.Select(ToDto).OfType<NodeDto>().ToArray()),
            FilterLeaf leaf => new NodeDto(
                "leaf",
                Value: FilterToDto(WizardFilterTree.Unwrap(leaf.Filter))
            ),
            _ => null,
        };

    private static FilterDto? FilterToDto(IFilterInformation filter) =>
        filter switch
        {
            NewPatternFilterInformation pattern => new FilterDto(
                "pattern",
                Heights: pattern.Pattern.Select(x => x.Height).ToArray(),
                Rotation: pattern.Rotation.Value,
                Include: pattern.IsIncludePattern,
                LocallyValid: pattern.IsValidLocally
            ),
            InterfaceFilterInformation iface => new FilterDto(
                "interface",
                Landing: iface.Landing.Select(x => x.Height).ToArray(),
                Throws: iface.Throws?.Select(x => x.Height).ToArray(),
                AllowRotation: iface.AllowRotation
            ),
            EasyNumberFilter.NumberFilter number => new FilterDto(
                "number",
                Heights: [number.Throw.Height],
                Amount: number.Amount,
                NumberType: (int)number.Type,
                Juggler: number.JugglerIndex
            ),
            StateFilter state => new FilterDto(
                "state",
                State: state.Items.Select(x => (int)x).ToArray()
            ),
            _ => null,
        };

    private static FilterNode? FromDto(NodeDto? node, ref int nextId)
    {
        if (node is null)
            return null;
        return node.Kind switch
        {
            "and" => new AndNode(FromChildren(node.Children, ref nextId)),
            "or" => new OrNode(FromChildren(node.Children, ref nextId)),
            "leaf" when node.Value is not null && FilterFromDto(node.Value) is { } filter =>
                new FilterLeaf(new WizardIdentifiedFilter(nextId++, filter)),
            _ => null,
        };
    }

    private static ImmutableList<FilterNode> FromChildren(NodeDto[]? children, ref int nextId)
    {
        var result = ImmutableList.CreateBuilder<FilterNode>();
        foreach (var child in children ?? [])
        {
            if (FromDto(child, ref nextId) is { } node)
                result.Add(node);
        }
        return result.ToImmutable();
    }

    private static IFilterInformation? FilterFromDto(FilterDto value) =>
        value.Kind switch
        {
            "pattern" when ValidHeights(value.Heights) && value.Rotation is >= -3 and <= 19 =>
                new NewPatternFilterInformation(
                    value.Heights!.Select(ThrowByHeight).OfType<Throw>().ToList(),
                    new PatternRotation(value.Rotation),
                    value.Include,
                    value.LocallyValid
                ),
            "interface"
                when ValidHeights(value.Landing)
                    && (value.Throws is null || ValidHeights(value.Throws)) =>
                new InterfaceFilterInformation(
                    value.Landing!.Select(ThrowByHeight).OfType<Throw>().ToList(),
                    value.AllowRotation,
                    value.Throws?.Select(ThrowByHeight).OfType<Throw>().ToList()
                ),
            "number"
                when ValidHeights(value.Heights)
                    && value.Amount is >= 0 and <= 1000
                    && Enum.IsDefined((EasyNumberFilter.NumberFilterType)value.NumberType)
                    && value.Juggler is null or >= 0 and < WizardState.MaxJugglersExact =>
                new EasyNumberFilter.NumberFilter
                {
                    Amount = value.Amount,
                    Type = (EasyNumberFilter.NumberFilterType)value.NumberType,
                    Throw = ThrowByHeight(value.Heights![0])!,
                    JugglerIndex = value.Juggler,
                },
            "state"
                when value.State is { Length: > 0 and <= 100 }
                    && value.State.All(x => Enum.IsDefined((StateValue)x)) => new StateFilter([
                .. value.State.Select(x => (StateValue)x),
            ]),
            _ => null,
        };

    private static bool IsValid(Snapshot? value) =>
        value is not null
        && value.Step is >= 0 and < WizardState.TotalSteps
        && value.Jugglers is >= WizardState.MinJugglers and <= WizardState.MaxJugglersExact
        && value.Period is >= WizardState.MinPeriod and <= WizardState.MaxPeriod
        && value.MinClubs is >= WizardState.MinClubs and <= WizardState.MaxClubs
        && value.MaxClubs is >= WizardState.MinClubs and <= WizardState.MaxClubs
        && value.MaxThrowHeight is >= 0 and <= WizardState.AbsoluteMaxThrowHeight
        && value.Throws is { Length: <= 51 }
        && value.Throws.All(x => ThrowByHeight(x) is not null)
        && IsValidNode(value.Filter);

    private static bool IsValidNode(NodeDto? node) =>
        node switch
        {
            null => true,
            { Kind: "and" or "or", Children: { Length: > 0 and <= 100 } children } => children.All(
                IsValidNode
            ),
            { Kind: "leaf", Value: not null } leaf => FilterFromDto(leaf.Value) is not null,
            _ => false,
        };

    private static bool ValidHeights(int[]? values) =>
        values is { Length: > 0 and <= 100 } && values.All(x => ThrowByHeight(x) is not null);

    private static Throw? ThrowByHeight(int height) =>
        Throw
            .Everything(WizardState.AbsoluteMaxThrowHeight)
            .FirstOrDefault(x => x.Height == height);
}
