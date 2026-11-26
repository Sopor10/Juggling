using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public sealed class Highgate31To45StandaloneRecordValidationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Test]
    public void EveryTranscriptionCreatesAValidStateAndMatchesItsCausalArrivals()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(repository.FullName, "docs/research/highgate-transcriptions-31-45.json")
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();

        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(repository!.FullName, "docs/research/highgate-transcriptions-31-45.json")
            ),
            JsonOptions
        )!;

        records.Select(record => record.Id).Should().BeEquivalentTo(
            "highgate-parsnip",
            "highgate-fiveclubwhynot",
            "highgate-fiveclubnotwhy"
        );

        foreach (var record in records)
        {
            var throws = record.Throws.ToDictionary(item => item.Id, StringComparer.Ordinal);
            var jugglers = record.Jugglers.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
            record.Throws.Select(item => item.Id).Should().OnlyHaveUniqueItems(record.Id);
            record.Throws.Should().OnlyContain(
                item =>
                    jugglers.Contains(item.JugglerId)
                    && (item.Hand == "R" || item.Hand == "L"),
                record.Id
            );
            record.Throws.Should().OnlyContain(
                item =>
                    item.HeightIsExplicit
                    && item.Height.HasValue
                    && item.Height.Value >= 0,
                record.Id
            );
            record.CausalEdges.Should().HaveCount(record.Throws.Count, record.Id);
            record.CausalEdges.Select(edge => edge.FromThrowId).Should().OnlyHaveUniqueItems(record.Id);

            foreach (var edge in record.CausalEdges)
            {
                throws.Should().ContainKey(edge.FromThrowId);
                throws.Should().ContainKey(edge.ToThrowId);
                var source = throws[edge.FromThrowId];
                var target = throws[edge.ToThrowId];
                (
                    target.StartTime + edge.CycleOffset * record.CycleLength
                ).Should().Be(
                    source.StartTime + source.Height!.Value,
                    $"{record.Id}: {edge.FromThrowId}"
                );
            }

            var arrivals = record.CausalEdges.Select(edge =>
            {
                var target = throws[edge.ToThrowId];
                return (target.JugglerId, target.Hand, target.StartTime + edge.CycleOffset * record.CycleLength);
            });
            arrivals.Should().OnlyHaveUniqueItems(record.Id);

            var state = new CausalDiagramEditorState
            {
                PresetId = "highgate-three-count",
                CycleLength = record.CycleLength,
                ActiveJugglerId = record.Jugglers[0].Id,
                Jugglers = record.Jugglers.Select(item => new EditorJuggler(item.Id, item.Name)).ToList(),
                Throws = record.Throws.Select(item => new EditorThrow(
                    item.Id,
                    item.JugglerId,
                    item.Hand,
                    item.StartTime,
                    item.Height!.Value,
                    item.HeightIsExplicit
                )).ToList(),
                CausalEdges = record
                .CausalEdges.Select(
                    (edge, index) =>
                        new EditorCausalEdge(
                            $"{record.Id}-e{index + 1}",
                            edge.FromThrowId,
                            edge.ToThrowId,
                            edge.CycleOffset
                        )
                )
                .ToList(),
            };
            var offsets = state.CausalEdges.Select(edge => edge.CycleOffset).ToArray();
            state.SynchronizeHeights();
            state.CausalEdges.Select(edge => edge.CycleOffset).Should().Equal(offsets, record.Id);
            state.IsValidForEditor().Should().BeTrue(record.Id);

            var imported = CausalDiagramEditorPresets.All
                .Single(preset => preset.Id == record.Id)
                .Create();
            imported.CycleLength.Should().Be(state.CycleLength, record.Id);
            imported.Jugglers.Select(item => (item.Id, item.Name))
                .Should()
                .Equal(state.Jugglers.Select(item => (item.Id, item.Name)), record.Id);
            imported.Throws.Select(item =>
                    (item.Id, item.JugglerId, item.Hand, item.StartTime, item.Height)
                )
                .Should()
                .Equal(
                    state.Throws.Select(item =>
                        (item.Id, item.JugglerId, item.Hand, item.StartTime, item.Height)
                    ),
                    record.Id
                );
            imported.CausalEdges.Select(item =>
                    (item.FromThrowId, item.ToThrowId, item.CycleOffset)
                )
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

    private sealed record Edge(string FromThrowId, string ToThrowId, int CycleOffset);
}
