using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class HighgateManipulationStandaloneRecordValidationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly IReadOnlyDictionary<string, PatternSequence> SourceSequences =
        new Dictionary<string, PatternSequence>
        {
            ["highgate-sevenclubjimstwocountvariation1"] = new(
                ["dp", "self", "sp", "self", "dp", "hold", "heff"],
                ["dp", "hold", "heff", "dp", "self", "sp", "self"]
            ),
            ["highgate-sevenclubjimstwocountvariation2"] = new(
                ["dp", "heff", "dp", "self", "sp", "hold", "self"],
                ["sp", "hold", "self", "dp", "heff", "dp", "self"]
            ),
            ["highgate-sevenclubjimstwocountvariation3"] = new(
                ["dp", "heff", "sp", "self", "sp", "hold", "heff"],
                ["sp", "hold", "heff", "dp", "heff", "sp", "self"]
            ),
            ["highgate-sevenclubwhynotvariation"] = new(
                ["dp", "heff", "dp", "self", "sp", "heff", "zip"],
                ["sp", "heff", "zip", "dp", "heff", "dp", "self"]
            ),
            ["highgate-notlikely"] = new(
                ["dp", "self", "zip", "self", "sp"],
                ["self", "sp", "dp", "self", "zip"]
            ),
            ["highgate-maybenot"] = new(
                ["sp", "self", "zip", "dp", "self"],
                ["dp", "self", "sp", "self", "zip"]
            ),
            ["highgate-fivecountpopcornvsfiveclubwhynot"] = new(
                ["pass", "self", "self", "heff", "heff"],
                ["heff", "zip", "pass", "self", "zip"]
            ),
            ["highgate-fiveclubwhynotvsfivecountpopcorn"] = new(
                ["pass", "self", "zip", "heff", "zip"],
                ["heff", "heff", "pass", "self", "self"]
            ),
            ["highgate-97586"] = new(
                ["dp", "zap", "self", "sp", "heff"],
                ["sp", "heff", "dp", "zap", "self"]
            ),
            ["highgate-78956"] = new(
                ["sp", "dp", "self", "heff", "zap"],
                ["heff", "zap", "sp", "dp", "self"]
            ),
            ["highgate-79685"] = new(
                ["sp", "self", "zap", "dp", "heff"],
                ["dp", "heff", "sp", "self", "zap"]
            ),
        };

    [Test]
    public void PeriodicManipulationClassRecordsAreValidAndEveryEdgeMatchesTheSourceSequence()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-manipulation-standalone-records.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();

        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-manipulation-standalone-records.json"
                )
            ),
            JsonOptions
        )!;
        records.Select(record => record.Id).Should().BeEquivalentTo(SourceSequences.Keys);

        foreach (var record in records)
        {
            var sequence = SourceSequences[record.Id];
            record.CycleLength.Should().Be(sequence.Anne.Length * 2, record.Id);
            record.Throws.Should().HaveCount((int)record.CycleLength * 2, record.Id);
            record.Throws.Select(item => item.Id).Should().OnlyHaveUniqueItems(record.Id);
            record.SourceLinks.Select(item => item.Edition).Should().Contain("2017 expanded PDF");

            var nodeByJugglerTime = record.Throws.ToDictionary(item =>
                (item.JugglerId, item.StartTime)
            );
            var edgesBySource = record.CausalEdges.ToDictionary(edge => edge.From);
            var expectedEdgeCount = 0;
            foreach (var node in record.Throws)
            {
                var sourceTime = node.JugglerId == "A" ? node.StartTime : node.StartTime - 0.5m;
                var eventIndex = (int)(sourceTime % sequence.Anne.Length);
                var token = (node.JugglerId == "A" ? sequence.Anne : sequence.Ben)[eventIndex];
                if (token == "hold")
                {
                    edgesBySource.Should().NotContainKey(node.Id, record.Id);
                    node.Height.Should().Be(0, record.Id);
                    continue;
                }

                expectedEdgeCount++;
                edgesBySource.Should().ContainKey(node.Id, record.Id);
                var edge = edgesBySource[node.Id];
                var expectedHeight = token switch
                {
                    "dp" => 4.5m,
                    "sp" => 3.5m,
                    "pass" => 3.5m,
                    "zap" => 2.5m,
                    "self" => 3m,
                    "heff" => 4m,
                    "zip" => 1m,
                    _ => throw new AssertionException(
                        $"Unknown source token {token} in {record.Id}."
                    ),
                };
                node.Height.Should().Be(expectedHeight, record.Id);
                node.HeightIsExplicit.Should().BeTrue(record.Id);

                var targetJuggler = token is "dp" or "sp" or "pass" or "zap"
                    ? (node.JugglerId == "A" ? "B" : "A")
                    : node.JugglerId;
                var landingTime = node.StartTime + expectedHeight - 2m;
                var baseTargetTime = landingTime % record.CycleLength;
                var target = nodeByJugglerTime[(targetJuggler, baseTargetTime)];
                edge.To.Should().Be(target.Id, $"{record.Id} {node.Id} ({token})");
                edge.CycleOffset.Should()
                    .Be(
                        (int)((landingTime - baseTargetTime) / record.CycleLength),
                        $"{record.Id} {node.Id} ({token})"
                    );
            }

            record.CausalEdges.Should().HaveCount(expectedEdgeCount, record.Id);
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
                                $"manipulation-check-{index}",
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

    private sealed record PatternSequence(string[] Anne, string[] Ben);

    private sealed record Record(
        string Id,
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

    private sealed record Edge(string From, string To, int CycleOffset);

    private sealed record SourceLink(string Edition, int? PrintedPage, int PdfPage);
}
