using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate226To240StandaloneRecordValidationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly HashSet<string> ExpectedIds =
    [
        "highgate-fourpersononecountfeedingpasspassself",
        "highgate-fourpersononecountfeedingchocolatebar",
        "highgate-fourpersononecountfeedingbookends",
        "highgate-fourpersononecountfeedinginvertedbookends",
        "highgate-fourpersononecountfeedingcountdown",
        "highgate-fourpersoncivilwarfeed",
        "highgate-mixedcountfeed",
        "highgate-passpassselfbox",
        "highgate-chocolatesquare",
        "highgate-bookendsbox",
        "highgate-invertedbookendsbox",
        "highgate-twocountlinefeed",
        "highgate-urbanterrorfeedonecountlinefeed",
        "highgate-fourpersonparsnipfeedingfivecountpopcorn",
    ];

    private static readonly string[][] FourPersonParsnipFeedingFiveCountPopcornRows =
    [
        ["heff", "heff", "Ben", "self", "self"],
        ["Anne", "Clare", "zip", "Clare", "zip"],
        ["Ben", "zip", "Daniel", "Ben", "zip"],
        ["Clare", "self", "self", "heff", "heff"],
    ];

    [Test]
    public void EveryInRangeStandaloneRecordCreatesAValidEditorStateAndPreservesOffsets()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-transcriptions-226-240.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();

        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-226-240.json"
                )
            ),
            JsonOptions
        )!;
        records.Select(record => record.Id).Should().BeEquivalentTo(ExpectedIds);
        records.Select(record => record.Id).Should().OnlyHaveUniqueItems();

        foreach (var record in records)
        {
            record.CausalEdges.Should().HaveCount(record.Throws.Count, record.Id);
            record.Throws.Select(item => item.Id).Should().OnlyHaveUniqueItems(record.Id);
            record.CausalEdges.Select(edge => edge.From).Should().OnlyHaveUniqueItems(record.Id);
            var throwsById = record.Throws.ToDictionary(item => item.Id);
            var throwIds = throwsById.Keys.ToHashSet();
            foreach (var edge in record.CausalEdges)
            {
                throwIds.Should().Contain(edge.From, record.Id);
                throwIds.Should().Contain(edge.To, record.Id);
                var source = throwsById[edge.From];
                var target = throwsById[edge.To];
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

            if (record.Id == "highgate-fourpersonparsnipfeedingfivecountpopcorn")
                ValidateFourPersonParsnipFeedingFiveCountPopcorn(record);
        }
    }

    private static void ValidateFourPersonParsnipFeedingFiveCountPopcorn(Record record)
    {
        record.CausalEdges.Select(edge => edge.To).Should().OnlyHaveUniqueItems(record.Id);
        var jugglerIds = record.Jugglers.ToDictionary(item => item.Name, item => item.Id);
        foreach (
            var (jugglerIndex, row) in FourPersonParsnipFeedingFiveCountPopcornRows.Select(
                (row, index) => (index, row)
            )
        )
        {
            var jugglerId = record.Jugglers[jugglerIndex].Id;
            var firstEventTime = record
                .Throws.Where(item => item.JugglerId == jugglerId)
                .Min(item => item.StartTime);
            foreach (var source in record.Throws.Where(item => item.JugglerId == jugglerId))
            {
                var beat = (int)(source.StartTime - firstEventTime);
                var action = row[beat % row.Length];
                string expectedJuggler;
                decimal expectedTime;
                if (action == "self")
                {
                    expectedJuggler = jugglerId;
                    expectedTime = source.StartTime + 1;
                }
                else if (action == "zip")
                {
                    expectedJuggler = jugglerId;
                    expectedTime = source.StartTime - 1;
                }
                else if (action == "heff")
                {
                    expectedJuggler = jugglerId;
                    expectedTime = source.StartTime + 2;
                }
                else
                {
                    expectedJuggler = jugglerIds[action];
                    var receiverTimes = record
                        .Throws.Where(item => item.JugglerId == expectedJuggler)
                        .Select(item => item.StartTime);
                    expectedTime = receiverTimes
                        .SelectMany(time =>
                            Enumerable
                                .Range(-1, 3)
                                .Select(offset =>
                                    (Time: time + offset * record.CycleLength, Offset: offset)
                                )
                        )
                        .Where(candidate => candidate.Time >= source.StartTime + 1)
                        .Min(candidate => candidate.Time);
                }

                var target = record.Throws.Single(item =>
                    item.JugglerId == expectedJuggler
                    && item.StartTime
                        + Math.Floor((expectedTime - item.StartTime) / record.CycleLength)
                            * record.CycleLength
                        == expectedTime
                );
                var edge = record.CausalEdges.Single(item => item.From == source.Id);
                edge.To.Should().Be(target.Id, source.Id);
                (target.StartTime + edge.CycleOffset * record.CycleLength)
                    .Should()
                    .Be(expectedTime, source.Id);
            }
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
