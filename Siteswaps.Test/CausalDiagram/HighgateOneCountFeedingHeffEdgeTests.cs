using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class HighgateOneCountFeedingHeffEdgeTests
{
    [Test]
    public void HeffsInTheThreeOneCountFeedingPresetsReturnToTheSameHandTwoBeatsLater()
    {
        var expected = new[]
        {
            (Id: "highgate-onecountfeedingfiveclubwhynotandfunkybookends", From: "A-R1", To: "A-R3", Offset: 0),
            (Id: "highgate-onecountfeedingfiveclubwhynotandfunkybookends", From: "A-L6", To: "A-L8", Offset: 0),
            (Id: "highgate-onecountfeedingwhynotandfiveclubwhynot", From: "A-L4", To: "A-L6", Offset: 0),
            (Id: "highgate-onecountfeedingwhynotandfiveclubwhynot", From: "A-R9", To: "A-R1", Offset: 1),
            (Id: "highgate-onecountfeedingmaybeandfiveclubwhynot", From: "A-R1", To: "A-R3", Offset: 0),
            (Id: "highgate-onecountfeedingmaybeandfiveclubwhynot", From: "A-L6", To: "A-L8", Offset: 0),
        };

        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(repository.FullName, "docs/research/highgate-transcriptions-241-255.json")
            )
        )
        {
            repository = repository.Parent;
        }

        repository.Should().NotBeNull();
        using var source = JsonDocument.Parse(
            File.ReadAllText(
                Path.Combine(repository!.FullName, "docs/research/highgate-transcriptions-241-255.json")
            )
        );

        foreach (var group in expected.GroupBy(edge => edge.Id))
        {
            var sourceRecord = source.RootElement
                .EnumerateArray()
                .Single(record => record.GetProperty("id").GetString() == group.Key);
            var sourceEdges = sourceRecord.GetProperty("causalEdges").EnumerateArray().ToArray();
            var production = CausalDiagramEditorPresets.AllForLoading.Single(preset => preset.Id == group.Key).Create();

            foreach (var edge in group)
            {
                sourceEdges.Should().Contain(sourceEdge =>
                    sourceEdge.GetProperty("from").GetString() == edge.From
                    && sourceEdge.GetProperty("to").GetString() == edge.To
                    && sourceEdge.GetProperty("cycleOffset").GetInt32() == edge.Offset
                );
                production.CausalEdges.Should().Contain(productionEdge =>
                    productionEdge.FromThrowId == edge.From
                    && productionEdge.ToThrowId == edge.To
                    && productionEdge.CycleOffset == edge.Offset
                );
            }
        }
    }
}
