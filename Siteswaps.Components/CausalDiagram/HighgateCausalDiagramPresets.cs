using System.Text.Json;

namespace Siteswaps.Components.CausalDiagram;

/// <summary>Loads the curated Highgate source data independently of the editor presets.</summary>
internal static class HighgateCausalDiagramPresets
{
    private const string ResourceName =
        "Siteswaps.Components.CausalDiagram.Data.highgate-causal-patterns.json";
    private const string WorkInProgressResourceName =
        "Siteswaps.Components.CausalDiagram.Data.highgate-causal-patterns-work-in-progress.json";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly List<HighgatePatternData> Patterns = Load(ResourceName);
    private static readonly List<HighgatePatternData> WorkInProgressPatterns = Load(
        WorkInProgressResourceName
    );

    public static IEnumerable<CausalDiagramEditorPreset> All =>
        ToPresets(Patterns);

    public static IEnumerable<CausalDiagramEditorPreset> WorkInProgress =>
        ToPresets(WorkInProgressPatterns);

    private static IEnumerable<CausalDiagramEditorPreset> ToPresets(
        IEnumerable<HighgatePatternData> patterns
    ) => patterns.Select(pattern => new CausalDiagramEditorPreset(
            pattern.Id,
            pattern.Label,
            pattern.Group,
            () => CreateState(pattern)
        ));

    private static List<HighgatePatternData> Load(string resourceName)
    {
        using var stream = typeof(HighgateCausalDiagramPresets).Assembly.GetManifestResourceStream(
            resourceName
        ) ?? throw new InvalidOperationException($"Missing embedded resource {resourceName}.");
        var patterns = JsonSerializer.Deserialize<List<HighgatePatternData>>(stream, JsonOptions)
            ?? throw new InvalidOperationException("The Highgate pattern data is empty.");
        if (
            patterns
                .SelectMany(pattern => pattern.Throws)
                .Any(item => item.HeightIsExplicit && item.Height is null or < 0)
        )
        {
            throw new InvalidDataException(
                "Explicit Highgate throw heights must be present and nonnegative."
            );
        }
        return patterns;
    }

    private static CausalDiagramEditorState CreateState(HighgatePatternData pattern)
    {
        var state = new CausalDiagramEditorState
        {
            PresetId = pattern.Id,
            CycleLength = pattern.CycleLength,
            ActiveJugglerId = pattern.Jugglers[0].Id,
            Jugglers = pattern
                .Jugglers.Select(juggler => new EditorJuggler(juggler.Id, juggler.Name))
                .ToList(),
            Throws = pattern
                .Throws.Select(item => new EditorThrow(
                    item.Id,
                    item.JugglerId,
                    item.Hand,
                    item.StartTime,
                    item.Height ?? 0m,
                    item.HeightIsExplicit
                ))
                .ToList(),
            CausalEdges = pattern
                .CausalEdges.Select(
                    (edge, index) =>
                        new EditorCausalEdge(
                            $"{pattern.Id}-e{index + 1}",
                            edge.From,
                            edge.To,
                            edge.CycleOffset
                        )
                )
                .ToList(),
        };
        var sourceCycleOffsets = state.CausalEdges.Select(edge => edge.CycleOffset).ToArray();
        state.SynchronizeHeights();
        if (
            state
                .CausalEdges.Where((edge, index) => edge.CycleOffset != sourceCycleOffsets[index])
                .Any()
        )
        {
            throw new InvalidDataException(
                $"Highgate preset '{pattern.Id}' has a causal edge the editor would silently retime."
            );
        }
        if (!state.IsValidForEditor())
        {
            throw new InvalidDataException(
                $"Highgate preset '{pattern.Id}' does not describe a valid causal pattern."
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

    private sealed record ThrowData(
        string Id,
        string JugglerId,
        string Hand,
        decimal StartTime,
        decimal? Height = null,
        bool HeightIsExplicit = false
    );

    private sealed record EdgeData(string From, string To, int CycleOffset);

    private sealed record SourceData(string Edition, string Url, int? PrintedPage, int PdfPage);
}
