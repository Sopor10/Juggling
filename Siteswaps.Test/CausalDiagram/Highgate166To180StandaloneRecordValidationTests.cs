using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate166To180StandaloneRecordValidationTests
{
    private static readonly Dictionary<string, string[][]> DestinationRows = new()
    {
        ["highgate-clockwiseonecountrunaround"] =
        [
            "Ben Ben self Ben Ben self Clare Clare Ben Ben Ben self Ben Ben self Clare Clare Ben".Split(
                ' '
            ),
            "Anne Anne Clare Anne Anne self self self Anne Anne Anne Clare Anne Anne self self self Anne".Split(
                ' '
            ),
            "self self Ben self self self Anne Anne self self self Ben self self self Anne Anne self".Split(
                ' '
            ),
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
                    "docs/research/highgate-transcriptions-166-180.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();

        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-166-180.json"
                )
            ),
            JsonOptions
        )!;
        records.Should().HaveCount(9);
        records.Select(record => record.Id).Should().OnlyHaveUniqueItems();

        foreach (var record in records)
        {
            record.CausalEdges.Should().HaveCount(record.Throws.Count, record.Id);
            record.Throws.Select(item => item.Id).Should().OnlyHaveUniqueItems(record.Id);
            record.CausalEdges.Select(edge => edge.From).Should().OnlyHaveUniqueItems(record.Id);
            var throwIds = record.Throws.Select(item => item.Id).ToHashSet();
            foreach (var edge in record.CausalEdges)
            {
                throwIds.Should().Contain(edge.From, record.Id);
                throwIds.Should().Contain(edge.To, record.Id);
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
                        item.Height ?? 0m,
                        item.HeightIsExplicit
                    ))
                    .ToList(),
                CausalEdges = record
                    .CausalEdges.Select(
                        (item, index) =>
                            new EditorCausalEdge(
                                $"check-{index}",
                                item.From,
                                item.To,
                                item.CycleOffset
                            )
                    )
                    .ToList(),
            };
            var offsets = state.CausalEdges.Select(edge => edge.CycleOffset).ToArray();

            state.SynchronizeHeights();

            state.CausalEdges.Select(edge => edge.CycleOffset).Should().Equal(offsets, record.Id);
            state.IsValidForEditor().Should().BeTrue(record.Id);

            if (DestinationRows.TryGetValue(record.Id, out var rows))
            {
                state
                    .CausalEdges.Select(edge => edge.ToThrowId)
                    .Should()
                    .OnlyHaveUniqueItems(record.Id);
                var jugglerIds = record.Jugglers.ToDictionary(item => item.Name, item => item.Id);
                foreach (var (jugglerIndex, row) in rows.Select((row, index) => (index, row)))
                {
                    var sourceJuggler = record.Jugglers[jugglerIndex].Id;
                    for (var beat = 0; beat < record.CycleLength; beat++)
                    {
                        var sourceThrow = record.Throws.Single(item =>
                            item.JugglerId == sourceJuggler && item.StartTime == beat
                        );
                        var receiver = row[beat] == "self" ? sourceJuggler : jugglerIds[row[beat]];
                        var targetBeat = (beat + 1) % record.CycleLength;
                        var targetThrow = record.Throws.Single(item =>
                            item.JugglerId == receiver && item.StartTime == targetBeat
                        );
                        state
                            .CausalEdges.Single(edge => edge.FromThrowId == sourceThrow.Id)
                            .ToThrowId.Should()
                            .Be(targetThrow.Id, record.Id);
                        state
                            .CausalEdges.Single(edge => edge.FromThrowId == sourceThrow.Id)
                            .CycleOffset.Should()
                            .Be(beat + 1 >= record.CycleLength ? 1 : 0, record.Id);
                    }
                }

                state.Throws.Should().OnlyContain(item => item.Height == 3m, record.Id);
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
        decimal? Height,
        bool HeightIsExplicit
    );

    private sealed record Edge(string From, string To, int CycleOffset);
}
