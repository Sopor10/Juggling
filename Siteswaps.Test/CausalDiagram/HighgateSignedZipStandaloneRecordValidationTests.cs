using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class HighgateSignedZipStandaloneRecordValidationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Test]
    public void SignedZipGraphsPassEditorValidationAndPreserveOffsets()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-signed-zip-transcriptions.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();

        using var document = JsonDocument.Parse(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-signed-zip-transcriptions.json"
                )
            )
        );
        var records = JsonSerializer.Deserialize<List<Record>>(
            document.RootElement.GetProperty("records"),
            JsonOptions
        );
        records.Should().NotBeNull();
        records!
            .Select(record => record.Id)
            .Should()
            .BeEquivalentTo(
                "highgate-passselfzip",
                "highgate-therecipe",
                "highgate-fourfourone",
                "highgate-fourfourfourzero",
                "highgate-aselftriple",
                "highgate-58552",
                "highgate-doublezapzip531"
            );
        records
            .Single(record => record.Id == "highgate-therecipe")
            .CausalEdges.Should()
            .HaveCount(14);
        records
            .Single(record => record.Id == "highgate-fourfourone")
            .CausalEdges.Should()
            .Contain(edge =>
                edge.FromThrowId == "B-R1" && edge.ToThrowId == "B-L3" && edge.CycleOffset == -1
            );
        records
            .Single(record => record.Id == "highgate-fourfourfourzero")
            .CausalEdges.Should()
            .Contain(edge =>
                edge.FromThrowId == "B-L1" && edge.ToThrowId == "B-L3" && edge.CycleOffset == -1
            );
        records
            .Single(record => record.Id == "highgate-aselftriple")
            .CausalEdges.Should()
            .Contain(edge =>
                edge.FromThrowId == "B-L3" && edge.ToThrowId == "B-R1" && edge.CycleOffset == 1
            );
        records.Single(record => record.Id == "highgate-58552").CausalEdges.Should().HaveCount(20);
        records
            .Single(record => record.Id == "highgate-doublezapzip531")
            .CausalEdges.Should()
            .Contain(edge =>
                edge.FromThrowId == "B-L1" && edge.ToThrowId == "B-R1" && edge.CycleOffset == 0
            );

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
                        (item, index) =>
                            new EditorCausalEdge(
                                $"signed-zip-{index}",
                                item.FromThrowId,
                                item.ToThrowId,
                                item.CycleOffset
                            )
                    )
                    .ToList(),
            };
            var offsets = state.CausalEdges.Select(edge => edge.CycleOffset).ToArray();
            state.NormalizeCausalEdgeOffsets();
            state.CausalEdges.Select(edge => edge.CycleOffset).Should().Equal(offsets, record.Id);
            state.IsValidForEditor().Should().BeTrue(record.Id);
            state
                .CausalEdges.Select(edge => edge.FromThrowId)
                .Should()
                .OnlyHaveUniqueItems(record.Id);
            state
                .CausalEdges.Select(edge => edge.FromThrowId)
                .Should()
                .BeEquivalentTo(state.Throws.Select(throwNode => throwNode.Id), record.Id);
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
        decimal Height,
        bool HeightIsExplicit
    );

    private sealed record Edge(string FromThrowId, string ToThrowId, int CycleOffset);
}
