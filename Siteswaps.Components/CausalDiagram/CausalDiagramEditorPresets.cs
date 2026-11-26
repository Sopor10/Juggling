namespace Siteswaps.Components.CausalDiagram;

public sealed record CausalDiagramEditorPreset(
    string Id,
    string Label,
    string Group,
    Func<CausalDiagramEditorState> Create
);

public static class CausalDiagramEditorPresets
{
    private const string AsynchronousGroup = "Asynchron";
    private const string SynchronousGroup = "Synchron";

    public static IReadOnlyList<CausalDiagramEditorPreset> All { get; } =
        new CausalDiagramEditorPreset[]
        {
            Siteswap("siteswap-3", "3 · 3-Keulen-Kaskade", "3"),
            Siteswap("siteswap-4", "4 · Fountain", "4"),
            Siteswap("siteswap-5", "5 · Cascade", "5"),
            Siteswap("siteswap-531", "531", "531"),
            Siteswap("siteswap-441", "441", "441"),
            Siteswap("siteswap-423", "423", "423"),
            Siteswap("siteswap-97531", "97531", "97531"),
            FourHanded("four-hand-period-3", "4-händig · Periode 3", 3),
            FourHanded("four-hand-period-4", "4-händig · Periode 4", 4),
            FourHanded("four-hand-period-5", "4-händig · Periode 5", 5),
            Feeding("feeding-3-3", "3:3 Polyrhythmus", 3, 3),
            Feeding("feeding-3-4", "3:4 Polyrhythmus", 3, 4),
            Feeding("feeding-5-3", "5:3 Polyrhythmus", 5, 3),
            new(
                "feeding-7-5",
                "2 Jongleure · Polyrhythmus 7:5",
                AsynchronousGroup,
                CausalDiagramEditorState.CreateFeeding75
            ),
            Juggler("jugglers-1", "1 Jongleur", 1),
            Juggler("jugglers-2", "2 Jongleure · asynchron", 2),
            Juggler("jugglers-3", "3 Jongleure", 3),
            Juggler("jugglers-4", "4 Jongleure", 4),
            new(
                "jugglers-2-synchronous-4",
                "2 Jongleure · synchron (4 Beats)",
                SynchronousGroup,
                CreateSynchronousFourBeatPair
            ),
        }
            .Concat(HighgateCausalDiagramPresets.All)
            .ToArray();

    public static IEnumerable<CausalDiagramEditorPreset> VisibleAsynchronous =>
        All.Where(preset =>
            preset.Id
                is "siteswap-3"
                    or "feeding-7-5"
                    or "jugglers-1"
                    or "jugglers-2"
                    or "jugglers-3"
                    or "jugglers-4"
        );

    public static IEnumerable<CausalDiagramEditorPreset> VisibleSynchronous =>
        All.Where(preset => preset.Id == "jugglers-2-synchronous-4");

    public static IEnumerable<CausalDiagramEditorPreset> VisibleHighgate =>
        All.Where(preset => preset.Group == "Highgate Collection");

    private static CausalDiagramEditorPreset Siteswap(string id, string label, string pattern) =>
        new(id, label, AsynchronousGroup, () =>
            CausalDiagramEditorState.CreateSiteswapPattern(pattern, id)
        );

    private static CausalDiagramEditorPreset FourHanded(string id, string label, int period) =>
        new(id, label, SynchronousGroup, () => CreateFourHanded(period));

    private static CausalDiagramEditorPreset Feeding(
        string id,
        string label,
        int first,
        int second
    ) =>
        new(id, label, AsynchronousGroup, () =>
            CausalDiagramEditorState.CreateFeedingPattern(first, second, id)
        );

    private static CausalDiagramEditorPreset Juggler(string id, string label, int count) =>
        new(id, label, AsynchronousGroup, () => CreateJugglerDefault(count));

    private static CausalDiagramEditorState CreateJugglerDefault(int jugglerCount)
    {
        var names = Enumerable
            .Range(0, jugglerCount)
            .Select(index => ((char)('A' + index)).ToString())
            .ToArray();
        var lines = names.Select(
            (name, index) =>
            {
                var phaseOffset = (decimal)index / jugglerCount;
                var nextJuggler = names[(index + 1) % jugglerCount];
                return $"{name}: (RLR {phaseOffset.ToString(System.Globalization.CultureInfo.InvariantCulture)}) 3>{nextJuggler}, 3>{nextJuggler}, 3>{nextJuggler}";
            }
        );

        if (
            !CausalDiagramEditorState.TryCreateFeeding(
                string.Join('\n', lines),
                out var state,
                out _
            ) || state is null
        )
            throw new InvalidOperationException(
                $"Die Vorlage für {jugglerCount} Jongleure konnte nicht erstellt werden."
            );

        state.PresetId = $"jugglers-{jugglerCount}";
        return state;
    }

    private static CausalDiagramEditorState CreateSynchronousFourBeatPair()
    {
        const string notation = "A: (RLRL 0) 3>B, 3>B, 3>B, 3>B\nB: (RLRL 0) 3>A, 3>A, 3>A, 3>A";
        if (
            !CausalDiagramEditorState.TryCreateFeeding(notation, out var state, out _)
            || state is null
        )
            throw new InvalidOperationException(
                "Die synchrone 4-Beat-Vorlage konnte nicht erstellt werden."
            );

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
