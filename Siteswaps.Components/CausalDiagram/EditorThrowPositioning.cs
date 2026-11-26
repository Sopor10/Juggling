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
        var earliestStart = 0m;
        var latestAllowedStart = latestStart;
        foreach (var edge in state.CausalEdges)
        {
            var source = state.Throws.FirstOrDefault(node => node.Id == edge.FromThrowId);
            var target = state.Throws.FirstOrDefault(node => node.Id == edge.ToThrowId);
            if (source is null || target is null || source.HeightIsExplicit)
                continue;

            var sourceBeat = state.CycleLength / Math.Max(1, state.Throws.Count(node => node.JugglerId == source.JugglerId));
            var targetBeat = state.CycleLength / Math.Max(1, state.Throws.Count(node => node.JugglerId == target.JugglerId));
            if (source.Id == item.Id)
                latestAllowedStart = Math.Min(latestAllowedStart, target.StartTime + edge.CycleOffset * state.CycleLength + 2m * targetBeat - 2m * sourceBeat);
            if (target.Id == item.Id)
                earliestStart = Math.Max(earliestStart, source.StartTime + 2m * sourceBeat - edge.CycleOffset * state.CycleLength - 2m * targetBeat);
        }

        if (earliestStart > latestAllowedStart)
            return item;

        var start = Math.Clamp(requestedStart, earliestStart, latestAllowedStart);
        if (state.SnapEnabled)
        {
            start =
                Math.Round(start / state.SnapStep, MidpointRounding.AwayFromZero) * state.SnapStep;
            start = Math.Clamp(start, earliestStart, latestAllowedStart);
        }

        return item with
        {
            StartTime = start,
        };
    }
}
