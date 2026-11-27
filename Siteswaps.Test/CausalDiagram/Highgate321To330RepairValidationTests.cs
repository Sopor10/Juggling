using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate321To330RepairValidationTests
{
    private static readonly HashSet<string> RepairedPresetIds =
    [
        "highgate-thirteenclubbookshelf",
        "highgate-thirteenclublowfatchocolatebox",
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Test]
    public void RepairedBookshelfRowsResolveToBalancedProductionPresetEdges()
    {
        var records = LoadRecords();
        var repaired = records.Where(record => RepairedPresetIds.Contains(record.Id)).ToArray();
        repaired.Select(record => record.Id).Should().BeEquivalentTo(RepairedPresetIds);

        foreach (var record in repaired)
        {
            record.SourceRows.Should().NotBeNull(record.Id);
            record.SourcePhases.Should().NotBeNull(record.Id);
            record.SourceFirstHands.Should().NotBeNull(record.Id);
            record.PassDirectionByJuggler.Should().NotBeNull(record.Id);

            var jugglerIds = record.Jugglers.ToDictionary(item => item.Name, item => item.Id);
            var throws = record.Throws.ToDictionary(item => item.Id);
            var expectedEdges = new List<(string From, string To, int CycleOffset)>();

            foreach (var (jugglerName, row) in record.SourceRows!)
            {
                var jugglerId = jugglerIds[jugglerName];
                var startTime = record.SourcePhases![jugglerName];
                var firstHand = record.SourceFirstHands![jugglerName];
                var route = record.PassDirectionByJuggler![jugglerName];
                var events = record.Throws
                    .Where(item => item.JugglerId == jugglerId)
                    .OrderBy(item => item.StartTime)
                    .ToArray();

                events[0].StartTime.Should().Be(startTime, $"{record.Id} {jugglerName} phase");
                events[0].Hand.Should().Be(firstHand, $"{record.Id} {jugglerName} first hand");

                for (var eventIndex = 0; eventIndex < events.Length; eventIndex++)
                {
                    var source = events[eventIndex];
                    var action = row[eventIndex % row.Length];
                    var targetJuggler = action == "self" ? jugglerId : jugglerIds[action];
                    var targetHand = action == "self"
                        ? Opposite(source.Hand)
                        : route == "crossing" ? source.Hand : Opposite(source.Hand);
                    var target = record.Throws
                        .Where(item => item.JugglerId == targetJuggler && item.Hand == targetHand)
                        .SelectMany(item => Enumerable.Range(-1, 4).Select(cycleOffset =>
                        (
                            Throw: item,
                            Offset: cycleOffset,
                            Time: item.StartTime + cycleOffset * record.CycleLength
                        )))
                        .Where(candidate => candidate.Time >= source.StartTime + 1)
                        .OrderBy(candidate => candidate.Time)
                        .First();

                    expectedEdges.Add((source.Id, target.Throw.Id, target.Offset));
                }
            }

            record.CausalEdges
                .Select(edge => (edge.From, edge.To, edge.CycleOffset))
                .Should()
                .BeEquivalentTo(expectedEdges, record.Id);

            var incoming = record.Throws.ToDictionary(item => item.Id, _ => 0);
            var outgoing = record.Throws.ToDictionary(item => item.Id, _ => 0);
            foreach (var edge in record.CausalEdges)
            {
                outgoing[edge.From]++;
                incoming[edge.To]++;
            }
            incoming.Should().OnlyContain(pair => pair.Value == outgoing[pair.Key], record.Id);

            var production = CausalDiagramEditorPresets.AllForLoading.Single(item => item.Id == record.Id).Create();
            production.IsValidPattern().Should().BeTrue(record.Id);
            production
                .CausalEdges.Select(edge => (edge.FromThrowId, edge.ToThrowId, edge.CycleOffset))
                .Should()
                .BeEquivalentTo(
                    expectedEdges.Select(edge => ((string?)edge.From, (string?)edge.To, edge.CycleOffset)),
                    record.Id
                );
        }
    }

    private static string Opposite(string hand) => hand == "R" ? "L" : "R";

    private static List<Record> LoadRecords()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            directory is not null
            && !File.Exists(Path.Combine(directory.FullName, "docs/research/highgate-transcriptions-321-330.json"))
        )
            directory = directory.Parent;
        directory.Should().NotBeNull();
        return JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(Path.Combine(directory!.FullName, "docs/research/highgate-transcriptions-321-330.json")),
            JsonOptions
        )!;
    }

    private sealed record Record(
        string Id,
        decimal CycleLength,
        List<Juggler> Jugglers,
        List<Throw> Throws,
        List<Edge> CausalEdges,
        Dictionary<string, string[]>? SourceRows = null,
        Dictionary<string, decimal>? SourcePhases = null,
        Dictionary<string, string>? SourceFirstHands = null,
        Dictionary<string, string>? PassDirectionByJuggler = null
    );

    private sealed record Juggler(string Id, string Name);

    private sealed record Throw(string Id, string JugglerId, string Hand, decimal StartTime);

    private sealed record Edge(string From, string To, int CycleOffset);
}
