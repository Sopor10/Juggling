namespace Siteswaps.Components.CausalDiagram;

/// <summary>Calculates SVG lanes without changing a throw's event time.</summary>
public static class CausalDiagramEditorLayout
{
    public const int ThrowHitTargetRadius = 28;
    private const int HandLaneOffset = 30;
    private const decimal MinimumSpacingForHandLanes = 120m;

    public static bool HasHandTracks(CausalDiagramEditorState state, string jugglerId) =>
        state
            .Throws.Where(item => item.JugglerId == jugglerId)
            .GroupBy(item => item.StartTime)
            .Any(group => group.Count() > 1);

    public static bool HasAnyHandTracks(CausalDiagramEditorState state) =>
        state
            .Throws.GroupBy(item => (item.JugglerId, item.StartTime))
            .Any(group => group.Count() > 1);

    public static decimal EffectiveLaneSpacing(CausalDiagramEditorState state) =>
        HasAnyHandTracks(state)
            ? Math.Max(MinimumSpacingForHandLanes, state.LaneSpacing)
            : state.LaneSpacing;

    public static int JugglerY(CausalDiagramEditorState state, int jugglerIndex) =>
        70 + (int)(EffectiveLaneSpacing(state) * jugglerIndex);

    public static int ThrowY(CausalDiagramEditorState state, EditorThrow item)
    {
        var jugglerIndex = state.Jugglers.FindIndex(juggler => juggler.Id == item.JugglerId);
        var offset = HasHandTracks(state, item.JugglerId)
            ? item.Hand == "L"
                ? -HandLaneOffset
                : HandLaneOffset
            : 0;
        return JugglerY(state, jugglerIndex) + offset;
    }
}
