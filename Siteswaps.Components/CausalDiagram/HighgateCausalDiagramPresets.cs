using System.Text.Json;

namespace Siteswaps.Components.CausalDiagram;

/// <summary>Loads the curated Highgate source data independently of the editor presets.</summary>
internal static class HighgateCausalDiagramPresets
{
    private const string ResourceName =
        "Siteswaps.Components.CausalDiagram.Data.highgate-causal-patterns.json";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly List<HighgatePatternData> Patterns = Load();

    public static IEnumerable<CausalDiagramEditorPreset> All =>
        Patterns.Select(pattern => new CausalDiagramEditorPreset(
            pattern.Id,
            pattern.Label,
            pattern.Group,
            () => CreateState(pattern)
        ));

    private static List<HighgatePatternData> Load()
    {
        using var stream =
            typeof(HighgateCausalDiagramPresets).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource {ResourceName}.");
        return JsonSerializer.Deserialize<List<HighgatePatternData>>(stream, JsonOptions)
            ?? throw new InvalidOperationException("The Highgate pattern data is empty.");
    }

    private static CausalDiagramEditorState CreateState(HighgatePatternData pattern)
    {
        var state = new CausalDiagramEditorState
        {
            PresetId = pattern.Id,
            CycleLength = pattern.CycleLength,
            ActiveJugglerId = pattern.Jugglers[0].Id,
            Jugglers = pattern.Jugglers
                .Select(juggler => new EditorJuggler(juggler.Id, juggler.Name))
                .ToList(),
            Throws = pattern.Throws
                .Select(item => new EditorThrow(
                    item.Id,
                    item.JugglerId,
                    item.Hand,
                    item.StartTime,
                    0m
                ))
                .ToList(),
            CausalEdges = pattern.CausalEdges
                .Select((edge, index) => new EditorCausalEdge(
                    $"{pattern.Id}-e{index + 1}",
                    edge.From,
                    edge.To,
                    edge.CycleOffset
                ))
                .ToList(),
        };
        var sourceCycleOffsets = state.CausalEdges.Select(edge => edge.CycleOffset).ToArray();
        state.SynchronizeHeights();
        if (
            state.CausalEdges.Where((edge, index) => edge.CycleOffset != sourceCycleOffsets[index])
                .Any()
        )
        {
            throw new InvalidDataException(
                $"Highgate preset '{pattern.Id}' has a causal edge the editor would silently retime."
            );
        }
        return state;
    }

    private sealed record HighgatePatternData(
        string Id,
        string Label,
        string Group,
        string Source,
        decimal CycleLength,
        List<JugglerData> Jugglers,
        List<ThrowData> Throws,
        List<EdgeData> CausalEdges,
        string? Notation = null,
        List<SourceData>? SourceLinks = null
    );

    private sealed record JugglerData(string Id, string Name);

    private sealed record ThrowData(string Id, string JugglerId, string Hand, decimal StartTime);

    private sealed record EdgeData(string From, string To, int CycleOffset);

    private sealed record SourceData(string Edition, string Url, int? PrintedPage, int PdfPage);
}
