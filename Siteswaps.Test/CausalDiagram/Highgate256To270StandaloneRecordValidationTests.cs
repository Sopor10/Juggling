using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate256To270StandaloneRecordValidationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly HashSet<string> ExpectedIds =
    [
        "highgate-fourpersonfourcountdropbackline1",
        "highgate-fourcounty",
        "highgate-fourcountdropbacky",
        "highgate-fourpersonfourcountdropbackline2",
        "highgate-threecounty",
        "highgate-fourcounttrapeze",
        "highgate-alloutcivilwarfeed",
        "highgate-bookendsinvertedbookendsbox",
        "highgate-doubleurbanterrorfeed",
        "highgate-threecountaccommodation",
        "highgate-lowfatchocolatebox",
        "highgate-bookshelf",
        "highgate-chocolatebookshelf",
        "highgate-countdownaccommodation1",
        "highgate-countdownaccommodation2",
        "highgate-jimsthreecounty",
        "highgate-rowrowrowyourboat",
        "highgate-rowrowrowyourboatambidextrousvariation",
        "highgate-frerejacques",
        "highgate-fourpersonthreecountrickspicnic",
        "highgate-fourpersontwocountrickspicnic",
        "highgate-fourpersononecountrickspicnic",
        "highgate-funkybookendsparsnipbox",
    ];

    [Test]
    public void EveryStandaloneRecordCreatesAValidEditorStateAndPreservesOffsets()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-transcriptions-256-270.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();

        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-256-270.json"
                )
            ),
            JsonOptions
        )!;
        records.Select(record => record.Id).Should().BeEquivalentTo(ExpectedIds);
        records.Should().HaveCount(ExpectedIds.Count);
        records.Select(record => record.Id).Should().OnlyHaveUniqueItems();

        foreach (var record in records)
        {
            var throwIds = record.Throws.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
            record.Throws.Should().OnlyHaveUniqueItems(item => item.Id);
            record.Jugglers.Should().OnlyHaveUniqueItems(item => item.Id);
            record.CausalEdges.Should().NotBeEmpty(record.Id);
            record.CausalEdges.Select(edge => edge.From).Should().OnlyHaveUniqueItems(record.Id);
            record
                .CausalEdges.Should()
                .OnlyContain(edge => throwIds.Contains(edge.From) && throwIds.Contains(edge.To));
            var throwsById = record.Throws.ToDictionary(item => item.Id, StringComparer.Ordinal);
            foreach (var edge in record.CausalEdges)
            {
                var source = throwsById[edge.From];
                var target = throwsById[edge.To];
                if (source.HeightIsExplicit && source.Height is not null)
                {
                    (
                        target.StartTime
                        + edge.CycleOffset * record.CycleLength
                        + 2
                        - source.StartTime
                    )
                        .Should()
                        .Be(source.Height.Value, record.Id);
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
                        item.StartTime,
                        item.Height ?? 0m,
                        item.HeightIsExplicit
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

            state.SynchronizeHeights();

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
