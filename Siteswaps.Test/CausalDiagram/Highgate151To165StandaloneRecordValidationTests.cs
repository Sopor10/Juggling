using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate151To165StandaloneRecordValidationTests
{
    private static readonly Dictionary<string, string[][]> SourceLanes = new()
    {
        ["highgate-twocounttwocountfourcounttriangle"] =
        [
            ["Clare", "self", "self", "self"],
            ["Anne", "self", "Clare", "self"],
            ["Ben", "self", "Ben", "self"],
        ],
        ["highgate-tarimstriangle"] =
        [
            ["Clare", "self", "self"],
            ["Anne", "Clare", "self"],
            ["Ben", "Ben", "self"],
        ],
        ["highgate-extendedtarimstriangle"] =
        [
            [
                "Clare",
                "self",
                "self",
                "Clare",
                "self",
                "self",
                "Clare",
                "Ben",
                "self",
                "Clare",
                "Ben",
                "self",
                "Clare",
                "Clare",
                "self",
                "Clare",
                "Clare",
                "self",
            ],
            [
                "Anne",
                "Clare",
                "self",
                "Anne",
                "Clare",
                "self",
                "Anne",
                "Anne",
                "self",
                "Anne",
                "Anne",
                "self",
                "Anne",
                "self",
                "self",
                "Anne",
                "self",
                "self",
            ],
            [
                "Ben",
                "Ben",
                "self",
                "Ben",
                "Ben",
                "self",
                "Ben",
                "self",
                "self",
                "Ben",
                "self",
                "self",
                "Ben",
                "Anne",
                "self",
                "Ben",
                "Anne",
                "self",
            ],
        ],
        ["highgate-onecountonecountthreecounttriangle"] =
        [
            ["Ben", "Ben", "Ben"],
            ["Clare", "Anne", "Anne"],
            ["Anne", "self", "self"],
        ],
        ["highgate-onecountonecountpasspassselftriangle"] =
        [
            ["Clare", "Clare", "self"],
            ["Anne", "Anne", "Clare"],
            ["Ben", "Ben", "Ben"],
        ],
        ["highgate-passpassselfpasspassselfcountdowntriangle"] =
        [
            [
                "Clare",
                "self",
                "self",
                "self",
                "Clare",
                "self",
                "self",
                "Clare",
                "self",
                "Clare",
                "self",
                "self",
            ],
            [
                "Anne",
                "Clare",
                "self",
                "Clare",
                "Anne",
                "self",
                "Clare",
                "Anne",
                "self",
                "Anne",
                "Clare",
                "self",
            ],
            [
                "Ben",
                "Ben",
                "self",
                "Ben",
                "Ben",
                "self",
                "Ben",
                "Ben",
                "self",
                "Ben",
                "Ben",
                "self",
            ],
        ],
        ["highgate-onecountonecountcountdowntriangle"] =
        [
            ["Clare", "Clare", "self", "Clare", "self", "self", "Clare", "self"],
            ["Anne", "Anne", "Clare", "Anne", "Clare", "Clare", "Anne", "Clare"],
            ["Ben", "Ben", "Ben", "Ben", "Ben", "Ben", "Ben", "Ben"],
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
                    "docs/research/highgate-transcriptions-151-165.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();
        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-151-165.json"
                )
            ),
            JsonOptions
        )!;

        foreach (var record in records)
        {
            record.CausalEdges.Should().HaveCount(record.Throws.Count, record.Id);
            var throwsById = record.Throws.ToDictionary(item => item.Id);
            var throwIds = throwsById.Keys.ToHashSet();
            foreach (var edge in record.CausalEdges)
            {
                throwIds.Should().Contain(edge.From, record.Id);
                throwIds.Should().Contain(edge.To, record.Id);
            }

            var state = new CausalDiagramEditorState
            {
                PresetId = "highgate-fourcounttriangleonins",
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

            if (SourceLanes.TryGetValue(record.Id, out var lanes))
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
                        var receiverName = lane[beat % lane.Length];
                        var receiver =
                            receiverName == "self" ? sourceJuggler : jugglersByName[receiverName];
                        var nextBeat = (beat + 1) % record.CycleLength;
                        var expectedTarget = record.Throws.Single(item =>
                            item.JugglerId == receiver && item.StartTime == nextBeat
                        );
                        var expectedOffset = beat + 1 >= record.CycleLength ? 1 : 0;
                        var edge = record.CausalEdges.Single(item => item.From == sourceThrow.Id);
                        edge.To.Should().Be(expectedTarget.Id, record.Id);
                        edge.CycleOffset.Should().Be(expectedOffset, record.Id);
                    }
                }

                state.Throws.Should().OnlyContain(item => item.Height == 3, record.Id);
            }
        }

        records.Should().HaveCount(13);
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

    private sealed record Edge(string From, string To, int CycleOffset);
}
