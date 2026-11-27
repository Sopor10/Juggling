using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate76To90StandaloneRecordValidationTests
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
                    "docs/research/highgate-transcriptions-76-90.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();
        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-76-90.json"
                )
            ),
            JsonOptions
        )!;

        foreach (var record in records)
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
                        (item, i) =>
                            new EditorCausalEdge($"check-{i}", item.From, item.To, item.CycleOffset)
                    )
                    .ToList(),
            };
            var offsets = state.CausalEdges.Select(edge => edge.CycleOffset).ToArray();

            state.NormalizeCausalEdgeOffsets();

            state.CausalEdges.Select(edge => edge.CycleOffset).Should().Equal(offsets, record.Id);
            state.IsValidForEditor().Should().BeTrue(record.Id);

            var imported = CausalDiagramEditorPresets
                .AllForLoading.Single(preset => preset.Id == record.Id)
                .Create();
            imported.IsValidForEditor().Should().BeTrue(record.Id);
            imported.CycleLength.Should().Be(state.CycleLength, record.Id);
            imported
                .Jugglers.Select(item => (item.Id, item.Name))
                .Should()
                .Equal(state.Jugglers.Select(item => (item.Id, item.Name)), record.Id);
            imported
                .Throws.Select(item =>
                    (item.Id, item.JugglerId, item.Hand, item.StartTime, imported.HeightOf(item.Id))
                )
                .Should()
                .Equal(
                    state.Throws.Select(item =>
                        (
                            item.Id,
                            item.JugglerId,
                            item.Hand,
                            item.StartTime,
                            state.HeightOf(item.Id)
                        )
                    ),
                    record.Id
                );
            imported
                .CausalEdges.Select(item => (item.FromThrowId, item.ToThrowId, item.CycleOffset))
                .Should()
                .Equal(
                    state.CausalEdges.Select(item =>
                        (item.FromThrowId, item.ToThrowId, item.CycleOffset)
                    ),
                    record.Id
                );
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
        decimal Height,
        bool HeightIsExplicit
    );

    private sealed record Edge(string From, string To, int CycleOffset);
}
