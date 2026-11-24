using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Siteswaps.Components.CausalDiagram;

public sealed class CausalDiagramEditorState
{
    private const string CompactTokenPrefix = "j3.";
    private const string PreviousCompactTokenPrefix = "j2.";
    private const string OldCompactTokenPrefix = "j1.";
    private const int MaximumTokenLength = 300_000;
    private const int MaximumDecodedPayloadBytes = 220_000;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private sealed record FeedingRow(
        string Name,
        string? HandSequence,
        decimal Offset,
        List<(int Height, string Target)> Throws
    );

    public int Version { get; set; } = 3;
    public string PresetId { get; set; } = "custom";
    public string ActiveJugglerId { get; set; } = "A";
    public decimal CycleLength { get; set; } = 7m;
    public int Repetitions { get; set; } = 2;
    public bool SnapEnabled { get; set; } = true;
    public decimal SnapStep { get; set; } = 0.25m;
    public decimal TimelineWidth { get; set; } = 560m;
    public decimal LaneSpacing { get; set; } = 105m;
    public List<EditorJuggler> Jugglers { get; set; } = [];
    public List<EditorThrow> Throws { get; set; } = [];
    public List<EditorCausalEdge> CausalEdges { get; set; } = [];

    public static CausalDiagramEditorState CreateSiteswap531() =>
        CreateSiteswapPattern("531", "siteswap-531");

    public static CausalDiagramEditorState CreateSiteswapPattern(
        string notation,
        string presetId = "custom"
    ) =>
        TryCreateSiteswap(notation, presetId, out var state) && state is not null
            ? state
            : throw new ArgumentException($"Ungültiger Siteswap: {notation}", nameof(notation));

    public static CausalDiagramEditorState CreateFeeding75() =>
        CreateFeedingPattern(7, 5, "feeding-7-5");

    public static CausalDiagramEditorState CreateFeedingPattern(
        int firstCount,
        int secondCount,
        string presetId
    )
    {
        if (firstCount is < 1 or > 64 || secondCount is < 1 or > 64)
            throw new ArgumentOutOfRangeException(nameof(firstCount));
        var first = string.Join(", ", Enumerable.Repeat("1>A.R", firstCount));
        var second = string.Join(", ", Enumerable.Repeat("1>B.R", secondCount));
        var notation = $"A: {first}\nB: {second}";
        if (!TryCreateFeeding(notation, out var state, out _) || state is null)
            throw new InvalidOperationException("The feeding preset could not be created.");
        state.PresetId = presetId;
        return state;
    }

    public static CausalDiagramEditorState CreatePassingPattern(
        IReadOnlyList<int> timeZones,
        IReadOnlyList<IReadOnlyList<int>> heights,
        IReadOnlyList<IReadOnlyList<int>> targets
    )
    {
        var diagram = Siteswap.Details.CausalDiagram.PassingDiagramBuilder.Build(
            timeZones,
            heights,
            targets
        );
        var jugglers = Enumerable
            .Range(0, timeZones.Count)
            .Select(index => new EditorJuggler($"P{index + 1}", ((char)('A' + index)).ToString()))
            .ToList();
        var cycleLength = diagram.StepCount;
        var throws = diagram
            .Throws.Select(
                (node, index) =>
                    new EditorThrow(
                        $"P{jugglers.FindIndex(juggler => juggler.Name == node.Hand.Person.Name) + 1}-{index + 1}",
                        $"P{jugglers.FindIndex(juggler => juggler.Name == node.Hand.Person.Name) + 1}",
                        node.Hand.Name,
                        node.Time,
                        node.Height
                    )
            )
            .ToList();

        var edges = new List<EditorCausalEdge>();
        foreach (var source in diagram.CausalArrows)
        {
            var sourceId = FindThrowId(source.From, diagram.Throws, throws);
            if (sourceId is null)
                continue;
            var targetPerson = Array.FindIndex(
                jugglers.ToArray(),
                juggler => juggler.Name == source.To.Hand.Person.Name
            );
            if (targetPerson < 0)
                continue;
            var targetJugglerId = $"P{targetPerson + 1}";
            var target = diagram
                .Throws.Select(
                    (node, index) =>
                        new
                        {
                            Throw = node,
                            EditorThrow = throws[index],
                            Cycle = (int)
                                Math.Round(
                                    (source.To.Time - node.Time) / cycleLength,
                                    MidpointRounding.AwayFromZero
                                ),
                        }
                )
                .Where(candidate =>
                    candidate.EditorThrow.JugglerId == targetJugglerId
                    && candidate.Throw.Hand.Name == source.To.Hand.Name
                )
                .Select(candidate => new
                {
                    candidate.EditorThrow,
                    candidate.Cycle,
                    Distance = Math.Abs(
                        candidate.Throw.Time + candidate.Cycle * cycleLength - source.To.Time
                    ),
                })
                .OrderBy(candidate => candidate.Distance)
                .FirstOrDefault();
            if (target is not null)
                edges.Add(
                    new EditorCausalEdge(
                        $"e-{sourceId}",
                        sourceId,
                        target.EditorThrow.Id,
                        target.Cycle
                    )
                );
        }

        return new CausalDiagramEditorState
        {
            PresetId = "custom",
            ActiveJugglerId = jugglers[0].Id,
            CycleLength = cycleLength,
            Jugglers = jugglers,
            Throws = throws,
            CausalEdges = edges,
        };
    }

    private static string? FindThrowId(
        Siteswap.Details.CausalDiagram.Throw target,
        System.Collections.Immutable.ImmutableList<Siteswap.Details.CausalDiagram.Throw> sourceThrows,
        List<EditorThrow> editorThrows
    )
    {
        for (var index = 0; index < sourceThrows.Count; index++)
            if (sourceThrows[index] == target)
                return editorThrows[index].Id;
        return null;
    }

    public static CausalDiagramEditorState CreateEmpty() =>
        new()
        {
            PresetId = "custom",
            ActiveJugglerId = "A",
            CycleLength = 7m,
            Jugglers = [new EditorJuggler("A", "Jongleur A")],
        };

    public static bool TryCreateSiteswap(
        string notation,
        string presetId,
        out CausalDiagramEditorState? state
    )
    {
        state = null;
        if (string.IsNullOrWhiteSpace(notation) || notation.Length > 64)
            return false;
        var normalizedNotation = notation.Trim();
        if (
            normalizedNotation.Length is < 1 or > 64
            || normalizedNotation.Any(c => !(c is >= '0' and <= '9' or >= 'a' and <= 'z'))
            || !Siteswap.Details.Siteswap.TryCreate(normalizedNotation, out var siteswap)
            || siteswap is null
            || siteswap.Length is < 1 or > 64
        )
            return false;

        var handPeriod =
            siteswap.Length % 2 == 0
            || Enumerable.Range(0, siteswap.Length).All(beat => siteswap.Items[beat] == 0)
                ? siteswap.Length
                : siteswap.Length * 2;
        var nodes = new List<EditorThrow>(handPeriod);
        for (var beat = 0; beat < handPeriod; beat++)
        {
            var height = siteswap.Items[beat % siteswap.Length];
            nodes.Add(
                new EditorThrow($"A{beat + 1}", "A", beat % 2 == 0 ? "R" : "L", beat, height)
            );
        }

        state = new CausalDiagramEditorState
        {
            PresetId = presetId,
            CycleLength = handPeriod,
            ActiveJugglerId = "A",
            Jugglers = [new EditorJuggler("A", "Jongleur A")],
            Throws = nodes,
            CausalEdges = SiteswapEdges(nodes, handPeriod, numberOfHands: 2),
        };
        state.SynchronizeHeights();
        return true;
    }

    private static List<EditorCausalEdge> SiteswapEdges(
        IReadOnlyList<EditorThrow> nodes,
        decimal period,
        int numberOfHands
    )
    {
        var byTime = nodes.ToDictionary(node => (int)node.StartTime);
        var edges = new List<EditorCausalEdge>();
        foreach (var node in nodes)
        {
            if (node.Height == 0)
                continue;
            var absoluteBeat = (int)node.StartTime + (int)node.Height - numberOfHands;
            var targetBeat = ((absoluteBeat % (int)period) + (int)period) % (int)period;
            var cycleOffset = (int)Math.Floor((double)absoluteBeat / (double)period);
            if (byTime.TryGetValue(targetBeat, out var target))
                edges.Add(new EditorCausalEdge($"e-{node.Id}", node.Id, target.Id, cycleOffset));
        }
        return edges;
    }

    public static bool TryCreateFeeding(
        string notation,
        out CausalDiagramEditorState? state,
        out string error
    )
    {
        state = null;
        error = "Eine Zeile pro Jongleur, z. B. A: 3>B, 3>A und in der nächsten Zeile B: 3>A, 3>B.";
        if (string.IsNullOrWhiteSpace(notation) || notation.Length > 8_000)
            return false;
        if (!TryParseFeedingRows(notation, out var rows) || !AreFeedingRowsValid(rows))
            return false;

        var cycleLength = rows.Select(row => row.Throws.Count).Aggregate(1, LeastCommonMultiple);
        if (rows.Sum(row => row.Throws.Count) > 500)
        {
            error = "Das gemeinsame Wiederholungsmuster benötigt mehr als 500 Knoten.";
            return false;
        }

        state = CreateFeedingEditorState(rows, cycleLength);
        error = string.Empty;
        return true;
    }

    private static bool TryParseFeedingRows(string notation, out List<FeedingRow> rows)
    {
        rows = [];
        foreach (
            var line in notation.Split(
                '\n',
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
            )
        )
        {
            if (!TryParseFeedingRow(line, rows, out var row))
                return false;
            rows.Add(row);
        }

        return rows.Count is >= 1 and <= 16;
    }

    private static bool TryParseFeedingRow(
        string line,
        IReadOnlyList<FeedingRow> existingRows,
        out FeedingRow row
    )
    {
        row = new FeedingRow(string.Empty, null, 0m, []);
        var separator = line.IndexOf(':');
        if (separator < 1)
            return false;

        var name = line[..separator].Trim();
        var pattern = line[(separator + 1)..].TrimStart();
        if (
            name.Length > 32
            || name.Any(char.IsControl)
            || existingRows.Any(existing =>
                string.Equals(existing.Name, name, StringComparison.OrdinalIgnoreCase)
            )
        )
            return false;

        string? handSequence = null;
        var offset = 0m;
        if (pattern.StartsWith('('))
        {
            var close = pattern.IndexOf(')');
            if (close < 2)
                return false;
            var handSpec = pattern[1..close].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            handSequence = handSpec[0].ToUpperInvariant();
            if (
                handSequence.Length is < 1 or > 16
                || handSequence.Any(hand => hand is not ('R' or 'L'))
                || handSpec.Length > 2
                || handSpec.Length == 2
                    && (
                        !decimal.TryParse(
                            handSpec[1],
                            NumberStyles.AllowDecimalPoint,
                            CultureInfo.InvariantCulture,
                            out offset
                    ) || offset is < 0m or > 1m
                    )
            )
                return false;
            pattern = pattern[(close + 1)..].TrimStart();
        }

        var throws = new List<(int Height, string Target)>();
        foreach (
            var token in pattern.Split(
                ',',
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
            )
        )
        {
            if (!TryParseFeedingThrow(token, out var parsedThrow))
                return false;
            throws.Add(parsedThrow);
        }
        if (throws.Count is < 1 or > 64)
            return false;

        row = new FeedingRow(name, handSequence, offset, throws);
        return true;
    }

    private static bool TryParseFeedingThrow(
        string token,
        out (int Height, string Target) parsedThrow
    )
    {
        parsedThrow = default;
        var arrow = token.IndexOf('>');
        if (
            arrow < 1
            || !int.TryParse(
                token[..arrow],
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var height
            )
            || height is < 1 or > 100
        )
            return false;

        var target = token[(arrow + 1)..].Trim();
        if (
            target.EndsWith(".R", StringComparison.OrdinalIgnoreCase)
            || target.EndsWith(".L", StringComparison.OrdinalIgnoreCase)
        )
            target = target[..^2];
        if (string.IsNullOrWhiteSpace(target) || target.Length > 32)
            return false;

        parsedThrow = (height, target);
        return true;
    }

    private static bool AreFeedingRowsValid(IReadOnlyList<FeedingRow> rows) =>
        rows.Sum(row => row.Throws.Count) <= 500
        && rows.All(row =>
            row.Throws.All(item =>
                rows.Any(target =>
                    string.Equals(target.Name, item.Target, StringComparison.OrdinalIgnoreCase)
                )
            )
        );

    private static CausalDiagramEditorState CreateFeedingEditorState(
        List<FeedingRow> rows,
        int cycleLength
    )
    {
        var jugglers = rows.Select((row, index) => new EditorJuggler($"P{index + 1}", row.Name))
            .ToList();
        var byName = rows.Select((row, index) => (row.Name, Juggler: jugglers[index]))
            .ToDictionary(
                item => item.Name,
                item => item.Juggler,
                StringComparer.OrdinalIgnoreCase
            );
        var nodes = new List<EditorThrow>();
        for (var personIndex = 0; personIndex < rows.Count; personIndex++)
        {
            var row = rows[personIndex];
            var beatDuration = cycleLength / row.Throws.Count;
            for (var beat = 0; beat < row.Throws.Count; beat++)
            {
                nodes.Add(
                    new EditorThrow(
                        $"P{personIndex + 1}-{beat + 1}",
                        jugglers[personIndex].Id,
                        row.HandSequence is null
                            ? beat % 2 == 0
                                ? "R"
                                : "L"
                            : row.HandSequence[beat % row.HandSequence.Length].ToString(),
                        (beat * beatDuration + row.Offset * beatDuration) % cycleLength,
                        row.Throws[beat].Height
                    )
                );
            }
        }

        var edges = CreateFeedingEdges(rows, byName, nodes, cycleLength);
        var state = new CausalDiagramEditorState
        {
            PresetId = "custom",
            CycleLength = cycleLength,
            ActiveJugglerId = jugglers[0].Id,
            Jugglers = jugglers,
            Throws = nodes,
            CausalEdges = edges,
        };
        state.SynchronizeHeights();
        return state;
    }

    private static List<EditorCausalEdge> CreateFeedingEdges(
        List<FeedingRow> rows,
        Dictionary<string, EditorJuggler> byName,
        IReadOnlyList<EditorThrow> nodes,
        int cycleLength
    )
    {
        var edges = new List<EditorCausalEdge>();
        for (var personIndex = 0; personIndex < rows.Count; personIndex++)
        {
            var row = rows[personIndex];
            var sourceBeatDuration = cycleLength / row.Throws.Count;
            for (var beat = 0; beat < row.Throws.Count; beat++)
            {
                var source = nodes.First(node => node.Id == $"P{personIndex + 1}-{beat + 1}");
                var targetId = byName[row.Throws[beat].Target].Id;
                var targetBeatDuration =
                    cycleLength
                    / rows.First(targetRow =>
                        string.Equals(
                            targetRow.Name,
                            row.Throws[beat].Target,
                            StringComparison.OrdinalIgnoreCase
                        )
                    ).Throws.Count;
                var causalTime =
                    source.StartTime
                    + row.Throws[beat].Height * sourceBeatDuration
                    - 2m * targetBeatDuration;
                var (target, cycleOffset) = NearestUnusedCyclicTarget(
                    nodes.Where(node => node.JugglerId == targetId),
                    causalTime,
                    cycleLength,
                    edges.Select(edge => (edge.ToThrowId, edge.CycleOffset)).ToHashSet()
                );
                if (target is null)
                    continue;
                edges.Add(
                    new EditorCausalEdge($"feed-{source.Id}", source.Id, target.Id, cycleOffset)
                );
            }
        }

        return edges;
    }

    private static int LeastCommonMultiple(int left, int right)
    {
        var a = left;
        var b = right;
        while (b != 0)
            (a, b) = (b, a % b);
        return checked(left / a * right);
    }

    private static (EditorThrow? Target, int CycleOffset) NearestUnusedCyclicTarget(
        IEnumerable<EditorThrow> candidates,
        decimal time,
        int cycleLength,
        HashSet<(string Id, int CycleOffset)> occupied
    )
    {
        return candidates
            .SelectMany(node =>
            {
                var nearest = (int)
                    Math.Round(
                        (time - node.StartTime) / cycleLength,
                        MidpointRounding.AwayFromZero
                    );
                return Enumerable
                    .Range(nearest - 250, 501)
                    .Select(offset =>
                        (
                            Target: node,
                            CycleOffset: offset,
                            Distance: Math.Abs(node.StartTime + offset * cycleLength - time)
                        )
                    );
            })
            .Where(candidate => !occupied.Contains((candidate.Target.Id, candidate.CycleOffset)))
            .OrderBy(candidate => candidate.Distance)
            .ThenBy(candidate => candidate.CycleOffset)
            .Select(candidate => ((EditorThrow?)candidate.Target, candidate.CycleOffset))
            .FirstOrDefault();
    }

    private static (EditorThrow Target, int CycleOffset) NearestCyclicTarget(
        IEnumerable<EditorThrow> candidates,
        decimal time,
        int cycleLength
    )
    {
        return candidates
            .Select(node =>
            {
                var cycleOffset = (int)
                    Math.Round(
                        (time - node.StartTime) / cycleLength,
                        MidpointRounding.AwayFromZero
                    );
                var distance = Math.Abs(node.StartTime + cycleOffset * cycleLength - time);
                return (Target: node, CycleOffset: cycleOffset, Distance: distance);
            })
            .OrderBy(candidate => candidate.Distance)
            .ThenBy(candidate => candidate.CycleOffset)
            .Select(candidate => (candidate.Target, candidate.CycleOffset))
            .First();
    }

    public string Encode()
    {
        SynchronizeHeights();
        using var output = new MemoryStream();
        using (var writer = new Utf8JsonWriter(output))
        {
            writer.WriteStartObject();
            writer.WriteNumber("v", Version);
            writer.WriteString("p", PresetId);
            writer.WriteString("a", ActiveJugglerId);
            writer.WriteNumber("c", CycleLength);
            writer.WriteNumber("r", Repetitions);
            writer.WriteBoolean("s", SnapEnabled);
            writer.WriteNumber("q", SnapStep);
            writer.WriteNumber("w", TimelineWidth);
            writer.WriteNumber("l", LaneSpacing);
            writer.WriteStartArray("j");
            foreach (var juggler in Jugglers)
            {
                writer.WriteStartArray();
                writer.WriteStringValue(juggler.Id);
                writer.WriteStringValue(juggler.Name);
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
            writer.WriteStartArray("t");
            foreach (var item in Throws)
            {
                writer.WriteStartArray();
                writer.WriteStringValue(item.Id);
                writer.WriteStringValue(item.JugglerId);
                writer.WriteStringValue(item.Hand);
                writer.WriteNumberValue(item.StartTime);
                writer.WriteNumberValue(item.Height);
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
            writer.WriteStartArray("e");
            foreach (var edge in CausalEdges)
            {
                writer.WriteStartArray();
                writer.WriteStringValue(edge.Id);
                writer.WriteStringValue(edge.FromThrowId);
                writer.WriteStringValue(edge.ToThrowId);
                writer.WriteNumberValue(edge.CycleOffset);
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        return CompactTokenPrefix + ToBase64Url(output.ToArray());
    }

    public static bool TryDecode(string? token, out CausalDiagramEditorState? state)
    {
        state = null;
        if (string.IsNullOrWhiteSpace(token) || token.Length > MaximumTokenLength)
            return false;
        try
        {
            CausalDiagramEditorState? decoded;
            if (token.StartsWith(CompactTokenPrefix, StringComparison.Ordinal))
                decoded = DeserializeCompact(
                    DecodePayload(token[CompactTokenPrefix.Length..]),
                    expectedVersion: 3
                );
            else if (token.StartsWith(PreviousCompactTokenPrefix, StringComparison.Ordinal))
            {
                decoded = DeserializeCompact(
                    DecodePayload(token[PreviousCompactTokenPrefix.Length..]),
                    expectedVersion: 2
                );
                decoded.Version = 3;
                decoded.NormalizeCausalEdges();
            }
            else if (token.StartsWith(OldCompactTokenPrefix, StringComparison.Ordinal))
                decoded = MigrateLegacy(
                    DeserializeV1(DecodePayload(token[OldCompactTokenPrefix.Length..]))
                );
            else
            {
                if (token.Contains('.', StringComparison.Ordinal))
                    return false;
                var json = DecodePayload(token);
                var legacy = JsonSerializer.Deserialize<LegacyState>(StrictUtf8.GetString(json));
                decoded = legacy is null ? null : MigrateLegacy(legacy);
            }
            if (decoded is not null)
            {
                decoded.PresetId = decoded.PresetId switch
                {
                    "siteswap531" => "siteswap-531",
                    "feeding75" => "feeding-7-5",
                    _ => decoded.PresetId,
                };
                MigrateLegacy75Heights(decoded);
                decoded.SynchronizeHeights();
            }
            if (!IsValid(decoded))
                return false;
            state = decoded;
            return true;
        }
        catch (Exception exception)
            when (exception
                    is FormatException
                        or JsonException
                        or ArgumentException
                        or DecoderFallbackException
                        or OverflowException
                        or InvalidOperationException
            )
        {
            return false;
        }
    }

    private static byte[] DecodePayload(string token)
    {
        var bytes = FromBase64Url(token);
        if (bytes.Length > MaximumDecodedPayloadBytes)
            throw new FormatException("The payload exceeds the limit.");
        return bytes;
    }

    private static CausalDiagramEditorState DeserializeCompact(byte[] json, int expectedVersion)
    {
        using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 8 });
        var root = doc.RootElement;
        if (
            root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("v", out var version)
            || !version.TryGetInt32(out var v)
            || v != expectedVersion
            || !root.TryGetProperty("p", out var preset)
            || preset.ValueKind != JsonValueKind.String
            || !root.TryGetProperty("a", out var active)
            || active.ValueKind != JsonValueKind.String
            || !root.TryGetProperty("c", out var cycle)
            || !cycle.TryGetDecimal(out var c)
            || !root.TryGetProperty("r", out var reps)
            || !reps.TryGetInt32(out var repetitions)
            || !root.TryGetProperty("s", out var snap)
            || snap.ValueKind is not (JsonValueKind.True or JsonValueKind.False)
            || !root.TryGetProperty("q", out var step)
            || !step.TryGetDecimal(out var snapStep)
            || !root.TryGetProperty("j", out var jugglers)
            || jugglers.ValueKind != JsonValueKind.Array
            || jugglers.GetArrayLength() > 16
            || !root.TryGetProperty("t", out var throws)
            || throws.ValueKind != JsonValueKind.Array
            || throws.GetArrayLength() > 500
            || !root.TryGetProperty("e", out var edges)
            || edges.ValueKind != JsonValueKind.Array
            || edges.GetArrayLength() > 2_000
        )
            throw new JsonException("Invalid editor state shape.");
        var state = new CausalDiagramEditorState
        {
            Version = v,
            PresetId = preset.GetString() ?? throw new JsonException(),
            ActiveJugglerId = active.GetString() ?? throw new JsonException(),
            CycleLength = c,
            Repetitions = repetitions,
            SnapEnabled = snap.GetBoolean(),
            SnapStep = snapStep,
            TimelineWidth = ReadOptionalDecimal(root, "w", 560m),
            LaneSpacing = ReadOptionalDecimal(root, "l", 105m),
            Jugglers = [],
            Throws = [],
            CausalEdges = [],
        };
        foreach (var item in jugglers.EnumerateArray())
        {
            if (
                item.ValueKind != JsonValueKind.Array
                || item.GetArrayLength() != 2
                || item[0].ValueKind != JsonValueKind.String
                || item[1].ValueKind != JsonValueKind.String
            )
                throw new JsonException();
            state.Jugglers.Add(
                new EditorJuggler(
                    item[0].GetString() ?? throw new JsonException(),
                    item[1].GetString() ?? throw new JsonException()
                )
            );
        }
        foreach (var item in throws.EnumerateArray())
        {
            if (
                item.ValueKind != JsonValueKind.Array
                || item.GetArrayLength() != 5
                || item[0].ValueKind != JsonValueKind.String
                || item[1].ValueKind != JsonValueKind.String
                || item[2].ValueKind != JsonValueKind.String
                || !item[3].TryGetDecimal(out var start)
                || !item[4].TryGetDecimal(out var height)
            )
                throw new JsonException();
            state.Throws.Add(
                new EditorThrow(
                    item[0].GetString() ?? throw new JsonException(),
                    item[1].GetString() ?? throw new JsonException(),
                    item[2].GetString() ?? throw new JsonException(),
                    start,
                    height
                )
            );
        }
        foreach (var item in edges.EnumerateArray())
        {
            if (
                item.ValueKind != JsonValueKind.Array
                || item.GetArrayLength() != 4
                || item[0].ValueKind != JsonValueKind.String
                || item[1].ValueKind != JsonValueKind.String
                || item[2].ValueKind != JsonValueKind.String
                || !item[3].TryGetInt32(out var offset)
            )
                throw new JsonException();
            state.CausalEdges.Add(
                new EditorCausalEdge(
                    item[0].GetString() ?? throw new JsonException(),
                    item[1].GetString() ?? throw new JsonException(),
                    item[2].GetString() ?? throw new JsonException(),
                    offset
                )
            );
        }
        return state;
    }

    private static LegacyState DeserializeV1(byte[] json)
    {
        using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 8 });
        var root = doc.RootElement;
        if (
            root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("v", out var version)
            || !version.TryGetInt32(out var v)
            || v != 1
            || !root.TryGetProperty("p", out var preset)
            || !root.TryGetProperty("c", out var cycle)
            || !cycle.TryGetDecimal(out var c)
            || !root.TryGetProperty("r", out var reps)
            || !reps.TryGetInt32(out var repetitions)
            || !root.TryGetProperty("s", out var snap)
            || !root.TryGetProperty("q", out var step)
            || !step.TryGetDecimal(out var snapStep)
            || !root.TryGetProperty("j", out var jugglers)
            || !root.TryGetProperty("t", out var throws)
        )
            throw new JsonException("Invalid legacy editor state.");
        var legacy = new LegacyState
        {
            Version = v,
            PresetId = preset.GetString() ?? throw new JsonException(),
            ActiveJugglerId = root.TryGetProperty("a", out var active)
                ? active.GetString() ?? throw new JsonException()
                : "A",
            CycleLength = c,
            Repetitions = repetitions,
            SnapEnabled = snap.GetBoolean(),
            SnapStep = snapStep,
            Jugglers = [],
            Throws = [],
        };
        foreach (var item in jugglers.EnumerateArray())
            legacy.Jugglers.Add(
                new LegacyJuggler(
                    item[0].GetString() ?? throw new JsonException(),
                    item[1].GetString() ?? throw new JsonException()
                )
            );
        foreach (var item in throws.EnumerateArray())
        {
            if (
                item.ValueKind != JsonValueKind.Array
                || item.GetArrayLength() is not (6 or 7)
                || !item[3].TryGetDecimal(out var start)
                || !item[4].TryGetDecimal(out var end)
            )
                throw new JsonException();
            legacy.Throws.Add(
                new LegacyThrow
                {
                    Id = item[0].GetString() ?? throw new JsonException(),
                    JugglerId = item[1].GetString() ?? throw new JsonException(),
                    Hand = item[2].GetString() ?? throw new JsonException(),
                    StartTime = start,
                    LandingTime = end,
                    TargetJugglerId = item[5].GetString() ?? throw new JsonException(),
                    TargetHand =
                        item.GetArrayLength() == 7
                            ? item[6].GetString() ?? throw new JsonException()
                            : "R",
                }
            );
        }
        return legacy;
    }

    private static CausalDiagramEditorState MigrateLegacy(LegacyState legacy)
    {
        var jugglers = legacy
            .Jugglers.Select(item => new EditorJuggler(item.Id, item.Name))
            .ToList();
        var nodes = legacy
            .Throws.Select(item => new EditorThrow(
                item.Id,
                item.JugglerId,
                item.Hand,
                item.StartTime,
                Math.Max(0.1m, item.LandingTime - item.StartTime)
            ))
            .ToList();
        var edges = new List<EditorCausalEdge>();
        foreach (var item in legacy.Throws)
        {
            var targetNodes = nodes.Where(node => node.JugglerId == item.TargetJugglerId).ToList();
            if (targetNodes.Count == 0)
                continue;
            var targetTime = item.LandingTime - Math.Max(1, jugglers.Count * 2);
            var (target, offset) = NearestCyclicTarget(
                targetNodes,
                targetTime,
                Math.Max(1, (int)legacy.CycleLength)
            );
            edges.Add(new EditorCausalEdge($"migrated-{item.Id}", item.Id, target.Id, offset));
        }
        var activeId = jugglers.Any(item => item.Id == legacy.ActiveJugglerId)
            ? legacy.ActiveJugglerId
            : jugglers.FirstOrDefault()?.Id ?? "";
        var migrated = new CausalDiagramEditorState
        {
            Version = 3,
            PresetId = legacy.PresetId,
            ActiveJugglerId = activeId,
            CycleLength = legacy.CycleLength,
            Repetitions = legacy.Repetitions,
            SnapEnabled = legacy.SnapEnabled,
            SnapStep = legacy.SnapStep,
            Jugglers = jugglers,
            Throws = nodes,
            CausalEdges = edges,
        };
        migrated.NormalizeCausalEdges();
        migrated.SynchronizeHeights();
        return migrated;
    }

    private static bool IsValid(CausalDiagramEditorState? state)
    {
        if (
            state is null
            || state.Version != 3
            || state.Jugglers is null
            || state.Throws is null
            || state.CausalEdges is null
            || !CausalDiagramEditorPresets.IsKnownId(state.PresetId)
            || state.CycleLength <= 0
            || state.CycleLength > 10_000
            || state.Repetitions is < 1 or > 20
            || state.SnapStep < 0.001m
            || state.SnapStep > 1_000
            || state.TimelineWidth is < 280m or > 1_120m
            || state.LaneSpacing is < 80m or > 180m
            || state.Jugglers.Count is < 1 or > 16
            || state.Throws.Count > 500
            || state.CausalEdges.Count > 2_000
        )
            return false;
        if (
            state.Jugglers.Any(item =>
                item is null || !ValidText(item.Id, 64) || !ValidText(item.Name, 128)
            )
        )
            return false;
        var jugglerIds = state.Jugglers.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        if (jugglerIds.Count != state.Jugglers.Count || !jugglerIds.Contains(state.ActiveJugglerId))
            return false;
        if (
            state.Throws.Any(item =>
                item is null
                || !ValidText(item.Id, 64)
                || !jugglerIds.Contains(item.JugglerId)
                || item.Hand is not ("R" or "L")
                || item.StartTime < 0
                || item.StartTime >= state.CycleLength
                || item.Height < 0
                || item.Height > 10_000
            )
        )
            return false;
        var throwIds = state.Throws.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        if (throwIds.Count != state.Throws.Count)
            return false;
        if (
            state.CausalEdges.Any(edge =>
                edge is null
                || !ValidText(edge.Id, 96)
                || !throwIds.Contains(edge.FromThrowId)
                || !throwIds.Contains(edge.ToThrowId)
                || edge.CycleOffset is < -1_000 or > 1_000
            )
        )
            return false;
        if (
            state.CausalEdges.Select(edge => edge.Id).Distinct(StringComparer.Ordinal).Count()
                != state.CausalEdges.Count
            || state
                .CausalEdges.Select(edge => edge.FromThrowId)
                .Distinct(StringComparer.Ordinal)
                .Count() != state.CausalEdges.Count
        )
            return false;
        return state.CausalEdges.All(edge =>
        {
            var source = state.Throws.First(node => node.Id == edge.FromThrowId);
            var target = state.Throws.First(node => node.Id == edge.ToThrowId);
            return source.Height >= 0
                && source.Height == DerivedHeight(state, source, target, edge.CycleOffset);
        });
    }

    public void SynchronizeHeights()
    {
        for (var edgeIndex = 0; edgeIndex < CausalEdges.Count; edgeIndex++)
        {
            var edge = CausalEdges[edgeIndex];
            var sourceIndex = Throws.FindIndex(node => node.Id == edge.FromThrowId);
            var target = Throws.FirstOrDefault(node => node.Id == edge.ToThrowId);
            if (sourceIndex < 0 || target is null)
                continue;
            var source = Throws[sourceIndex];
            var height = DerivedHeight(this, source, target, edge.CycleOffset);
            while (height < 0 && edge.CycleOffset < 1_000)
            {
                edge = edge with { CycleOffset = edge.CycleOffset + 1 };
                height = DerivedHeight(this, source, target, edge.CycleOffset);
            }
            CausalEdges[edgeIndex] = edge;
            Throws[sourceIndex] = source with { Height = height };
        }
    }

    private void NormalizeCausalEdges()
    {
        if (CausalEdges is null || Throws is null)
            return;
        var throwIds = Throws
            .Where(node => node is not null)
            .Select(node => node.Id)
            .ToHashSet(StringComparer.Ordinal);
        var sources = new HashSet<string>(StringComparer.Ordinal);
        var edgeIds = new HashSet<string>(StringComparer.Ordinal);
        var normalized = new List<EditorCausalEdge>();
        foreach (var edge in CausalEdges)
        {
            if (
                edge is null
                || !throwIds.Contains(edge.FromThrowId)
                || !throwIds.Contains(edge.ToThrowId)
                || edge.CycleOffset is < -1_000 or > 1_000
                || !ValidText(edge.Id, 96)
                || edgeIds.Contains(edge.Id)
                || sources.Contains(edge.FromThrowId)
            )
                continue;
            edgeIds.Add(edge.Id);
            sources.Add(edge.FromThrowId);
            normalized.Add(edge);
        }
        CausalEdges = normalized;
    }

    private static void MigrateLegacy75Heights(CausalDiagramEditorState state)
    {
        if (
            state.CycleLength != 35m
            || state.Jugglers.Count != 2
            || state.Throws.Count != 12
            || state.CausalEdges.Count != 12
        )
            return;
        var rows = state
            .Jugglers.Select(juggler =>
                state
                    .Throws.Where(node => node.JugglerId == juggler.Id)
                    .OrderBy(node => node.StartTime)
                    .ToArray()
            )
            .ToArray();
        if (rows[0].Length != 7 || rows[1].Length != 5)
            return;
        var edges = state.CausalEdges.ToDictionary(
            edge => edge.FromThrowId,
            StringComparer.Ordinal
        );
        for (var rowIndex = 0; rowIndex < rows.Length; rowIndex++)
        {
            var row = rows[rowIndex];
            var beatDuration = state.CycleLength / row.Length;
            for (var beat = 0; beat < row.Length; beat++)
            {
                var node = row[beat];
                var targetIndex = (beat - 2 + row.Length) % row.Length;
                var targetCycle = beat < 2 ? 0 : 1;
                if (
                    node.StartTime != beat * beatDuration
                    || node.Height != row.Length
                    || node.Hand != (beat % 2 == 0 ? "R" : "L")
                    || !edges.TryGetValue(node.Id, out var edge)
                    || edge.ToThrowId != row[targetIndex].Id
                    || edge.CycleOffset != targetCycle
                )
                    return;
            }
        }

        state.PresetId = "feeding-7-5";
        state.CausalEdges = state
            .Throws.Select(node => new EditorCausalEdge($"feed-{node.Id}", node.Id, node.Id, 0))
            .ToList();
        for (var index = 0; index < state.Throws.Count; index++)
            state.Throws[index] = state.Throws[index] with { Height = 2m };
    }

    private static decimal DerivedHeight(
        CausalDiagramEditorState state,
        EditorThrow source,
        EditorThrow target,
        int cycleOffset
    )
    {
        var sourceCount = Math.Max(
            1,
            state.Throws.Count(node => node.JugglerId == source.JugglerId)
        );
        var targetCount = Math.Max(
            1,
            state.Throws.Count(node => node.JugglerId == target.JugglerId)
        );
        var sourceBeat = state.CycleLength / sourceCount;
        var targetBeat = state.CycleLength / targetCount;
        return (
                target.StartTime
                + cycleOffset * state.CycleLength
                - source.StartTime
                + 2m * targetBeat
            ) / sourceBeat;
    }

    private static bool ValidText(string? value, int length) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= length;

    private static decimal ReadOptionalDecimal(JsonElement root, string name, decimal fallback)
    {
        if (!root.TryGetProperty(name, out var value))
            return fallback;
        if (!value.TryGetDecimal(out var parsed))
            throw new JsonException($"Invalid {name} setting.");
        return parsed;
    }

    private static string ToBase64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] FromBase64Url(string token)
    {
        if (
            string.IsNullOrEmpty(token)
            || token.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '='))
        )
            throw new FormatException("Invalid Base64URL.");
        var base64 = token.Replace('-', '+').Replace('_', '/');
        base64 += new string('=', (4 - base64.Length % 4) % 4);
        return Convert.FromBase64String(base64);
    }

    private sealed class LegacyState
    {
        public int Version { get; set; } = 1;
        public string PresetId { get; set; } = "custom";
        public string ActiveJugglerId { get; set; } = "A";
        public decimal CycleLength { get; set; }
        public int Repetitions { get; set; }
        public bool SnapEnabled { get; set; } = true;
        public decimal SnapStep { get; set; } = 0.25m;
        public List<LegacyJuggler> Jugglers { get; set; } = [];
        public List<LegacyThrow> Throws { get; set; } = [];
    }

    private sealed record LegacyJuggler(string Id, string Name);

    private sealed class LegacyThrow
    {
        public string Id { get; set; } = "";
        public string JugglerId { get; set; } = "";
        public string Hand { get; set; } = "R";
        public decimal StartTime { get; set; }
        public decimal LandingTime { get; set; }
        public string TargetJugglerId { get; set; } = "";
        public string TargetHand { get; set; } = "R";
    }
}

public sealed record EditorJuggler(string Id, string Name);

public sealed record EditorThrow(
    string Id,
    string JugglerId,
    string Hand,
    decimal StartTime,
    decimal Height
);

public sealed record EditorCausalEdge(
    string Id,
    string FromThrowId,
    string ToThrowId,
    int CycleOffset = 0
);
