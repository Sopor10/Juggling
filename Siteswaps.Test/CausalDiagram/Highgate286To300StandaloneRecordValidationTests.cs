using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate286To300StandaloneRecordValidationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Test]
    public void EveryStandaloneRecordMatchesCatalogAndCreatesAValidEditorState()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-transcriptions-286-300.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();

        var sourcePath = Path.Combine(
            repository!.FullName,
            "docs/research/highgate-transcriptions-286-300.json"
        );
        var catalogPath = Path.Combine(
            repository.FullName,
            "Siteswaps.Components/CausalDiagram/Data/highgate-causal-patterns.json"
        );
        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(sourcePath),
            JsonOptions
        )!;
        var sourceNodes = JsonNode.Parse(File.ReadAllText(sourcePath))!.AsArray();
        var catalogNodes = JsonNode.Parse(File.ReadAllText(catalogPath))!.AsArray();

        records.Select(record => record.Id).Should().OnlyHaveUniqueItems();
        records
            .Select(record => record.Id)
            .Should()
            .BeEquivalentTo(
                new[]
                {
                    "highgate-rotatingy",
                    "highgate-karamazovshuffle",
                    "highgate-rotatingλ",
                    "highgate-cyclone",
                    "highgate-seattleshuffle",
                    "highgate-itsagoodonecountdownweave",
                    "highgate-gorillaweave",
                    "highgate-thirteenclubtwocountfeed",
                    "highgate-thirteenclubpasspassselffeed",
                    "highgate-thirteenclubonecountfeedingfourcount",
                    "highgate-thirteenclubonecountfeedingthreecount",
                    "highgate-thirteenclubonecountfeedingtwocount",
                    "highgate-thirteenclubonecountfeedingpasspassself",
                    "highgate-thirteenclubonecountfeedingchocolatebar",
                    "highgate-thirteenclubonecountfeedingbookends",
                    "highgate-thirteenclubonecountfeedinginvertedbookends",
                }
            );

        foreach (var node in sourceNodes)
        {
            var id = node!["id"]!.GetValue<string>();
            var catalogNode = catalogNodes.SingleOrDefault(item =>
                item!["id"]!.GetValue<string>() == id
            );
            if (catalogNode is null)
                continue;
            var sourceComparable = (JsonObject)node.DeepClone();
            var catalogComparable = (JsonObject)catalogNode!.DeepClone();
            foreach (
                var key in new[]
                {
                    "stable_id",
                    "notes",
                    "variants",
                    "modelLimits",
                    "validation",
                    "visualVerification",
                }
            )
            {
                sourceComparable.Remove(key);
                catalogComparable.Remove(key);
            }
            JsonNode.DeepEquals(sourceComparable, catalogComparable).Should().BeTrue(id);
        }

        foreach (var record in records)
        {
            record.Throws.Select(item => item.Id).Should().OnlyHaveUniqueItems(record.Id);
            record.CausalEdges.Should().HaveCount(record.Throws.Count, record.Id);
            var throwIds = record.Throws.Select(item => item.Id).ToHashSet();
            record.CausalEdges.Select(edge => edge.From).Should().OnlyHaveUniqueItems(record.Id);
            record.CausalEdges.Select(edge => edge.From).Should().BeEquivalentTo(throwIds);
            record
                .CausalEdges.SelectMany(edge => new[] { edge.From, edge.To })
                .Should()
                .OnlyContain(id => throwIds.Contains(id), record.Id);

            if (record.Id == "highgate-gorillaweave")
            {
                var outgoing = record.CausalEdges.ToDictionary(edge => edge.From);
                outgoing["P2-1"].To.Should().Be("P1-2");
                outgoing["P2-3"].To.Should().Be("P1-4");
                outgoing["P2-9"].To.Should().Be("P3-10");
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

            var synchronizedThrows = state.Throws.ToDictionary(item => item.Id);
            foreach (var edge in state.CausalEdges)
            {
                var source = synchronizedThrows[edge.FromThrowId];
                var target = synchronizedThrows[edge.ToThrowId];
                (target.StartTime + edge.CycleOffset * state.CycleLength + 2 - source.StartTime)
                    .Should()
                    .Be(source.Height, record.Id);
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
