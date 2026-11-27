using System.Globalization;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class Highgate316To330StandaloneRecordValidationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Test]
    public void EveryStandaloneRecordCreatesAValidStateAndPreservesOffsets()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-transcriptions-316-330.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();
        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-316-330.json"
                )
            ),
            JsonOptions
        )!;

        foreach (var record in records)
        {
            record.Throws.Select(item => item.Id).Should().OnlyHaveUniqueItems(record.Id);
            record.CausalEdges.Should().HaveCount(record.Throws.Count, record.Id);
            record.CausalEdges.Select(edge => edge.From).Should().OnlyHaveUniqueItems(record.Id);
            var throwIds = record.Throws.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
            record
                .CausalEdges.Should()
                .OnlyContain(
                    edge => throwIds.Contains(edge.From) && throwIds.Contains(edge.To),
                    record.Id
                );

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

        records
            .Select(record => record.Id)
            .Should()
            .BeEquivalentTo(
                new[]
                {
                    "highgate-thirteenclubfourcount",
                    "highgate-thirteenclubthreecount",
                    "highgate-thirteenclubtwocount",
                    "highgate-thirteenclubonecount",
                    "highgate-thirteenclubfunkybookendslinefeedfeedingfivecoun-cde45e90c5",
                    "highgate-thirteenclubmartinsonecountlinefeedfeedingfiveco-44febdca3e",
                    "highgate-thirteenclubonecountlinefeedfeedingfivecountpopcornandparsnip",
                    "highgate-thirteenclubonecountlinefeedfeedingwhynotandfivecountpopcorn",
                    "highgate-thirteenclubonecountlinefeedfeedingmaybeandfivecountpopcorn",
                    "highgate-thirteenclubweave",
                }
            );
        records.Select(record => record.Id).Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void EverySourceRowMatchesItsTargetAndFeasibleThrowHeight()
    {
        var records = LoadRecords();

        foreach (var record in records)
        {
            record.SourceRows.Should().NotBeNull(record.Id);
            var idByName = record.Jugglers.ToDictionary(item => item.Name, item => item.Id);
            var throwById = record.Throws.ToDictionary(item => item.Id, StringComparer.Ordinal);
            var edgeBySource = record.CausalEdges.ToDictionary(
                item => item.From,
                StringComparer.Ordinal
            );

            record.SourceLinks.ValueKind.Should().Be(JsonValueKind.Array, record.Id);
            record
                .SourceLinks.EnumerateArray()
                .Select(item => item.GetProperty("edition").GetString())
                .Should()
                .Contain(item => item!.Contains("2017", StringComparison.Ordinal))
                .And.Contain(item => item!.Contains("2014", StringComparison.Ordinal));

            foreach (var (name, row) in record.SourceRows!)
            {
                var jugglerId = idByName[name];
                var events = record.Throws.Where(item => item.JugglerId == jugglerId).ToArray();
                events.Should().HaveCount((int)record.CycleLength, $"{record.Id} {name}");

                foreach (var source in events)
                {
                    var sequenceSuffix = string.Concat(
                        source.Id.Reverse().TakeWhile(char.IsDigit).Reverse()
                    );
                    var sequenceIndex = int.Parse(sequenceSuffix, CultureInfo.InvariantCulture) - 1;
                    var token = row[sequenceIndex % row.Length];
                    var targetName =
                        token is "self" or "zip" or "heff" ? name
                        : token.Contains(':', StringComparison.Ordinal) ? token.Split(':')[1]
                        : token;
                    var target = throwById[edgeBySource[source.Id].To];
                    target.JugglerId.Should().Be(idByName[targetName], $"{record.Id} {source.Id}");

                    var height =
                        target.StartTime
                        + edgeBySource[source.Id].CycleOffset * record.CycleLength
                        - source.StartTime;
                    var action = token is "self" or "zip" or "heff" ? token : "pass";

                    if (record.SourceActionHeights is not null)
                    {
                        height
                            .Should()
                            .Be(record.SourceActionHeights[action], $"{record.Id} {source.Id}");
                    }
                    else
                    {
                        var minimums = record.SourceMinimumActionHeights!;
                        var minimum = minimums[action];
                        if (action == "zip")
                            height
                                .Should()
                                .BeLessThanOrEqualTo(minimum, $"{record.Id} {source.Id}");
                        else
                            height
                                .Should()
                                .BeGreaterThanOrEqualTo(minimum, $"{record.Id} {source.Id}");
                    }
                }
            }
        }
    }

    [Test]
    public void LineFeedRowsMatchCausalTargetsHandsAndArrivalOffsets()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-transcriptions-316-330.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();
        var records = JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-316-330.json"
                )
            ),
            JsonOptions
        )!;

        foreach (var record in records.Where(item => item.PassDirectionByJuggler is not null))
        {
            var idByName = record.Jugglers.ToDictionary(item => item.Name, item => item.Id);
            var throwById = record.Throws.ToDictionary(item => item.Id, StringComparer.Ordinal);
            var edgeBySource = record.CausalEdges.ToDictionary(
                item => item.From,
                StringComparer.Ordinal
            );
            foreach (var (name, row) in record.SourceRows!)
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
                    var token = row[index % row.Length];
                    var parts = token.Split(':');
                    var targetName =
                        token is "self" or "zip" or "heff" ? name
                        : parts.Length > 1 ? parts[1]
                        : token;
                    var targetJuggler = idByName[targetName];
                    var target = throwById[edgeBySource[source.Id].To];
                    target.JugglerId.Should().Be(targetJuggler, $"{record.Id} {source.Id}");

                    var route = token switch
                    {
                        "self" or "zip" => "opposite",
                        "heff" => "same",
                        _ => parts.Length == 3 ? parts[2] : record.PassDirectionByJuggler![name],
                    };
                    var targetHand = route switch
                    {
                        "same" or "crossing" => source.Hand,
                        "opposite" or "straight" => source.Hand == "R" ? "L" : "R",
                        _ => throw new AssertionException(
                            $"Unknown route '{route}' in {record.Id}"
                        ),
                    };
                    target.Hand.Should().Be(targetHand, $"{record.Id} {source.Id}");

                    var minimumDelta = token switch
                    {
                        "zip" => -1m,
                        "heff" => 2m,
                        "self" => 1m,
                        _ when parts[0] == "double" => 2m,
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
                        .Where(item => item.Throw.Hand == targetHand)
                        .Where(item =>
                            token == "zip" ? item.Time <= threshold : item.Time >= threshold
                        )
                        .ToArray();
                    var expected =
                        token == "zip"
                            ? candidates.OrderByDescending(item => item.Time).First()
                            : candidates.OrderBy(item => item.Time).First();
                    target.Id.Should().Be(expected.Throw.Id, $"{record.Id} {source.Id} endpoint");
                    edgeBySource[source.Id]
                        .CycleOffset.Should()
                        .Be(expected.Offset, $"{record.Id} {source.Id} offset");
                }
            }
        }

        records.Where(item => item.PassDirectionByJuggler is not null).Should().HaveCount(6);
    }

    private static List<Record> LoadRecords()
    {
        var repository = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (
            repository is not null
            && !File.Exists(
                Path.Combine(
                    repository.FullName,
                    "docs/research/highgate-transcriptions-316-330.json"
                )
            )
        )
            repository = repository.Parent;
        repository.Should().NotBeNull();
        return JsonSerializer.Deserialize<List<Record>>(
            File.ReadAllText(
                Path.Combine(
                    repository!.FullName,
                    "docs/research/highgate-transcriptions-316-330.json"
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
        Dictionary<string, string[]>? SourceRows = null,
        Dictionary<string, string>? PassDirectionByJuggler = null,
        Dictionary<string, decimal>? SourceActionHeights = null,
        Dictionary<string, decimal>? SourceMinimumActionHeights = null,
        JsonElement SourceLinks = default
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
