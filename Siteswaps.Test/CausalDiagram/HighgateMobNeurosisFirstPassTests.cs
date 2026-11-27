using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class HighgateMobNeurosisFirstPassTests
{
    [Test]
    public void FirstAnneAndBenPassesMatchThePrintedRowsAndCausalDiagram()
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
        {
            repository = repository.Parent;
        }

        repository.Should().NotBeNull();
        using var source = JsonDocument.Parse(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-241-255.json"
                )
            )
        );
        var record = source
            .RootElement.EnumerateArray()
            .Single(item => item.GetProperty("id").GetString() == "highgate-mobneurosispsychosis");

        record.GetProperty("sourceRows").GetProperty("Anne")[0].GetString().Should().Be("Ben");
        record.GetProperty("sourceRows").GetProperty("Ben")[0].GetString().Should().Be("Anne");
        AssertEdge(record.GetProperty("causalEdges").EnumerateArray(), "A-R1", "B-R3");
        AssertEdge(record.GetProperty("causalEdges").EnumerateArray(), "B-R1", "A-L2");

        var production = CausalDiagramEditorPresets
            .AllForLoading.Single(preset => preset.Id == "highgate-mobneurosispsychosis")
            .Create();
        production
            .CausalEdges.Should()
            .Contain(edge =>
                edge.FromThrowId == "A-R1" && edge.ToThrowId == "B-R3" && edge.CycleOffset == 0
            );
        production
            .CausalEdges.Should()
            .Contain(edge =>
                edge.FromThrowId == "B-R1" && edge.ToThrowId == "A-L2" && edge.CycleOffset == 0
            );
    }

    private static void AssertEdge(IEnumerable<JsonElement> edges, string from, string to)
    {
        edges
            .Should()
            .Contain(edge =>
                edge.GetProperty("from").GetString() == from
                && edge.GetProperty("to").GetString() == to
                && edge.GetProperty("cycleOffset").GetInt32() == 0
            );
    }
}
