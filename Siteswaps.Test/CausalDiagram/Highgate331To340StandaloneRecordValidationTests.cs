using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate331To340StandaloneRecordValidationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Test]
    public void TranscribedRecordsMatchTheirBaseGraphsAndCreateValidEditorStates()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-transcriptions-331-340.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();

        var chunkPath = Path.Combine(
            repository!.FullName,
            "docs/research/highgate-transcriptions-331-340.json"
        );
        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(chunkPath),
            JsonOptions
        )!;
        var baseRecords = new[]
        {
            JsonSerializer.Deserialize<List<Record>>(
                File.ReadAllText(
                    Path.Combine(
                        repository.FullName,
                        "docs/research/highgate-transcriptions-271-285.json"
                    )
                ),
                JsonOptions
            )!,
            JsonSerializer.Deserialize<List<Record>>(
                File.ReadAllText(
                    Path.Combine(
                        repository.FullName,
                        "docs/research/highgate-transcriptions-286-300.json"
                    )
                ),
                JsonOptions
            )!,
        }
            .SelectMany(items => items)
            .ToDictionary(record => record.Id);

        records
            .Select(record => record.Id)
            .Should()
            .BeEquivalentTo(
                new[]
                {
                    "highgate-thirteenclubdresserdrawerweave",
                    "highgate-thirteenclubgorillaweave",
                }
            );

        foreach (var record in records)
        {
            var baseId = record
                .Variants.EnumerateArray()
                .Select(item => item.GetString())
                .Should()
                .ContainSingle()
                .Subject;
            var baseRecord = baseRecords[baseId];
            record.CycleLength.Should().Be(baseRecord.CycleLength, record.Id);
            record.Jugglers.Should().BeEquivalentTo(baseRecord.Jugglers, record.Id);
            record.Throws.Should().BeEquivalentTo(baseRecord.Throws, record.Id);
            record.CausalEdges.Should().BeEquivalentTo(baseRecord.CausalEdges, record.Id);
            record
                .SourceLinks.EnumerateArray()
                .Select(link => link.GetProperty("edition").GetString())
                .Should()
                .Contain("2017 expanded PDF")
                .And.Contain("2014 PDF");
            record.Throws.Select(item => item.Id).Should().OnlyHaveUniqueItems(record.Id);
            record.CausalEdges.Should().HaveCount(record.Throws.Count, record.Id);

            var throwIds = record.Throws.Select(item => item.Id).ToHashSet();
            record
                .CausalEdges.SelectMany(edge => new[] { edge.From, edge.To })
                .Should()
                .OnlyContain(id => throwIds.Contains(id), record.Id);

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
        }
    }

    private sealed record Record(
        string Id,
        decimal CycleLength,
        List<Juggler> Jugglers,
        List<Throw> Throws,
        List<Edge> CausalEdges,
        JsonElement Variants,
        JsonElement SourceLinks
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
