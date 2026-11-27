using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate241To255StandaloneRecordValidationTests
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
                    "docs/research/highgate-transcriptions-241-255.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();
        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-241-255.json"
                )
            ),
            JsonOptions
        )!;
        records.Should().HaveCount(23);
        records.Select(record => record.Id).Should().OnlyHaveUniqueItems();

        foreach (var record in records)
        {
            record.Throws.Select(item => item.Id).Should().OnlyHaveUniqueItems(record.Id);
            var throwIds = record.Throws.Select(item => item.Id).ToHashSet();
            var outgoingThrowIds = new HashSet<string>();
            foreach (var edge in record.CausalEdges)
            {
                throwIds.Should().Contain(edge.FromId, record.Id);
                throwIds.Should().Contain(edge.ToIdValue, record.Id);
                outgoingThrowIds.Add(edge.FromId).Should().BeTrue(record.Id);
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
                        (item, i) =>
                            new EditorCausalEdge(
                                $"check-{i}",
                                item.FromId,
                                item.ToIdValue,
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

    [TestCase("highgate-parsniplinefeed")]
    [TestCase("highgate-funkybookendslinefeed")]
    public void CorrectedLineFeedGraphsAreBalancedAndImportedIntoTheProductionPreset(string id)
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-transcriptions-241-255.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();

        var source = JsonSerializer
            .Deserialize<List<Record>>(
                File.ReadAllText(
                    Path.Combine(
                        repository!.FullName,
                        "docs/research/highgate-transcriptions-241-255.json"
                    )
                ),
                JsonOptions
            )!
            .Single(record => record.Id == id);
        source.SourceRows.Should().NotBeNull(id);
        source.SourcePhases.Should().NotBeNull(id);
        source.PassDirectionByJuggler.Should().NotBeNull(id);

        var degrees = source.Throws.ToDictionary(
            item => item.Id,
            item => (Incoming: 0, Outgoing: 0)
        );
        foreach (var edge in source.CausalEdges)
        {
            degrees[edge.FromId] = degrees[edge.FromId] with
            {
                Outgoing = degrees[edge.FromId].Outgoing + 1,
            };
            degrees[edge.ToIdValue] = degrees[edge.ToIdValue] with
            {
                Incoming = degrees[edge.ToIdValue].Incoming + 1,
            };
        }
        degrees.Should().OnlyContain(pair => pair.Value.Incoming == pair.Value.Outgoing, id);

        var preset = CausalDiagramEditorPresets.AllForLoading.Single(item => item.Id == id);
        var production = preset.Create();
        production.IsValidPattern().Should().BeTrue(id);
        production
            .Throws.Select(item => item.Id)
            .Should()
            .BeEquivalentTo(source.Throws.Select(item => item.Id), id);
        production
            .CausalEdges.Select(edge => (edge.FromThrowId, edge.ToThrowId, edge.CycleOffset))
            .Should()
            .BeEquivalentTo(
                source.CausalEdges.Select(edge => (edge.FromId, edge.ToIdValue, edge.CycleOffset)),
                id
            );
    }

    private sealed record Record(
        string Id,
        decimal CycleLength,
        List<Juggler> Jugglers,
        List<Throw> Throws,
        List<Edge> CausalEdges,
        Dictionary<string, string[]>? SourceRows = null,
        Dictionary<string, decimal>? SourcePhases = null,
        Dictionary<string, string>? PassDirectionByJuggler = null
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

    private sealed class Edge
    {
        public string? From { get; init; }
        public string? To { get; init; }
        public string? FromThrowId { get; init; }
        public string? ToThrowId { get; init; }
        public int CycleOffset { get; init; }

        public string FromId => FromThrowId ?? From!;
        public string ToIdValue => ToThrowId ?? To!;
    }
}
