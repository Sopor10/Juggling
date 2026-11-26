using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate91To105StandaloneRecordValidationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Test]
    public void NewOrdinaryPopcornGraphsAreValidAndPreserveOffsets()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-transcriptions-91-105.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();

        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-91-105.json"
                )
            ),
            JsonOptions
        )!;
        var expectedCounts = new Dictionary<string, int>
        {
            ["highgate-classicsixcountpopcorn"] = 12,
            ["highgate-sixcountpopcornwithheffs"] = 12,
            ["highgate-fivecountpopcorn"] = 20,
            ["highgate-fivecountpopcornwithheffs"] = 20,
            ["highgate-fivecountpopcornwithdoublepassesandheffs"] = 20,
            ["highgate-fourcountpopcorn"] = 8,
            ["highgate-fourcountpopcornwithheffs"] = 8,
            ["highgate-sevenclubwhynotstylepattern"] = 28,
            ["highgate-sevenclubnotwhystylepattern"] = 28,
            ["highgate-sevenclubmaybestylepattern1"] = 28,
            ["highgate-sevenclubmaybestylepattern2"] = 28,
        };
        var selectedRecords = records.Where(item => expectedCounts.ContainsKey(item.Id)).ToArray();
        selectedRecords.Should().HaveCount(expectedCounts.Count);
        foreach (var record in selectedRecords)
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
                        item.StartTime,
                        item.Height,
                        item.HeightIsExplicit
                    ))
                    .ToList(),
                CausalEdges = record
                    .CausalEdges.Select(
                        (item, index) =>
                            new EditorCausalEdge(
                                $"check-{index}",
                                item.FromThrowId ?? item.From ?? string.Empty,
                                item.ToThrowId ?? item.To ?? string.Empty,
                                item.CycleOffset
                            )
                    )
                    .ToList(),
            };
            var offsets = state.CausalEdges.Select(edge => edge.CycleOffset).ToArray();

            state.SynchronizeHeights();

            state.Throws.Should().HaveCount(expectedCounts[record.Id], record.Id);
            state.CausalEdges.Should().HaveCount(expectedCounts[record.Id], record.Id);
            state.Throws.Should().OnlyContain(item => item.HeightIsExplicit, record.Id);
            state.CausalEdges.Select(edge => edge.CycleOffset).Should().Equal(offsets);
            state.IsValidForEditor().Should().BeTrue(record.Id);

            var imported = CausalDiagramEditorPresets.All
                .Single(preset => preset.Id == record.Id)
                .Create();
            imported.IsValidForEditor().Should().BeTrue(record.Id);
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

        var classic = selectedRecords.Single(item => item.Id == "highgate-classicsixcountpopcorn");
        classic.Throws.Single(item => item.Id == "A-R1").Height.Should().Be(4m);
        classic.Throws.Single(item => item.Id == "A-R5").Height.Should().Be(5m);
        classic.Throws.Single(item => item.Id == "B-R1").Height.Should().Be(5m);
        classic.Throws.Single(item => item.Id == "B-R3").Height.Should().Be(4m);
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

    private sealed record Edge(
        string? From,
        string? To,
        int CycleOffset,
        string? FromThrowId = null,
        string? ToThrowId = null
    );
}
