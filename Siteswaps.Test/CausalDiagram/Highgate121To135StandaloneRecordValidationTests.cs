using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate121To135StandaloneRecordValidationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Test]
    public void PassPassZipFeedRecordsCreateValidEditorStatesAndPreserveOffsets()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-transcriptions-121-135.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();

        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-121-135.json"
                )
            ),
            JsonOptions
        )!;
        var expectedIds = new HashSet<string>
        {
            "highgate-passpasszipfeedingthreecount",
            "highgate-passpasszipfeedingfrenchthreecount",
        };
        var selected = records.Where(record => expectedIds.Contains(record.Id)).ToArray();

        foreach (var record in selected)
        {
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
                        item.StartTime
                    ))
                    .ToList(),
                CausalEdges = record
                    .CausalEdges.Select(
                        (item, index) =>
                            new EditorCausalEdge(
                                $"{record.Id}-check-{index + 1}",
                                item.From,
                                item.To,
                                item.CycleOffset
                            )
                    )
                    .ToList(),
            };
            var offsets = state.CausalEdges.Select(edge => edge.CycleOffset).ToArray();

            state.NormalizeCausalEdgeOffsets();

            state.CausalEdges.Select(edge => edge.CycleOffset).Should().Equal(offsets, record.Id);
            state.IsValidForEditor().Should().BeTrue(record.Id);
        }

        selected.Select(record => record.Id).Should().BeEquivalentTo(expectedIds);
    }

    [Test]
    public void EveryNewStandaloneFeedRecordCreatesAValidEditorStateAndPreservesOffsets()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-transcriptions-121-135.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();

        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-121-135.json"
                )
            ),
            JsonOptions
        )!;
        var expectedIds = new HashSet<string>
        {
            "highgate-whynotfeedingfiveclubwhynotandfivecountpopcorn",
            "highgate-maybefeedingfiveclubwhynotandfivecountpopcorn",
            "highgate-parsnipfeedingfivecountpopcornandwhynot",
            "highgate-parsnipfeedingmaybeandfivecountpopcorn",
            "highgate-funkybookendsfeedingfiveclubwhynotandwhynot",
            "highgate-funkybookendsfeedingmaybeandfiveclubwhynot",
            "highgate-martinsonecountfeedingwhynot",
        };
        var selected = records.Where(record => expectedIds.Contains(record.Id)).ToArray();
        selected.Select(record => record.Id).Should().BeEquivalentTo(expectedIds);

        foreach (var record in selected)
        {
            record.CausalEdges.Should().HaveCount(record.Throws.Count, record.Id);
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
                        item.StartTime
                    ))
                    .ToList(),
                CausalEdges = record
                    .CausalEdges.Select(
                        (item, index) =>
                            new EditorCausalEdge(
                                $"{record.Id}-check-{index + 1}",
                                item.From,
                                item.To,
                                item.CycleOffset
                            )
                    )
                    .ToList(),
            };
            var offsets = state.CausalEdges.Select(edge => edge.CycleOffset).ToArray();

            state.NormalizeCausalEdgeOffsets();

            state.CausalEdges.Select(edge => edge.CycleOffset).Should().Equal(offsets, record.Id);
            state.IsValidForEditor().Should().BeTrue(record.Id);
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
        decimal Height,
        bool HeightIsExplicit
    );

    private sealed record Edge(string From, string To, int CycleOffset);
}
