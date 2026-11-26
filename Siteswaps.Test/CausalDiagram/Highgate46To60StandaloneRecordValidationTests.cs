using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public sealed class Highgate46To60StandaloneRecordValidationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Test]
    public void NewTranscriptionsCreateValidStatesAndPreserveTheirSourceEdges()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(repository.FullName, "docs/research/highgate-transcriptions-46-60.json")
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();

        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(repository!.FullName, "docs/research/highgate-transcriptions-46-60.json")
            ),
            JsonOptions
        )!;
        var selected = records
            .Where(record =>
                record.StableId
                is "highgate-jimsthreecountasynchversion"
                    or "highgate-synchronousonecount"
                    or "highgate-synchronousonecountwithheffs"
            )
            .ToDictionary(record => record.StableId);
        selected.Keys.Should().BeEquivalentTo(
            "highgate-jimsthreecountasynchversion",
            "highgate-synchronousonecount",
            "highgate-synchronousonecountwithheffs"
        );

        foreach (var record in selected.Values)
        {
            var throws = record.Throws.ToDictionary(item => item.Id, StringComparer.Ordinal);
            record.Throws.Select(item => item.Id).Should().OnlyHaveUniqueItems(record.StableId);
            record.CausalEdges.Should().NotBeEmpty(record.StableId);
            record.CausalEdges.Select(edge => edge.SourceId).Should().OnlyHaveUniqueItems(record.StableId);
            record.CausalEdges.Should().OnlyContain(
                edge => throws.ContainsKey(edge.SourceId) && throws.ContainsKey(edge.TargetId),
                record.StableId
            );
            if (record.StableId is "highgate-synchronousonecount" or "highgate-synchronousonecountwithheffs")
            {
                foreach (var edge in record.CausalEdges)
                {
                    var source = throws[edge.SourceId];
                    var target = throws[edge.TargetId];
                    if (source.JugglerId == target.JugglerId)
                        target.Hand.Should().Be(source.Hand, $"{record.StableId}: {edge.SourceId}");
                    else
                        target.Hand.Should().NotBe(source.Hand, $"{record.StableId}: {edge.SourceId}");
                    (
                        target.StartTime + edge.CycleOffset * record.CycleLength
                    ).Should().Be(
                        source.StartTime + source.Height!.Value,
                        $"{record.StableId}: {edge.SourceId}"
                    );
                }
            }
            record.SourceLinks
                .Select(link => link.Edition)
                .Should()
                .Contain("2017 expanded PDF")
                .And.Contain("2014 PDF");

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
                    item.Height ?? 0m,
                    item.HeightIsExplicit
                )).ToList(),
                CausalEdges = record.CausalEdges.Select((edge, index) => new EditorCausalEdge(
                    $"{record.StableId}-e{index + 1}",
                    edge.SourceId,
                    edge.TargetId,
                    edge.CycleOffset
                )).ToList(),
            };
            var offsets = state.CausalEdges.Select(edge => edge.CycleOffset).ToArray();
            state.SynchronizeHeights();
            state.CausalEdges.Select(edge => edge.CycleOffset).Should().Equal(offsets, record.StableId);
            state.IsValidForEditor().Should().BeTrue(record.StableId);

            var imported = CausalDiagramEditorPresets.All
                .Single(preset => preset.Id == record.StableId)
                .Create();
            imported.CycleLength.Should().Be(state.CycleLength, record.StableId);
            imported.Jugglers.Select(item => (item.Id, item.Name))
                .Should()
                .Equal(state.Jugglers.Select(item => (item.Id, item.Name)), record.StableId);
            imported.Throws.Select(item =>
                    (item.Id, item.JugglerId, item.Hand, item.StartTime, item.Height)
                )
                .Should()
                .Equal(
                    state.Throws.Select(item =>
                        (item.Id, item.JugglerId, item.Hand, item.StartTime, item.Height)
                    ),
                    record.StableId
                );
            imported.CausalEdges.Select(item =>
                    (item.FromThrowId, item.ToThrowId, item.CycleOffset)
                )
                .Should()
                .Equal(
                    state.CausalEdges.Select(item =>
                        (item.FromThrowId, item.ToThrowId, item.CycleOffset)
                    ),
                    record.StableId
                );
        }

        selected["highgate-jimsthreecountasynchversion"].Throws.Should().HaveCount(28);
        selected["highgate-jimsthreecountasynchversion"].CausalEdges.Should().HaveCount(24);
        selected["highgate-synchronousonecount"].Throws.Should().HaveCount(4);
        selected["highgate-synchronousonecount"].CausalEdges.Should().HaveCount(4);
        selected["highgate-synchronousonecountwithheffs"].Throws.Should().HaveCount(8);
        selected["highgate-synchronousonecountwithheffs"].CausalEdges.Should().HaveCount(8);
    }

    private sealed record Record(
        [property: JsonPropertyName("stable_id")] string StableId,
        decimal CycleLength,
        List<Juggler> Jugglers,
        List<Throw> Throws,
        List<Edge> CausalEdges,
        List<SourceLink> SourceLinks
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

    private sealed record Edge(
        string? FromThrowId,
        string? ToThrowId,
        string? From,
        string? To,
        int CycleOffset
    )
    {
        public string SourceId => FromThrowId ?? From!;
        public string TargetId => ToThrowId ?? To!;
    }

    private sealed record SourceLink(string Edition);
}
