using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public sealed class Highgate61To75StandaloneRecordValidationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Test]
    public void EveryNewTranscriptionCreatesAValidImportedState()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-transcriptions-61-75.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();

        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-61-75.json"
                )
            ),
            JsonOptions
        )!;
        var newIds = new HashSet<string>(
            [
                "highgate-asailorwenttosea",
                "highgate-countdown",
                "highgate-geometriccountdown",
                "highgate-fizzbuzz",
                "highgate-jimsfizzbuzzsynchronousversion",
            ],
            StringComparer.Ordinal
        );
        var selected = records
            .Where(record => newIds.Contains(record.Id))
            .ToDictionary(record => record.Id, StringComparer.Ordinal);
        selected.Keys.Should().BeEquivalentTo(newIds);

        foreach (var record in selected.Values)
        {
            var throwById = record.Throws.ToDictionary(item => item.Id, StringComparer.Ordinal);
            record.VisualVerification.ValueKind.Should().Be(JsonValueKind.String, record.Id);
            record.Throws.Select(item => item.Id).Should().OnlyHaveUniqueItems(record.Id);
            record.CausalEdges.Should().HaveCount(record.Throws.Count, record.Id);
            record.CausalEdges.Select(edge => edge.From).Should().OnlyHaveUniqueItems(record.Id);
            record.CausalEdges.Select(edge => edge.To).Should().OnlyHaveUniqueItems(record.Id);
            record
                .CausalEdges.Should()
                .OnlyContain(
                    edge => throwById.ContainsKey(edge.From) && throwById.ContainsKey(edge.To),
                    record.Id
                );
            record
                .SourceLinks.Select(link => link.Edition)
                .Should()
                .Contain(edition => edition.Contains("2017", StringComparison.Ordinal))
                .And.Contain(edition => edition.Contains("2014", StringComparison.Ordinal));

            var state = new CausalDiagramEditorState
            {
                PresetId = "highgate-three-count",
                CycleLength = record.CycleLength,
                ActiveJugglerId = record.Jugglers[0].Id,
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
                        (edge, index) =>
                            new EditorCausalEdge(
                                $"{record.Id}-e{index + 1}",
                                edge.From,
                                edge.To,
                                edge.CycleOffset
                            )
                    )
                    .ToList(),
            };
            var offsets = state.CausalEdges.Select(edge => edge.CycleOffset).ToArray();
            state.SynchronizeHeights();
            state.CausalEdges.Select(edge => edge.CycleOffset).Should().Equal(offsets, record.Id);
            state.IsValidForEditor().Should().BeTrue(record.Id);

            var imported = CausalDiagramEditorPresets
                .AllForLoading.Single(preset => preset.Id == record.Id)
                .Create();
            imported.CycleLength.Should().Be(state.CycleLength, record.Id);
            imported
                .Jugglers.Select(item => (item.Id, item.Name))
                .Should()
                .Equal(state.Jugglers.Select(item => (item.Id, item.Name)), record.Id);
            imported
                .Throws.Select(item =>
                    (item.Id, item.JugglerId, item.Hand, item.StartTime, item.Height)
                )
                .Should()
                .Equal(
                    state.Throws.Select(item =>
                        (item.Id, item.JugglerId, item.Hand, item.StartTime, item.Height)
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

        selected.Should().HaveCount(5);
    }

    private sealed record Record(
        string Id,
        decimal CycleLength,
        List<Juggler> Jugglers,
        List<Throw> Throws,
        List<Edge> CausalEdges,
        List<SourceLink> SourceLinks,
        JsonElement VisualVerification
    );

    private sealed record Juggler(string Id, string Name);

    private sealed record Throw(
        string Id,
        string JugglerId,
        string Hand,
        decimal StartTime,
        decimal? Height = null,
        bool HeightIsExplicit = false
    );

    private sealed record Edge(string From, string To, int CycleOffset);

    private sealed record SourceLink(string Edition);
}
