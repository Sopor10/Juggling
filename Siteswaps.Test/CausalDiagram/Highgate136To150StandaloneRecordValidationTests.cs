using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate136To150StandaloneRecordValidationTests
{
    private static readonly Dictionary<string, Dictionary<string, string[]>> CompositeFeastRows =
        new()
        {
            ["highgate-trifle"] = new()
            {
                ["Anne"] = "Ben Ben self self Clare self self self self Clare self self".Split(' '),
                ["Ben"] = "Anne Anne self self self Clare self self Clare self self self".Split(' '),
                ["Clare"] = "self self self self Anne Ben self self Ben Anne self self".Split(' '),
            },
            ["highgate-trilogy"] = new()
            {
                ["Anne"] = "Ben self self self self Clare self Clare self self self self Ben self self".Split(' '),
                ["Ben"] = "Anne self Clare self self self self self self self Clare self Anne self self".Split(' '),
                ["Clare"] = "self self Ben self self Anne self Anne self self Ben self self self self".Split(' '),
            },
        };

    private static readonly Dictionary<string, string[][]> PulsarLanes = new()
    {
        ["highgate-pulsar"] =
        [
            "self self Ben self self self Clare self Ben self Clare self Ben self self self Clare self self self Clare self Ben self".Split(
                ' '
            ),
            "Clare self Anne self Clare self self self Anne self self self Anne self Clare self self self Clare self self self Anne self".Split(
                ' '
            ),
            "Ben self self self Ben self Anne self self self Anne self self self Ben self Anne self Ben self Anne self self self".Split(
                ' '
            ),
        ],
        ["highgate-ambidextrouspulsarthreecountfeed"] =
        [
            "self self self Ben self self self self self Clare self self Ben self self Clare self self Ben self self self self self Clare self self self self self Clare self self Ben self self".Split(
                ' '
            ),
            "Clare self self Anne self self Clare self self self self self Anne self self self self self Anne self self Clare self self self self self Clare self self self self self Anne self self".Split(
                ' '
            ),
            "Ben self self self self self Ben self self Anne self self self self self Anne self self self self self Ben self self Anne self self Ben self self Anne self self self self self".Split(
                ' '
            ),
        ],
        ["highgate-ambidextrouspulsarpasspassselffeed"] =
        [
            "self Ben self self Clare self Ben Clare self Ben self self Clare self self Clare Ben self".Split(
                ' '
            ),
            "Clare Anne self Clare self self Anne self self Anne Clare self self Clare self self Anne self".Split(
                ' '
            ),
            "Ben self self Ben Anne self self Anne self self Ben self Anne Ben self Anne self self".Split(
                ' '
            ),
        ],
        ["highgate-ambidextrouspulsarpassselfpassfeed"] =
        [
            "self self Ben self self Clare Ben self Clare Ben self self Clare self self Clare self Ben".Split(
                ' '
            ),
            "Clare self Anne Clare self self Anne self self Anne self Clare self self Clare self self Anne".Split(
                ' '
            ),
            "Ben self self Ben self Anne self self Anne self self Ben Anne self Ben Anne self self".Split(
                ' '
            ),
        ],
        ["highgate-ambidextrouspulsaronecountfeed"] =
        [
            "self Ben self Clare Ben Clare Ben self Clare self Clare Ben".Split(' '),
            "Clare Anne Clare self Anne self Anne Clare self Clare self Anne".Split(' '),
            "Ben self Ben Anne self Anne self Ben Anne Ben Anne self".Split(' '),
        ],
        ["highgate-quasar"] =
        [
            "Ben self Clare self self Clare self self self Ben self self Ben self Clare".Split(' '),
            "Anne self self self Clare self self Clare self Anne Clare self Anne self self".Split(
                ' '
            ),
            "self self Anne self Ben Anne self Ben self self Ben self self self Anne".Split(' '),
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
                    "docs/research/highgate-transcriptions-136-150.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();
        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-136-150.json"
                )
            ),
            JsonOptions
        )!;

        foreach (var record in records)
        {
            record.CausalEdges.Should().HaveCount(record.Throws.Count, record.Id);
            var throwsById = record.Throws.ToDictionary(item => item.Id);
            var endpoints = record.Throws.Select(item => item.Id).ToHashSet();
            foreach (var edge in record.CausalEdges)
            {
                endpoints.Should().Contain(edge.From, record.Id);
                endpoints.Should().Contain(edge.To, record.Id);
                var source = throwsById[edge.From];
                var target = throwsById[edge.To];
                if (record.Id.Contains("pulsar", StringComparison.Ordinal))
                {
                    (target.StartTime + edge.CycleOffset * record.CycleLength + 2)
                        .Should()
                        .Be(source.StartTime + source.Height, record.Id);
                }
            }

            if (PulsarLanes.TryGetValue(record.Id, out var lanes))
            {
                var jugglersByName = record.Jugglers.ToDictionary(
                    item => item.Name,
                    item => item.Id
                );
                foreach (var (jugglerIndex, lane) in lanes.Select((lane, index) => (index, lane)))
                {
                    var sourceJuggler = record.Jugglers[jugglerIndex].Id;
                    for (var beat = 0; beat < record.CycleLength; beat++)
                    {
                        var sourceThrow = record.Throws.Single(item =>
                            item.JugglerId == sourceJuggler && item.StartTime == beat
                        );
                        var recipientName = lane[beat % lane.Length];
                        var recipient =
                            recipientName == "self" ? sourceJuggler : jugglersByName[recipientName];
                        var nextBeat = (beat + 1) % record.CycleLength;
                        var expectedTarget = record.Throws.Single(item =>
                            item.JugglerId == recipient && item.StartTime == nextBeat
                        );
                        var edge = record.CausalEdges.Single(item => item.From == sourceThrow.Id);
                        edge.To.Should().Be(expectedTarget.Id, record.Id);
                        edge.CycleOffset.Should()
                            .Be(beat + 1 >= record.CycleLength ? 1 : 0, record.Id);
                    }
                }
            }

            if (CompositeFeastRows.TryGetValue(record.Id, out var feastRows))
            {
                record.SourceRows.Should().BeEquivalentTo(feastRows, record.Id);
                ValidateCompositeFeastRows(record, feastRows);

                var production = CausalDiagramEditorPresets.AllForLoading.Single(preset => preset.Id == record.Id).Create();
                production.IsValidPattern().Should().BeTrue(record.Id);
                production
                    .CausalEdges.Select(edge => (edge.FromThrowId, edge.ToThrowId, edge.CycleOffset))
                    .Should()
                    .BeEquivalentTo(
                        record.CausalEdges.Select(edge => (edge.From, edge.To, edge.CycleOffset)),
                        record.Id
                    );
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
                        item.Height,
                        item.HeightIsExplicit
                    ))
                    .ToList(),
                CausalEdges = record
                    .CausalEdges.Select(
                        (item, i) =>
                            new EditorCausalEdge($"check-{i}", item.From, item.To, item.CycleOffset)
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

    private static void ValidateCompositeFeastRows(
        Record record,
        Dictionary<string, string[]> sourceRows
    )
    {
        var jugglerIds = record.Jugglers.ToDictionary(item => item.Name, item => item.Id);
        var throwsByJugglerAndTime = record.Throws.ToDictionary(
            item => (item.JugglerId, item.StartTime)
        );
        var edgesBySource = record.CausalEdges.ToDictionary(item => item.From);

        foreach (var (jugglerName, row) in sourceRows)
        {
            var jugglerId = jugglerIds[jugglerName];
            foreach (var source in record.Throws.Where(item => item.JugglerId == jugglerId))
            {
                var action = row[(int)source.StartTime % row.Length];
                var targetJuggler = action == "self" ? jugglerId : jugglerIds[action];
                var targetTime = source.StartTime + 1;
                var expectedTarget = throwsByJugglerAndTime[
                    (targetJuggler, targetTime % record.CycleLength)
                ];
                var expectedOffset = (int)(targetTime / record.CycleLength);
                var edge = edgesBySource[source.Id];
                edge.To.Should().Be(expectedTarget.Id, source.Id);
                edge.CycleOffset.Should().Be(expectedOffset, source.Id);
            }
        }
    }

    private sealed record Record(
        string Id,
        decimal CycleLength,
        List<Juggler> Jugglers,
        List<Throw> Throws,
        List<Edge> CausalEdges,
        Dictionary<string, string[]>? SourceRows = null
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

    private sealed record Edge(string From, string To, int CycleOffset);
}
