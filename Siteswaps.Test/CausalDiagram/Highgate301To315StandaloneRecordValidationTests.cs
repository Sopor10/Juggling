using System.Globalization;
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

        records.Should().HaveCount(26);
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

    [Test]
    public void AddedRecordsFollowThePrintedTargetRowsAndPassDirections()
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

        foreach (var record in records.Where(item => item.SourceRows is not null))
        {
            var idByName = record.Jugglers.ToDictionary(item => item.Name, item => item.Id);
            var throwById = record.Throws.ToDictionary(item => item.Id, StringComparer.Ordinal);
            var edgeBySource = record.CausalEdges.ToDictionary(
                item => item.From,
                item => item.To,
                StringComparer.Ordinal
            );
            foreach (var (name, row) in record.SourceRows!)
            {
                var jugglerId = idByName[name];
                var events = record
                    .Throws.Where(item => item.JugglerId == jugglerId)
                    .OrderBy(item => item.StartTime)
                    .ToArray();
                events.Should().HaveCount((int)record.CycleLength, $"{record.Id} {name}");
                var sourceRows = row.ToArray();
                foreach (var source in events)
                {
                    var index =
                        int.Parse(
                            source.Id[(source.Id.LastIndexOf('-') + 1)..],
                            CultureInfo.InvariantCulture
                        ) - 1;
                    var token = sourceRows[index % sourceRows.Length];
                    var tokenParts = token.Split(':');
                    var targetName =
                        token is "self" or "zip" or "heff" ? name
                        : tokenParts.Length > 1 ? tokenParts[1]
                        : token;
                    var target = throwById[edgeBySource[source.Id]];
                    target.JugglerId.Should().Be(idByName[targetName], $"{record.Id} {source.Id}");

                    string? route = null;
                    if (token is "self" or "zip")
                        route = "opposite";
                    else if (token == "heff")
                        route = "same";
                    else
                    {
                        var routeKey = $"{name}->{targetName}";
                        route =
                            tokenParts.Length == 3
                                ? tokenParts[2]
                                : record.SourceRouteOverrides?.GetValueOrDefault(routeKey)
                                    ?? record.PassDirectionByJuggler?.GetValueOrDefault(name);
                    }
                    if (route is not null)
                    {
                        var expectedHand = route switch
                        {
                            "same" or "crossing" => source.Hand,
                            "opposite" or "straight" => source.Hand == "R" ? "L" : "R",
                            _ => throw new AssertionException(
                                $"Unknown route {route} in {record.Id}"
                            ),
                        };
                        target.Hand.Should().Be(expectedHand, $"{record.Id} {source.Id} ({token})");
                    }
                    if (route is not null)
                    {
                        var expectedHand = route switch
                        {
                            "same" or "crossing" => source.Hand,
                            "opposite" or "straight" => source.Hand == "R" ? "L" : "R",
                            _ => throw new AssertionException(
                                $"Unknown route {route} in {record.Id}"
                            ),
                        };
                        var isZip = token == "zip";
                        var minimumDelta = token switch
                        {
                            "zip" => -1m,
                            "heff" => 2m,
                            "self" => 1m,
                            _ when tokenParts[0] == "double" => 2m,
                            _ => 1m,
                        };
                        var threshold = source.StartTime + minimumDelta;
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
                            .Where(item => item.Throw.Hand == expectedHand)
                            .Where(item => isZip ? item.Time <= threshold : item.Time >= threshold)
                            .ToArray();
                        var expected = isZip
                            ? candidates.OrderByDescending(item => item.Time).First()
                            : candidates.OrderBy(item => item.Time).First();
                        target
                            .Id.Should()
                            .Be(expected.Throw.Id, $"{record.Id} {source.Id} endpoint");
                        record
                            .CausalEdges.Single(item => item.From == source.Id)
                            .CycleOffset.Should()
                            .Be(expected.Offset, $"{record.Id} {source.Id} cycle offset");
                    }
                }
            }
        }
    }

    private sealed record Record(
        string Id,
        decimal CycleLength,
        List<Juggler> Jugglers,
        List<Throw> Throws,
        List<Edge> CausalEdges,
        Dictionary<string, string[]>? SourceRows = null,
        Dictionary<string, string>? PassDirectionByJuggler = null,
        Dictionary<string, string>? SourceRouteOverrides = null
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
