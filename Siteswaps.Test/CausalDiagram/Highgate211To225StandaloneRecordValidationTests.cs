using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate211To225StandaloneRecordValidationTests
{
    private static readonly Dictionary<string, string[][]> SourceRows = new()
    {
        ["highgate-tenclubpasspasshefftriangle"] =
        [
            ["Ben", "Ben", "heff"],
            ["Clare", "heff", "Clare"],
            ["heff", "Anne", "Anne"],
        ],
        ["highgate-tenclubfrenchthreecounttriangle"] =
        [
            ["Ben", "self", "heff"],
            ["heff", "Clare", "self"],
            ["self", "heff", "Anne"],
        ],
    };

    private static readonly Dictionary<string, string[][]> OneCountFeedRows = new()
    {
        ["highgate-tenclubonecountvariation1"] =
        [
            ["Clare", "Ben"],
            ["Anne", "Clare"],
            ["Ben", "Anne"],
        ],
        ["highgate-tenclubonecountvariation2"] =
        [
            ["Clare", "Clare", "Ben", "Ben"],
            ["Anne", "Anne", "Clare", "Clare"],
            ["Ben", "Ben", "Anne", "Anne"],
        ],
    };

    private static readonly Dictionary<
        string,
        (string Destination, int LaneIndexOffset)[][]
    > SourceEndpointRows = new()
    {
        ["highgate-tenclubtarimstriangle"] =
        [
            [("Ben", 2), ("Anne", 1), ("Ben", 2)],
            [("Anne", 1), ("Clare", 0), ("Ben", 1)],
            [("Clare", 1), ("Anne", 2), ("Clare", 1)],
        ],
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Test]
    public void EveryStandaloneRecordCreatesAValidEditorStateAndPreservesOffsets()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-transcriptions-211-225.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();
        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-211-225.json"
                )
            ),
            JsonOptions
        )!;
        records.Select(record => record.Id).Should().OnlyHaveUniqueItems();

        foreach (var record in records)
        {
            record.CausalEdges.Should().HaveCount(record.Throws.Count, record.Id);
            record.Throws.Select(item => item.Id).Should().OnlyHaveUniqueItems(record.Id);
            record.CausalEdges.Select(edge => edge.FromId).Should().OnlyHaveUniqueItems(record.Id);
            var throwIds = record.Throws.Select(item => item.Id).ToHashSet();
            foreach (var edge in record.CausalEdges)
            {
                throwIds.Should().Contain(edge.FromId, record.Id);
                throwIds.Should().Contain(edge.ToIdValue, record.Id);
            }

            ValidateHeightDerivedRows(record);
            ValidateOneCountFeedRows(record);
            ValidateTarimEndpoints(record);

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
                        item.Height,
                        item.HeightIsExplicit
                    ))
                    .ToList(),
                CausalEdges = record
                    .CausalEdges.Select(
                        (item, i) =>
                            new EditorCausalEdge(
                                $"check-{i}",
                                item.FromId,
                                item.ToIdValue,
                                item.CycleOffset
                            )
                    )
                    .ToList(),
            };
            var offsets = state.CausalEdges.Select(edge => edge.CycleOffset).ToArray();
            state.SynchronizeHeights();
            state.CausalEdges.Select(edge => edge.CycleOffset).Should().Equal(offsets, record.Id);
            state.IsValidForEditor().Should().BeTrue(record.Id);
        }
        records.Should().HaveCount(12);
    }

    private static void ValidateHeightDerivedRows(Record record)
    {
        if (SourceRows.TryGetValue(record.Id, out var sourceRows))
        {
            var jugglerIds = record.Jugglers.ToDictionary(item => item.Name, item => item.Id);
            foreach (
                var (jugglerIndex, sourceRow) in sourceRows.Select((row, index) => (index, row))
            )
            {
                var sourceJuggler = record.Jugglers[jugglerIndex].Id;
                foreach (var source in record.Throws.Where(item => item.JugglerId == sourceJuggler))
                {
                    var action = sourceRow[(int)source.StartTime % sourceRow.Length];
                    var height = action == "heff" ? 4m : 3m;
                    var landingTime = source.StartTime + height - 2m;
                    var cycleOffset = (int)Math.Floor(landingTime / record.CycleLength);
                    var targetJuggler = action is "self" or "heff"
                        ? sourceJuggler
                        : jugglerIds[action];
                    var targetTime = landingTime - cycleOffset * record.CycleLength;
                    var expectedTarget = record.Throws.Single(item =>
                        item.JugglerId == targetJuggler && item.StartTime == targetTime
                    );
                    var edge = record.CausalEdges.Single(item => item.FromId == source.Id);

                    edge.ToIdValue.Should().Be(expectedTarget.Id, record.Id);
                    edge.CycleOffset.Should().Be(cycleOffset, record.Id);
                }
            }
        }
    }

    private static void ValidateOneCountFeedRows(Record record)
    {
        if (OneCountFeedRows.TryGetValue(record.Id, out var feedRows))
        {
            var jugglerIds = record.Jugglers.ToDictionary(item => item.Name, item => item.Id);
            var phases = new Dictionary<string, decimal>
            {
                ["Anne"] = 0.5m,
                ["Ben"] = 0m,
                ["Clare"] = 0.5m,
            };
            foreach (var (jugglerIndex, sourceRow) in feedRows.Select((row, index) => (index, row)))
            {
                var sourceJuggler = record.Jugglers[jugglerIndex];
                var sources = record
                    .Throws.Where(item => item.JugglerId == sourceJuggler.Id)
                    .OrderBy(item => item.StartTime)
                    .ToList();
                sources.Should().HaveCount(sourceRow.Length, record.Id);
                foreach (var (index, source) in sources.Select((item, i) => (i, item)))
                {
                    var destinationName = sourceRow[index];
                    var targetJuggler = jugglerIds[destinationName];
                    var delta = phases[destinationName] == phases[sourceJuggler.Name] ? 2m : 1.5m;
                    var landingTime = source.StartTime + delta;
                    var cycleOffset = (int)Math.Floor(landingTime / record.CycleLength);
                    var targetTime = landingTime - cycleOffset * record.CycleLength;
                    var expectedTarget = record.Throws.Single(item =>
                        item.JugglerId == targetJuggler && item.StartTime == targetTime
                    );
                    var edge = record.CausalEdges.Single(item => item.FromId == source.Id);

                    edge.ToIdValue.Should().Be(expectedTarget.Id, record.Id);
                    edge.CycleOffset.Should().Be(cycleOffset, record.Id);
                    (expectedTarget.StartTime + edge.CycleOffset * record.CycleLength + 2m)
                        .Should()
                        .Be(source.StartTime + delta + 2m, record.Id);
                }
            }
        }
    }

    private static void ValidateTarimEndpoints(Record record)
    {
        if (SourceEndpointRows.TryGetValue(record.Id, out var endpointRows))
        {
            var jugglerIds = record.Jugglers.ToDictionary(item => item.Name, item => item.Id);
            foreach (
                var (jugglerIndex, sourceRow) in endpointRows.Select((row, index) => (index, row))
            )
            {
                var sourceJuggler = record.Jugglers[jugglerIndex];
                var sources = record
                    .Throws.Where(item => item.JugglerId == sourceJuggler.Id)
                    .OrderBy(item => item.StartTime)
                    .ToList();
                sources.Should().HaveCount(6, record.Id);
                foreach (var (index, source) in sources.Select((item, i) => (i, item)))
                {
                    var (destinationName, laneIndexOffset) = sourceRow[index % 3];
                    var targetIndex = index + laneIndexOffset;
                    var expectedTarget = record
                        .Throws.Where(item => item.JugglerId == jugglerIds[destinationName])
                        .OrderBy(item => item.StartTime)
                        .ElementAt(targetIndex % 6);
                    var expectedOffset = targetIndex / 6;
                    var edge = record.CausalEdges.Single(item => item.FromId == source.Id);

                    edge.ToIdValue.Should().Be(expectedTarget.Id, record.Id);
                    edge.CycleOffset.Should().Be(expectedOffset, record.Id);
                    var sourceToLanding =
                        expectedTarget.StartTime
                        + edge.CycleOffset * record.CycleLength
                        - source.StartTime;
                    var expectedPhaseDelta = (sourceJuggler.Name, destinationName) switch
                    {
                        ("Anne", "Ben") => 5m / 3m,
                        ("Anne", "Anne") => 1m,
                        ("Ben", "Anne") => 4m / 3m,
                        ("Ben", "Clare") => 2m / 3m,
                        ("Ben", "Ben") => 1m,
                        ("Clare", "Anne") => 5m / 3m,
                        ("Clare", "Clare") => 1m,
                        _ => throw new AssertionException("Unexpected source action."),
                    };
                    sourceToLanding
                        .Should()
                        .BeApproximately(expectedPhaseDelta, 0.000002m, record.Id);
                }
            }
        }
    }

    private sealed record Record(
        string Id,
        decimal CycleLength,
        List<Juggler> Jugglers,
        List<Throw> Throws,
        List<Edge> CausalEdges
    );

    private sealed record Juggler(string Id, string Name);

    private sealed record Throw(
        string Id,
        string JugglerId,
        string Hand,
        decimal StartTime,
        decimal Height = 0,
        bool HeightIsExplicit = false
    );

    private sealed class Edge
    {
        public string? From { get; init; }
        public string? To { get; init; }
        public string? FromThrowId { get; init; }
        public string? ToThrowId { get; init; }
        public int CycleOffset { get; init; }
        public string FromId => FromThrowId ?? From!;
        public string ToIdValue => ToThrowId ?? To!;
    }
}
