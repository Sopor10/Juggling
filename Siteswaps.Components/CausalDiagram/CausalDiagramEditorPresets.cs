namespace Siteswaps.Components.CausalDiagram;

public sealed record CausalDiagramEditorPreset(
    string Id,
    string Label,
    string Group,
    Func<CausalDiagramEditorState> Create
);

public static class CausalDiagramEditorPresets
{
    public static IReadOnlyList<CausalDiagramEditorPreset> All { get; } =
    [
        new(
            "siteswap-3",
            "3 · 3-Keulen-Kaskade",
            "Asynchron",
            () => CausalDiagramEditorState.CreateSiteswapPattern("3", "siteswap-3")
        ),
        new(
            "siteswap-4",
            "4 · Fountain",
            "Asynchron",
            () => CausalDiagramEditorState.CreateSiteswapPattern("4", "siteswap-4")
        ),
        new(
            "siteswap-5",
            "5 · Cascade",
            "Asynchron",
            () => CausalDiagramEditorState.CreateSiteswapPattern("5", "siteswap-5")
        ),
        new(
            "siteswap-531",
            "531",
            "Asynchron",
            () => CausalDiagramEditorState.CreateSiteswapPattern("531", "siteswap-531")
        ),
        new(
            "siteswap-441",
            "441",
            "Asynchron",
            () => CausalDiagramEditorState.CreateSiteswapPattern("441", "siteswap-441")
        ),
        new(
            "siteswap-423",
            "423",
            "Asynchron",
            () => CausalDiagramEditorState.CreateSiteswapPattern("423", "siteswap-423")
        ),
        new(
            "siteswap-97531",
            "97531",
            "Asynchron",
            () => CausalDiagramEditorState.CreateSiteswapPattern("97531", "siteswap-97531")
        ),
        new("four-hand-period-3", "4-händig · Periode 3", "Synchron", () => CreateFourHanded(3)),
        new("four-hand-period-4", "4-händig · Periode 4", "Synchron", () => CreateFourHanded(4)),
        new("four-hand-period-5", "4-händig · Periode 5", "Synchron", () => CreateFourHanded(5)),
        new(
            "feeding-3-3",
            "3:3 Polyrhythmus",
            "Asynchron",
            () => CausalDiagramEditorState.CreateFeedingPattern(3, 3, "feeding-3-3")
        ),
        new(
            "feeding-3-4",
            "3:4 Polyrhythmus",
            "Asynchron",
            () => CausalDiagramEditorState.CreateFeedingPattern(3, 4, "feeding-3-4")
        ),
        new(
            "feeding-5-3",
            "5:3 Polyrhythmus",
            "Asynchron",
            () => CausalDiagramEditorState.CreateFeedingPattern(5, 3, "feeding-5-3")
        ),
        new(
            "feeding-7-5",
            "2 Jongleure · Polyrhythmus 7:5",
            "Asynchron",
            CausalDiagramEditorState.CreateFeeding75
        ),
        new("jugglers-1", "1 Jongleur", "Asynchron", () => CreateJugglerDefault(1)),
        new("jugglers-2", "2 Jongleure", "Asynchron", () => CreateJugglerDefault(2)),
        new("jugglers-3", "3 Jongleure", "Asynchron", () => CreateJugglerDefault(3)),
        new("jugglers-4", "4 Jongleure", "Asynchron", () => CreateJugglerDefault(4)),
        new(
            "jugglers-2-synchronous-4",
            "2 Jongleure · synchron (4 Beats)",
            "Synchron",
            CreateSynchronousFourBeatPair
        ),
    ];

    public static IEnumerable<CausalDiagramEditorPreset> VisibleAsynchronous =>
        All.Where(preset =>
            preset.Id is "siteswap-3"
                or "feeding-7-5"
                or "jugglers-1"
                or "jugglers-2"
                or "jugglers-3"
                or "jugglers-4"
        );

    public static IEnumerable<CausalDiagramEditorPreset> VisibleSynchronous =>
        All.Where(preset => preset.Id == "jugglers-2-synchronous-4");

    private static CausalDiagramEditorState CreateJugglerDefault(int jugglerCount)
    {
        var names = Enumerable.Range(0, jugglerCount)
            .Select(index => ((char)('A' + index)).ToString())
            .ToArray();
        var lines = names.Select((name, index) =>
        {
            var phaseOffset = (decimal)index / jugglerCount;
            var nextJuggler = names[(index + 1) % jugglerCount];
            return $"{name}: (RLR {phaseOffset.ToString(System.Globalization.CultureInfo.InvariantCulture)}) 3>{nextJuggler}, 3>{nextJuggler}, 3>{nextJuggler}";
        });

        if (!CausalDiagramEditorState.TryCreateFeeding(string.Join('\n', lines), out var state, out _) || state is null)
            throw new InvalidOperationException($"Die Vorlage für {jugglerCount} Jongleure konnte nicht erstellt werden.");

        state.PresetId = $"jugglers-{jugglerCount}";
        return state;
    }

    private static CausalDiagramEditorState CreateSynchronousFourBeatPair()
    {
        const string notation = "A: (RLRL 0) 3>B, 3>B, 3>B, 3>B\nB: (RLRL 0) 3>A, 3>A, 3>A, 3>A";
        if (!CausalDiagramEditorState.TryCreateFeeding(notation, out var state, out _) || state is null)
            throw new InvalidOperationException("Die synchrone 4-Beat-Vorlage konnte nicht erstellt werden.");

        state.PresetId = "jugglers-2-synchronous-4";
        return state;
    }

    private static CausalDiagramEditorState CreateFourHanded(int period)
    {
        var firstHands = string.Concat(
            Enumerable.Range(0, period).Select(beat => beat % 2 == 0 ? 'R' : 'L')
        );
        var secondHands = string.Concat(
            Enumerable.Range(0, period).Select(beat => beat % 2 == 0 ? 'R' : 'L')
        );
        var firstThrows = string.Join(", ", Enumerable.Repeat("3>B", period));
        var secondThrows = string.Join(", ", Enumerable.Repeat("3>A", period));
        var notation = $"A: ({firstHands} 0) {firstThrows}\nB: ({secondHands} 0.5) {secondThrows}";
        if (
            !CausalDiagramEditorState.TryCreateFeeding(notation, out var state, out _)
            || state is null
        )
            throw new InvalidOperationException(
                $"Die 4-händige Vorlage mit Periode {period} konnte nicht erstellt werden."
            );
        state.PresetId = $"four-hand-period-{period}";
        return state;
    }

    public static bool IsKnownId(string? id) =>
        id is "custom" or "siteswap531" or "feeding75" or "feeding-5-3"
        || All.Any(preset => preset.Id == id);
}
