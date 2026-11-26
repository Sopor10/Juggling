using System.Globalization;
using Siteswaps.Components.Details;

namespace Siteswaps.Components.CausalDiagram;

/// <summary>Projects editor throws onto a shared time grid, preserving asynchronous offsets.</summary>
public static class CausalDiagramEditorTableAdapter
{
    public sealed record Row(
        string JugglerId,
        string Juggler,
        string? Hand,
        IReadOnlyList<IReadOnlyList<DetailViewModel.ThrowChip>> Cells
    );

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
                                item.Height.ToString("0.##", CultureInfo.InvariantCulture),
                                kind,
                                Hand: item.Hand
                            )
                        );
                    })
            )
            .ToList();

        var times = events.Select(item => item.Time).Distinct().Order().ToList();
        var rows = new List<Row>();
        foreach (var juggler in state.Jugglers)
        {
            var jugglerEvents = events.Where(item => item.JugglerId == juggler.Id).ToList();
            var hasSimultaneousEvents = jugglerEvents
                .GroupBy(item => item.Time)
                .Any(group => group.Count() > 1);
            var hands = hasSimultaneousEvents ? new string?[] { "L", "R" } : [null];

            foreach (var hand in hands)
            {
                rows.Add(
                    new Row(
                        juggler.Id,
                        juggler.Name,
                        hand,
                        times
                            .Select(time =>
                                (IReadOnlyList<DetailViewModel.ThrowChip>)
                                    jugglerEvents
                                        .Where(item =>
                                            item.Time == time
                                            && (hand is null || item.Chip.Hand == hand)
                                        )
                                        .Select(item => item.Chip)
                                        .ToList()
                            )
                            .ToList()
                    )
                );
            }
        }
        return new Table(times, rows);
    }
}
