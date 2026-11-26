using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate301To315StandaloneRecordValidationTests
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
                    "docs/research/highgate-transcriptions-301-315.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();
        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-301-315.json"
                )
            ),
            JsonOptions
        )!;

        foreach (var record in records)
        {
            var throwIds = record.Throws.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
            record.CausalEdges.Should().HaveCount(record.Throws.Count, record.Id);
            record.CausalEdges.Select(edge => edge.From).Should().OnlyHaveUniqueItems(record.Id);
            record
                .CausalEdges.Should()
                .OnlyContain(
                    edge => throwIds.Contains(edge.From) && throwIds.Contains(edge.To),
                    record.Id
                );

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
                                $"validation-{index}",
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
        }

        records.Should().HaveCount(9);
    }

    [Test]
    public void CountdownFeedingTargetsMatchTheSourceRows()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-transcriptions-301-315.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();
        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-301-315.json"
                )
            ),
            JsonOptions
        )!;
        var pattern = records.Single(item =>
            item.Id == "highgate-thirteenclubonecountfeedingcountdown"
        );
        var expectedTargets = new Dictionary<string, string[]>
        {
            ["P1"] = ["P2", "P1", "P1", "P2", "P1", "P2", "P2", "P1"],
            ["P2"] = ["P1", "P3", "P3", "P1", "P3", "P1", "P1", "P3"],
            ["P3"] = ["P2", "P2", "P4", "P2", "P4", "P4", "P2", "P4"],
            ["P4"] = ["P3", "P4", "P4", "P3", "P4", "P3", "P3", "P4"],
        };
        var throwById = pattern.Throws.ToDictionary(item => item.Id, StringComparer.Ordinal);
        var targetBySource = pattern.CausalEdges.ToDictionary(
            item => item.From,
            item => throwById[item.To].JugglerId,
            StringComparer.Ordinal
        );

        foreach (var (jugglerId, expected) in expectedTargets)
        {
            var row = pattern
                .Throws.Where(item => item.JugglerId == jugglerId)
                .OrderBy(item => item.StartTime)
                .ToArray();
            row.Should().HaveCount(expected.Length, jugglerId);
            row.Select(item => targetBySource[item.Id]).Should().Equal(expected, jugglerId);
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
        bool HeightIsExplicit = false
    );

    private sealed record Edge(string From, string To, int CycleOffset);
}
