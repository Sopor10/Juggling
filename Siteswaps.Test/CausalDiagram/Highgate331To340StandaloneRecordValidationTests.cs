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
    public void EveryStandaloneRecordHasVerifiedSourcesAndCreatesAValidEditorState()
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
            .Equal("highgate-thirteenclubdresserdrawerweave", "highgate-thirteenclubgorillaweave");

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
            record
                .Throws.Select(item => (item.Id, item.StartTime))
                .Should()
                .BeEquivalentTo(
                    baseRecord.Throws.Select(item =>
                        (
                            item.Id,
                            item.StartTime
                                + (
                                    record.Id == "highgate-thirteenclubgorillaweave"
                                        && item.JugglerId != "P2"
                                        ? 0.5m
                                        : 0m
                                )
                        )
                    ),
                    record.Id
                );
            var expectedEdges = baseRecord.CausalEdges
                .Select(edge =>
                    record.Id == "highgate-thirteenclubgorillaweave"
                        ? edge with
                        {
                            To = edge.From switch
                            {
                                "P2-1" => "P1-2",
                                "P2-3" => "P1-4",
                                "P2-9" => "P3-10",
                                _ => edge.To,
                            },
                        }
                        : edge
                )
                .ToList();
            record.CausalEdges.Should().BeEquivalentTo(expectedEdges, record.Id);
            AssertSourceAnchors(record);
            record.Throws.Select(item => item.Id).Should().OnlyHaveUniqueItems(record.Id);
            record.CausalEdges.Should().HaveCount(record.Throws.Count, record.Id);
            record
                .CausalEdges.GroupBy(edge => edge.From)
                .Should()
                .OnlyContain(
                    group => group.Count() == 1,
                    $"each throw needs one outgoing edge in {record.Id}"
                );
            record
                .CausalEdges.Select(edge => edge.From)
                .Should()
                .BeEquivalentTo(record.Throws.Select(item => item.Id), record.Id);

            var throwIds = record.Throws.Select(item => item.Id).ToHashSet();
            record
                .CausalEdges.SelectMany(edge => new[] { edge.From, edge.To })
                .Should()
                .OnlyContain(id => throwIds.Contains(id), record.Id);

            var state = CreateEditorState(record);
            var offsets = state.CausalEdges.Select(edge => edge.CycleOffset).ToArray();
            state.SynchronizeHeights();
            state.CausalEdges.Select(edge => edge.CycleOffset).Should().Equal(offsets, record.Id);
            state.IsValidForEditor().Should().BeTrue(record.Id);
        }
    }

    private static void AssertSourceAnchors(Record record)
    {
        var sourceLinks = record.SourceLinks.EnumerateArray().ToArray();
        sourceLinks.Should().HaveCount(2, record.Id);
        sourceLinks
            .Select(link => link.GetProperty("edition").GetString())
            .Should()
            .Contain("2017 expanded PDF")
            .And.Contain("2014 PDF");
        var expectedPages = record.Id switch
        {
            "highgate-thirteenclubdresserdrawerweave" => (
                printed2017: 327,
                pdf2017: 328,
                printed2014: 235
            ),
            "highgate-thirteenclubgorillaweave" => (
                printed2017: 329,
                pdf2017: 330,
                printed2014: 237
            ),
            _ => throw new AssertionException($"Unexpected source record {record.Id}"),
        };
        sourceLinks
            .Single(link => link.GetProperty("edition").GetString() == "2017 expanded PDF")
            .GetProperty("printedPage")
            .GetInt32()
            .Should()
            .Be(expectedPages.printed2017, record.Id);
        sourceLinks
            .Single(link => link.GetProperty("edition").GetString() == "2017 expanded PDF")
            .GetProperty("pdfPage")
            .GetInt32()
            .Should()
            .Be(expectedPages.pdf2017, record.Id);
        sourceLinks
            .Single(link => link.GetProperty("edition").GetString() == "2014 PDF")
            .GetProperty("printedPage")
            .GetInt32()
            .Should()
            .Be(expectedPages.printed2014, record.Id);
    }

    private static CausalDiagramEditorState CreateEditorState(Record record) =>
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
                    item.Height ?? 0m,
                    item.HeightIsExplicit
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
