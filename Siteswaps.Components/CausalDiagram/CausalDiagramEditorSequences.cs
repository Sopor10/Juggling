namespace Siteswaps.Components.CausalDiagram;

using System.Text.Json;

/// <summary>A finite ordered sequence of periodic causal graphs and between-graph events.</summary>
public sealed record CausalDiagramSequencePhase(string StateId, string Label);

public sealed record CausalDiagramTransitionEvent(
    int AfterPhaseIndex,
    string Actor,
    string Action,
    decimal DurationBeats,
    int SelfThrows
);

public sealed record CausalDiagramEditorSequence(
    string Id,
    string Label,
    IReadOnlyList<CausalDiagramSequencePhase> Phases,
    IReadOnlyList<CausalDiagramTransitionEvent> Transitions
);

/// <summary>Curated finite sequences. State graphs live in their own source records.</summary>
public static class CausalDiagramEditorSequences
{
    private const string RunawayStatesResource =
        "Siteswaps.Components.CausalDiagram.Data.highgate-runaway-source-states.json";
    private static readonly Dictionary<string, CausalDiagramEditorState> RunawayStates =
        LoadRunawayStates();

    public static IReadOnlyList<CausalDiagramEditorSequence> All { get; } =
    [
        new(
            "highgate-thirteenclubrunaway",
            "Thirteen club runaway",
            [
                new(
                    "highgate-thirteenclubrunaway-initial-urban-terror",
                    "Initial urban-terror feed"
                ),
                new(
                    "highgate-thirteenclubrunaway-after-daniel-walk",
                    "Pass-pass-self after Daniel walks"
                ),
                new(
                    "highgate-thirteenclubrunaway-before-anne-walk",
                    "Pass-pass-self before Anne walks"
                ),
                new(
                    "highgate-thirteenclubrunaway-after-anne-walk",
                    "Urban-terror after Anne walks"
                ),
            ],
            [
                new(
                    0,
                    "Daniel",
                    "Walks across after a pass; the group changes to pass-pass-self.",
                    2.5m,
                    3
                ),
                new(
                    2,
                    "Anne",
                    "Walks across after passing with Daniel; the group returns to urban-terror.",
                    2.5m,
                    3
                ),
            ]
        ),
    ];

    public static IEnumerable<CausalDiagramEditorPreset> RunawayPhasePresets =>
        All.Single(sequence => sequence.Id == "highgate-thirteenclubrunaway")
            .Phases.Select(phase => new CausalDiagramEditorPreset(
                phase.StateId,
                phase.Label,
                "Highgate Collection · Runaway state",
                () => CreateRunawayPhase(phase.StateId)
            ));

    public static CausalDiagramEditorState CreateRunawayPhase(string stateId)
    {
        if (!RunawayStates.TryGetValue(stateId, out var state))
            throw new ArgumentException("Unknown Runaway phase.", nameof(stateId));
        var copy = new CausalDiagramEditorState
        {
            PresetId = state.PresetId,
            ActiveJugglerId = state.ActiveJugglerId,
            CycleLength = state.CycleLength,
            Jugglers = [.. state.Jugglers],
            Throws = [.. state.Throws],
            CausalEdges = [.. state.CausalEdges],
        };
        copy.NormalizeCausalEdgeOffsets();
        return copy;
    }

    private static Dictionary<string, CausalDiagramEditorState> LoadRunawayStates()
    {
        using var stream = typeof(CausalDiagramEditorSequences).Assembly.GetManifestResourceStream(
            RunawayStatesResource
        )
            ?? throw new InvalidOperationException(
                $"Missing embedded resource {RunawayStatesResource}."
            );
        using var document = JsonDocument.Parse(stream);
        var result = new Dictionary<string, CausalDiagramEditorState>(StringComparer.Ordinal);
        foreach (var item in document.RootElement.EnumerateArray())
        {
            var id = ReadString(item.GetProperty("id"));
            var cycleLength = item.GetProperty("cycleLength").GetDecimal();
            var jugglers = item.GetProperty("jugglers")
                .EnumerateArray()
                .Select(juggler => new EditorJuggler(
                    ReadString(juggler.GetProperty("id")),
                    ReadString(juggler.GetProperty("name"))
                ))
                .ToList();
            var throws = item.GetProperty("throws")
                .EnumerateArray()
                .Select(node => new EditorThrow(
                    ReadString(node.GetProperty("id")),
                    ReadString(node.GetProperty("jugglerId")),
                    ReadString(node.GetProperty("hand")),
                    node.GetProperty("startTime").GetDecimal()
                ))
                .ToList();
            var edges = item.GetProperty("causalEdges")
                .EnumerateArray()
                .Select(
                    (edge, index) =>
                        new EditorCausalEdge(
                            $"runaway-{index + 1}",
                            ReadString(edge.GetProperty("from")),
                            ReadString(edge.GetProperty("to")),
                            edge.GetProperty("cycleOffset").GetInt32()
                        )
                )
                .ToList();
            var state = new CausalDiagramEditorState
            {
                PresetId = id,
                CycleLength = cycleLength,
                ActiveJugglerId = jugglers[0].Id,
                Jugglers = jugglers,
                Throws = throws,
                CausalEdges = edges,
            };
            state.NormalizeCausalEdgeOffsets();
            result.Add(id, state);
        }
        return result;
    }

    private static string ReadString(JsonElement value) =>
        value.GetString()
        ?? throw new InvalidOperationException("Runaway source state has a missing string value.");

    public static bool IsValidSelection(string id, int phaseIndex) =>
        All.Any(sequence =>
            sequence.Id == id && phaseIndex >= 0 && phaseIndex < sequence.Phases.Count
        );
}
