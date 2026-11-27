using System.Globalization;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class HighgateRunawayStandaloneStateValidationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Test]
    public void EveryPeriodicPhaseIsAValidGraphAndPreservesOffsets()
    {
        var records = LoadRecords();
        records.Select(item => item.Id).Should().OnlyHaveUniqueItems();
        records.Should().HaveCount(4);
        foreach (var record in records)
        {
            record.Throws.Should().HaveCount(24, record.Id);
            record.CausalEdges.Should().HaveCount(24, record.Id);
            record.CausalEdges.Select(edge => edge.From).Should().OnlyHaveUniqueItems(record.Id);
            var ids = record.Throws.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
            record
                .CausalEdges.Should()
                .OnlyContain(edge => ids.Contains(edge.From) && ids.Contains(edge.To), record.Id);
            var state = new CausalDiagramEditorState
            {
                PresetId = "highgate-three-count",
                CycleLength = record.CycleLength,
                ActiveJugglerId = record.Jugglers[0].Id,
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
                                $"runaway-{index}",
                                item.From,
                                item.To,
                                item.CycleOffset
                            )
                    )
                    .ToList(),
            };
            var offsets = state.CausalEdges.Select(edge => edge.CycleOffset).ToArray();
            state.NormalizeCausalEdgeOffsets();
            state.CausalEdges.Select(edge => edge.CycleOffset).Should().Equal(offsets, record.Id);
            state.IsValidForEditor().Should().BeTrue(record.Id);
        }
    }

    [Test]
    public void EveryPhaseMatchesItsPrintedActorRowsAndRoutes()
    {
        foreach (var record in LoadRecords())
        {
            var idByName = record.Jugglers.ToDictionary(item => item.Name, item => item.Id);
            var throwById = record.Throws.ToDictionary(item => item.Id, StringComparer.Ordinal);
            var edgeByFrom = record.CausalEdges.ToDictionary(
                item => item.From,
                StringComparer.Ordinal
            );
            foreach (var (name, row) in record.SourceRows)
            {
                var events = record
                    .Throws.Where(item => item.JugglerId == idByName[name])
                    .OrderBy(item => item.StartTime)
                    .ToArray();
                events.Should().HaveCount((int)record.CycleLength, $"{record.Id} {name}");
                foreach (var source in events)
                {
                    var index =
                        int.Parse(
                            source.Id[(source.Id.LastIndexOf('-') + 1)..],
                            CultureInfo.InvariantCulture
                        ) - 1;
                    var token = row[
                        (index + record.SourceRowOffsetByJuggler.GetValueOrDefault(name))
                            % row.Length
                    ];
                    var targetName = token == "self" ? name : token;
                    var target = throwById[edgeByFrom[source.Id].To];
                    target.JugglerId.Should().Be(idByName[targetName], $"{record.Id} {source.Id}");
                    var route = token == "self" ? "opposite" : record.PassDirectionByJuggler[name];
                    var targetHand = route is "straight" or "opposite"
                        ? source.Hand == "R"
                            ? "L"
                            : "R"
                        : source.Hand;
                    target.Hand.Should().Be(targetHand, $"{record.Id} {source.Id}");

                    var candidates = record
                        .Throws.Where(item => item.JugglerId == target.JugglerId)
                        .SelectMany(item =>
                            Enumerable
                                .Range(-2, 5)
                                .Select(offset => new
                                {
                                    Throw = item,
                                    Offset = offset,
                                    Time = item.StartTime + offset * record.CycleLength,
                                })
                        )
                        .Where(item => item.Throw.Hand == targetHand)
                        .Where(item => item.Time >= source.StartTime + 1)
                        .OrderBy(item => item.Time)
                        .First();
                    target.Id.Should().Be(candidates.Throw.Id, $"{record.Id} {source.Id} endpoint");
                    edgeByFrom[source.Id]
                        .CycleOffset.Should()
                        .Be(candidates.Offset, $"{record.Id} {source.Id} offset");
                }
            }
        }
    }

    private static List<Record> LoadRecords()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-runaway-source-states.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();
        return JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-runaway-source-states.json"
                )
            ),
            JsonOptions
        )!;
    }

    private sealed record Record(
        string Id,
        decimal CycleLength,
        List<Juggler> Jugglers,
        List<Throw> Throws,
        List<Edge> CausalEdges,
        Dictionary<string, string[]> SourceRows,
        Dictionary<string, int> SourceRowOffsetByJuggler,
        Dictionary<string, string> PassDirectionByJuggler
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
