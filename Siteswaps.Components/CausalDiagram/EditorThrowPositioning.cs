namespace Siteswaps.Components.CausalDiagram;

public static class EditorThrowPositioning
{
    public static EditorThrow MoveStart(
        CausalDiagramEditorState state,
        EditorThrow item,
        decimal requestedStart
    )
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(item);

        var latestStart = state.CycleLength - 0.01m;
        var start = Math.Clamp(requestedStart, 0m, latestStart);
        if (state.SnapEnabled)
        {
            start =
                Math.Round(start / state.SnapStep, MidpointRounding.AwayFromZero) * state.SnapStep;
            // Keep the snapped node position inside this cycle.
            start = Math.Min(start, Math.Floor(latestStart / state.SnapStep) * state.SnapStep);
            start = Math.Max(0m, start);
        }

        return item with
        {
            StartTime = start,
        };
    }
}
