using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate78To79SlowFastFollowupSourceTests
{
    private static readonly Dictionary<
        string,
        (string Destination, int LaneIndexOffset)[][]
    > ExpectedRows = new()
    {
        ["highgate-twocountvspasspassself"] =
        [
            [
                ("Ben", 1),
                ("Anne", 1),
                ("Ben", 0),
                ("Anne", 1),
                ("Ben", -1),
                ("Anne", 1),
                ("Ben", -1),
                ("Anne", 1),
            ],
            [("Anne", 1), ("Anne", 1), ("Ben", 1), ("Anne", 1), ("Anne", 2), ("Ben", 1)],
        ],
        ["highgate-passpassselfvsonecount"] =
        [
            [("Ben", 1), ("Anne", 1), ("Ben", 0), ("Ben", 0), ("Anne", 1), ("Ben", -1)],
            [("Anne", 1), ("Anne", 1), ("Anne", 2), ("Anne", 2)],
        ],
    };

    private static readonly Dictionary<string, decimal[]> ExpectedTimes = new()
    {
        ["highgate-twocountvspasspassself:Anne"] = [0m, 0.75m, 1.5m, 2.25m, 3m, 3.75m, 4.5m, 5.25m],
        ["highgate-twocountvspasspassself:Ben"] = [0m, 1m, 2m, 3m, 4m, 5m],
        ["highgate-passpassselfvsonecount:Anne"] =
        [
            0m,
            0.666667m,
            1.333333m,
            2m,
            2.666667m,
            3.333333m,
        ],
        ["highgate-passpassselfvsonecount:Ben"] = [0m, 1m, 2m, 3m],
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Test]
    public void RemainingSlowFastGraphsMatchSourceRowsAndImportedRuntimePresets()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-transcriptions-78-79-follow-up.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();
        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-78-79-follow-up.json"
                )
            ),
            JsonOptions
        )!;

        foreach (var record in records)
        {
            var edges = record.CausalEdges.ToDictionary(edge => edge.From);
            var namesById = record.Jugglers.ToDictionary(item => item.Id, item => item.Name);
            foreach (
                var (laneIndex, lane) in record.Jugglers.Select((item, index) => (index, item))
            )
            {
                var sources = record
                    .Throws.Where(item => item.JugglerId == lane.Id)
                    .OrderBy(item => item.StartTime)
                    .ToList();
                sources
                    .Select(item => item.StartTime)
                    .Should()
                    .Equal(ExpectedTimes[$"{record.Id}:{lane.Name}"]);
                var sourceRows = ExpectedRows[record.Id][laneIndex];
                foreach (var (index, source) in sources.Select((item, i) => (i, item)))
                {
                    var (destination, laneIndexOffset) = sourceRows[index];
                    var targetIndex = index + laneIndexOffset;
                    var destinationLane = record
                        .Throws.Where(item => namesById[item.JugglerId] == destination)
                        .OrderBy(item => item.StartTime)
                        .ToList();
                    var edge = edges[source.Id];
                    edge.To.Should()
                        .Be(destinationLane[targetIndex % destinationLane.Count].Id, record.Id);
                    edge.CycleOffset.Should().Be(targetIndex / destinationLane.Count, record.Id);
                }
            }

            var state = new CausalDiagramEditorState
            {
                PresetId = "highgate-three-count",
                ActiveJugglerId = record.Jugglers[0].Id,
                CycleLength = record.CycleLength,
                Jugglers = record
                    .Jugglers.Select(item => new EditorJuggler(item.Id, item.Name))
                    .ToList(),
                Throws = record
                    .Throws.Select(item => new EditorThrow(
                        item.Id,
                        item.JugglerId,
                        item.Hand,
                        item.StartTime,
                        0m,
                        false
                    ))
                    .ToList(),
                CausalEdges = record
                    .CausalEdges.Select(
                        (item, i) =>
                            new EditorCausalEdge($"check-{i}", item.From, item.To, item.CycleOffset)
                    )
                    .ToList(),
            };
            var offsets = state.CausalEdges.Select(item => item.CycleOffset).ToArray();
            state.SynchronizeHeights();
            state.CausalEdges.Select(item => item.CycleOffset).Should().Equal(offsets, record.Id);
            state.IsValidForEditor().Should().BeTrue(record.Id);

            var imported = CausalDiagramEditorPresets
                .AllForLoading.Single(preset => preset.Id == record.Id)
                .Create();
            imported.IsValidForEditor().Should().BeTrue(record.Id);
            imported.CycleLength.Should().Be(state.CycleLength, record.Id);
            imported
                .Throws.Select(item => (item.Id, item.JugglerId, item.Hand, item.StartTime))
                .Should()
                .Equal(
                    state.Throws.Select(item =>
                        (item.Id, item.JugglerId, item.Hand, item.StartTime)
                    ),
                    record.Id
                );
            imported
                .CausalEdges.Select(item => (item.FromThrowId, item.ToThrowId, item.CycleOffset))
                .Should()
                .Equal(
                    state.CausalEdges.Select(item =>
                        (item.FromThrowId, item.ToThrowId, item.CycleOffset)
                    ),
                    record.Id
                );
        }

        records.Should().HaveCount(2);
    }

    private sealed record Record(
        string Id,
        decimal CycleLength,
        List<Juggler> Jugglers,
        List<Throw> Throws,
        List<Edge> CausalEdges
    );

    private sealed record Juggler(string Id, string Name);

    private sealed record Throw(string Id, string JugglerId, string Hand, decimal StartTime);

    private sealed record Edge(string From, string To, int CycleOffset);
}
