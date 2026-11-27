using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate271To285StandaloneRecordValidationTests
{
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
                    "docs/research/highgate-transcriptions-271-285.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();

        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-271-285.json"
                )
            ),
            JsonOptions
        )!;
        records.Should().HaveCount(10);

        records
            .Select(record => record.Id)
            .Should()
            .BeEquivalentTo(
                new[]
                {
                    "highgate-londonsburning",
                    "highgate-rotator",
                    "highgate-pistons",
                    "highgate-flyingtrapeze",
                    "highgate-benzenering",
                    "highgate-dosado",
                    "highgate-threeleafclover",
                    "highgate-stickytriangle",
                    "highgate-theweave",
                    "highgate-dresserdrawerweave",
                }
            );

        foreach (var record in records)
        {
            record.Jugglers.Select(item => item.Id).Should().OnlyHaveUniqueItems(record.Id);
            record.Throws.Select(item => item.Id).Should().OnlyHaveUniqueItems(record.Id);
            var jugglerIds = record.Jugglers.Select(item => item.Id).ToHashSet();
            record
                .Throws.Should()
                .OnlyContain(item => jugglerIds.Contains(item.JugglerId), record.Id);
            record.CausalEdges.Should().HaveCount(record.Throws.Count, record.Id);
            var throwIds = record.Throws.Select(item => item.Id).ToHashSet();
            record.CausalEdges.Select(edge => edge.From).Should().OnlyHaveUniqueItems(record.Id);
            record
                .CausalEdges.Select(edge => edge.From)
                .Should()
                .BeEquivalentTo(throwIds, record.Id);
            record
                .CausalEdges.SelectMany(edge => new[] { edge.From, edge.To })
                .Should()
                .OnlyContain(id => throwIds.Contains(id), record.Id);
            var throwsById = record.Throws.ToDictionary(item => item.Id);
            foreach (var edge in record.CausalEdges)
            {
                var source = throwsById[edge.From];
                var target = throwsById[edge.To];
                if (source.HeightIsExplicit)
                {
                    (
                        target.StartTime
                        + edge.CycleOffset * record.CycleLength
                        + 2
                        - source.StartTime
                    )
                        .Should()
                        .Be(source.Height, record.Id);
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
                        item.StartTime
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
        decimal? Height,
        bool HeightIsExplicit
    );

    private sealed record Edge(string From, string To, int CycleOffset);
}
