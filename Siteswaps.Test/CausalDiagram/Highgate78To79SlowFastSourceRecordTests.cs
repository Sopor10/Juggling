using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate78To79SlowFastSourceRecordTests
{
    private static readonly Dictionary<
        string,
        (string Destination, int LaneIndexOffset)[][]
    > ExpectedRows = new()
    {
        ["highgate-fourcountvsthreecount"] =
        [
            [
                ("Ben", 1),
                ("Anne", 1),
                ("Anne", 1),
                ("Anne", 1),
                ("Ben", 0),
                ("Anne", 1),
                ("Anne", 1),
                ("Anne", 1),
            ],
            [("Anne", 1), ("Ben", 1), ("Ben", 1), ("Anne", 1), ("Ben", 1), ("Ben", 1)],
        ],
        ["highgate-threecountvstwocount"] =
        [
            [("Ben", 1), ("Anne", 1), ("Anne", 1), ("Ben", 0), ("Anne", 1), ("Anne", 1)],
            [("Anne", 1), ("Ben", 1), ("Anne", 2), ("Ben", 1)],
        ],
        ["highgate-twocountvsonecount"] =
        [
            [("Ben", 1), ("Anne", 1), ("Ben", 0), ("Anne", 1)],
            [("Anne", 1), ("Anne", 2)],
        ],
    };

    private static readonly Dictionary<string, decimal[]> ExpectedTimes = new()
    {
        ["highgate-fourcountvsthreecount:Anne"] = [0m, 0.75m, 1.5m, 2.25m, 3m, 3.75m, 4.5m, 5.25m],
        ["highgate-fourcountvsthreecount:Ben"] = [0m, 1m, 2m, 3m, 4m, 5m],
        ["highgate-threecountvstwocount:Anne"] =
        [
            0m,
            0.666667m,
            1.333333m,
            2m,
            2.666667m,
            3.333333m,
        ],
        ["highgate-threecountvstwocount:Ben"] = [0m, 1m, 2m, 3m],
        ["highgate-twocountvsonecount:Anne"] = [0m, 0.5m, 1m, 1.5m],
        ["highgate-twocountvsonecount:Ben"] = [0m, 1m],
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Test]
    public void SlowFastRecordsMatchPrintedRowsAndAreRegisteredAsValidPresets()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-transcriptions-78-79.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();

        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-78-79.json"
                )
            ),
            JsonOptions
        )!;
        records.Select(record => record.Id).Should().OnlyHaveUniqueItems();

        foreach (var record in records)
        {
            var laneNames = record.Jugglers.ToDictionary(item => item.Id, item => item.Name);
            var edgesBySource = record.CausalEdges.ToDictionary(edge => edge.From);
            record.CausalEdges.Should().HaveCount(record.Throws.Count, record.Id);
            foreach (var lane in record.Jugglers)
            {
                var laneThrows = record
                    .Throws.Where(item => item.JugglerId == lane.Id)
                    .OrderBy(item => item.StartTime)
                    .ToList();
                laneThrows
                    .Select(item => item.StartTime)
                    .Should()
                    .Equal(ExpectedTimes[$"{record.Id}:{lane.Name}"]);

                foreach (var (index, source) in laneThrows.Select((item, i) => (i, item)))
                {
                    var (destination, laneIndexOffset) = ExpectedRows[record.Id][
                        lane.Id == "A" ? 0 : 1
                    ][index];
                    var targetIndex = index + laneIndexOffset;
                    var targetThrows = record
                        .Throws.Where(item => laneNames[item.JugglerId] == destination)
                        .OrderBy(item => item.StartTime)
                        .ToList();
                    var expectedTarget = targetThrows[targetIndex % targetThrows.Count];
                    var expectedCycleOffset = targetIndex / targetThrows.Count;
                    var edge = edgesBySource[source.Id];

                    edge.To.Should().Be(expectedTarget.Id, record.Id);
                    edge.CycleOffset.Should().Be(expectedCycleOffset, record.Id);
                }
            }

            var state = ToState(record);
            var offsets = state.CausalEdges.Select(edge => edge.CycleOffset).ToArray();
            state.SynchronizeHeights();
            state.CausalEdges.Select(edge => edge.CycleOffset).Should().Equal(offsets, record.Id);
            state.IsValidForEditor().Should().BeTrue(record.Id);

            var imported = CausalDiagramEditorPresets
                .All.Single(preset => preset.Id == record.Id)
                .Create();
            imported.IsValidForEditor().Should().BeTrue(record.Id);
            imported.CycleLength.Should().Be(record.CycleLength, record.Id);
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

        records.Should().HaveCount(3);
    }

    private static CausalDiagramEditorState ToState(Record record) =>
        new()
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
                    (item, index) =>
                        new EditorCausalEdge($"check-{index}", item.From, item.To, item.CycleOffset)
                )
                .ToList(),
        };

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
