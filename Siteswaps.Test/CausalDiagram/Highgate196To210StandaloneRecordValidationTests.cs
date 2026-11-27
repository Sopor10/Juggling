using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate196To210StandaloneRecordValidationTests
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
                    "docs/research/highgate-transcriptions-196-210.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();
        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-196-210.json"
                )
            ),
            JsonOptions
        )!;
        records.Select(record => record.Id).Should().OnlyHaveUniqueItems();

        foreach (var record in records)
        {
            record.CausalEdges.Should().HaveCount(record.Throws.Count, record.Id);
            record
                .CausalEdges.Where(edge => edge.Id is not null)
                .Select(edge => edge.Id)
                .Should()
                .OnlyHaveUniqueItems(record.Id);
            var throwsById = record.Throws.ToDictionary(item => item.Id);
            throwsById.Keys.Should().OnlyHaveUniqueItems(record.Id);
            var outgoing = new HashSet<string>();
            foreach (var edge in record.CausalEdges)
            {
                throwsById.Should().ContainKey(edge.FromId, record.Id);
                throwsById.Should().ContainKey(edge.ToIdValue, record.Id);
                outgoing.Add(edge.FromId).Should().BeTrue(record.Id);
                var source = throwsById[edge.FromId];
                var target = throwsById[edge.ToIdValue];
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
            if (record.Id == "highgate-tenclubgorilla")
            {
                var destinations = record.CausalEdges.ToDictionary(
                    edge => edge.FromId,
                    edge => (edge.ToIdValue, edge.CycleOffset)
                );
                destinations
                    .Should()
                    .Contain(new KeyValuePair<string, (string, int)>("B-R1", ("C-L2", 0)));
                destinations
                    .Should()
                    .Contain(new KeyValuePair<string, (string, int)>("B-L2", ("A-R2", 0)));
                destinations
                    .Should()
                    .Contain(new KeyValuePair<string, (string, int)>("C-R1", ("B-L2", 0)));
                destinations
                    .Should()
                    .Contain(new KeyValuePair<string, (string, int)>("C-L2", ("C-R1", 1)));
                destinations
                    .Should()
                    .Contain(new KeyValuePair<string, (string, int)>("A-R2", ("B-L2", 1)));
                destinations
                    .Should()
                    .Contain(new KeyValuePair<string, (string, int)>("A-L1", ("A-R2", 0)));
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
                        item.Height,
                        item.HeightIsExplicit
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

            state.SynchronizeHeights();

            state.CausalEdges.Select(edge => edge.CycleOffset).Should().Equal(offsets, record.Id);
            state.IsValidForEditor().Should().BeTrue(record.Id);
        }

        records.Should().HaveCount(5);
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
        decimal Height = 0,
        bool HeightIsExplicit = false
    );

    private sealed class Edge
    {
        public string? Id { get; init; }
        public string? From { get; init; }
        public string? To { get; init; }
        public string? FromThrowId { get; init; }
        public string? ToThrowId { get; init; }
        public int CycleOffset { get; init; }

        public string FromId => FromThrowId ?? From!;
        public string ToIdValue => ToThrowId ?? To!;
    }
}
