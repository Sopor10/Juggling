namespace Siteswaps.Components.CausalDiagram;

public sealed record CausalDiagramEditorPreset(string Id, string Label, string Group, Func<CausalDiagramEditorState> Create);

public static class CausalDiagramEditorPresets
{
    public static IReadOnlyList<CausalDiagramEditorPreset> All { get; } =
    [
        new("siteswap-3", "3 · Cascade", "Siteswap", () => CausalDiagramEditorState.CreateSiteswapPattern("3", "siteswap-3")),
        new("siteswap-4", "4 · Fountain", "Siteswap", () => CausalDiagramEditorState.CreateSiteswapPattern("4", "siteswap-4")),
        new("siteswap-5", "5 · Cascade", "Siteswap", () => CausalDiagramEditorState.CreateSiteswapPattern("5", "siteswap-5")),
        new("siteswap-531", "531", "Siteswap", () => CausalDiagramEditorState.CreateSiteswapPattern("531", "siteswap-531")),
        new("siteswap-441", "441", "Siteswap", () => CausalDiagramEditorState.CreateSiteswapPattern("441", "siteswap-441")),
        new("siteswap-423", "423", "Siteswap", () => CausalDiagramEditorState.CreateSiteswapPattern("423", "siteswap-423")),
        new("siteswap-97531", "97531", "Siteswap", () => CausalDiagramEditorState.CreateSiteswapPattern("97531", "siteswap-97531")),
        new("feeding-3-3", "3:3 Polyrhythmus", "Polyrhythmus", () => CausalDiagramEditorState.CreateFeedingPattern(3, 3, "feeding-3-3")),
        new("feeding-5-3", "5:3 Polyrhythmus", "Polyrhythmus", () => CausalDiagramEditorState.CreateFeedingPattern(5, 3, "feeding-5-3")),
        new("feeding-7-5", "7:5 Polyrhythmus", "Polyrhythmus", CausalDiagramEditorState.CreateFeeding75)
    ];

    public static bool IsKnownId(string? id) =>
        id is "custom" or "siteswap531" or "feeding75" || All.Any(preset => preset.Id == id);
}
