using System.Globalization;
using Siteswaps.Components.Details;

namespace Siteswaps.Components.CausalDiagram;

/// <summary>Projects editor throws onto a shared time grid, preserving asynchronous offsets.</summary>
public static class CausalDiagramEditorTableAdapter
{
    public sealed record Row(string Juggler, IReadOnlyList<DetailViewModel.ThrowChip?> Cells);

    public sealed record Table(IReadOnlyList<decimal> Times, IReadOnlyList<Row> Rows);

    public static Table Map(CausalDiagramEditorState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var cycleLength = Math.Max(0m, state.CycleLength);
        var repetitions = Math.Max(1, state.Repetitions);
        var events = state
            .Throws.Where(item => state.Jugglers.Any(juggler => juggler.Id == item.JugglerId))
            .SelectMany(item =>
                Enumerable
                    .Range(0, repetitions)
                    .Select(cycle =>
                    {
                        var time = item.StartTime + cycle * cycleLength;
                        var juggler = state.Jugglers.First(candidate =>
                            candidate.Id == item.JugglerId
                        );
                        var edge = state.CausalEdges.FirstOrDefault(candidate =>
                            candidate.FromThrowId == item.Id
                        );
                        var targetJuggler = edge is null
                            ? null
                            : state
                                .Throws.FirstOrDefault(candidate => candidate.Id == edge.ToThrowId)
                                ?.JugglerId;
                        var kind =
                            targetJuggler is not null && targetJuggler != juggler.Id
                                ? Siteswap.Details.PassOrSelf.Pass
                                : Siteswap.Details.PassOrSelf.Self;
                        return (
                            JugglerId: juggler.Id,
                            Time: time,
                            Chip: new DetailViewModel.ThrowChip(
                                state
                                    .HeightOf(item.Id)
                                    ?.ToString("0.##", CultureInfo.InvariantCulture)
                                    ?? "—",
                                kind,
                                Hand: item.Hand
                            )
                        );
                    })
            )
            .ToList();

        var times = events.Select(item => item.Time).Distinct().Order().ToList();
        var rows = state
            .Jugglers.Select(juggler => new Row(
                juggler.Name,
                times
                    .Select(time =>
                        events
                            .Where(item => item.JugglerId == juggler.Id && item.Time == time)
                            .Select(item => item.Chip)
                            .FirstOrDefault()
                    )
                    .ToList()
            ))
            .ToList();
        return new Table(times, rows);
    }
}
