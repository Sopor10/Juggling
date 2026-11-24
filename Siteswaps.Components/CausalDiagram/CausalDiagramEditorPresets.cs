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
            "3 · Cascade",
            "Siteswap",
            () => CausalDiagramEditorState.CreateSiteswapPattern("3", "siteswap-3")
        ),
        new(
            "siteswap-4",
            "4 · Fountain",
            "Siteswap",
            () => CausalDiagramEditorState.CreateSiteswapPattern("4", "siteswap-4")
        ),
        new(
            "siteswap-5",
            "5 · Cascade",
            "Siteswap",
            () => CausalDiagramEditorState.CreateSiteswapPattern("5", "siteswap-5")
        ),
        new(
            "siteswap-531",
            "531",
            "Siteswap",
            () => CausalDiagramEditorState.CreateSiteswapPattern("531", "siteswap-531")
        ),
        new(
            "siteswap-441",
            "441",
            "Siteswap",
            () => CausalDiagramEditorState.CreateSiteswapPattern("441", "siteswap-441")
        ),
        new(
            "siteswap-423",
            "423",
            "Siteswap",
            () => CausalDiagramEditorState.CreateSiteswapPattern("423", "siteswap-423")
        ),
        new(
            "siteswap-97531",
            "97531",
            "Siteswap",
            () => CausalDiagramEditorState.CreateSiteswapPattern("97531", "siteswap-97531")
        ),
        new("four-hand-period-3", "4-händig · Periode 3", "4-händig", () => CreateFourHanded(3)),
        new("four-hand-period-4", "4-händig · Periode 4", "4-händig", () => CreateFourHanded(4)),
        new("four-hand-period-5", "4-händig · Periode 5", "4-händig", () => CreateFourHanded(5)),
        new(
            "feeding-3-3",
            "3:3 Polyrhythmus",
            "Polyrhythmus",
            () => CausalDiagramEditorState.CreateFeedingPattern(3, 3, "feeding-3-3")
        ),
        new(
            "feeding-3-4",
            "3:4 Polyrhythmus",
            "Polyrhythmus",
            () => CausalDiagramEditorState.CreateFeedingPattern(3, 4, "feeding-3-4")
        ),
        new(
            "feeding-5-3",
            "5:3 Polyrhythmus",
            "Polyrhythmus",
            () => CausalDiagramEditorState.CreateFeedingPattern(5, 3, "feeding-5-3")
        ),
        new(
            "feeding-7-5",
            "7:5 Polyrhythmus",
            "Polyrhythmus",
            CausalDiagramEditorState.CreateFeeding75
        ),
    ];

    public static IEnumerable<CausalDiagramEditorPreset> VisibleSiteswap =>
        All.Where(preset => preset.Id is "siteswap-3" or "siteswap-97531");

    public static IEnumerable<CausalDiagramEditorPreset> VisiblePolyrhythms =>
        All.Where(preset => preset.Id is "feeding-3-4" or "feeding-7-5");

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
