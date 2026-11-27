using System.Globalization;
using System.Numerics;
using System.Text.Json;

namespace Siteswaps.Components.CausalDiagram;

public sealed class CausalDiagramEditorState
{
    private const string CompactTokenPrefix = "j4.";
    private const string SequenceCompactTokenPrefix = "j5.";
    private const int MaximumTokenLength = 300_000;
    private const int MaximumDecodedPayloadBytes = 220_000;

    private sealed record FeedingRow(
        string Name,
        string? HandSequence,
        decimal Offset,
        List<(int Height, string Target)> Throws
    );

    public int Version { get; set; } = 4;

    /// <summary>Optional finite-sequence selection; the graph remains the ordinary periodic state.</summary>
    public string? SequenceId { get; set; }
    public int? SequencePhaseIndex { get; set; }
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

    /// <summary>Checks whether the state can be safely displayed and edited.</summary>
    public bool IsValidForEditor() => IsValid(this);

    /// <summary>Checks graph balance and whether the mapped throws form a periodic juggle.</summary>
    public bool IsValidPattern() =>
        IsValidForEditor()
        && HasBalancedCausalEdges(this)
        && CausalDiagramEditorJifAdapter.TryMap(this, out _, out _);

    internal bool HasBalancedCausalEdges() => HasBalancedCausalEdges(this);

    private static bool HasBalancedCausalEdges(CausalDiagramEditorState state)
    {
        var incomingCounts = state
            .CausalEdges.GroupBy(edge => edge.ToThrowId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        var outgoingCounts = state
            .CausalEdges.GroupBy(edge => edge.FromThrowId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        return state.Throws.All(node =>
            incomingCounts.GetValueOrDefault(node.Id) == outgoingCounts.GetValueOrDefault(node.Id)
        );
    }

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
                        node.Time
                    )
            )
            .ToList();

        var edges = new List<EditorCausalEdge>();
        foreach (var source in diagram.CausalArrows)
        {
            var sourceId = FindThrowId(source.From, diagram.Throws, throws);
            if (sourceId is null)
                continue;
            var targetPerson = Array.FindIndex(jugglers.ToArray(), juggler =>
                juggler.Name == source.To.Hand.Person.Name
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
        Siteswap.Details.CausalDiagram.DiagramThrow target,
        System.Collections.Immutable.ImmutableList<Siteswap.Details.CausalDiagram.DiagramThrow> sourceThrows,
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
            nodes.Add(new EditorThrow($"A{beat + 1}", "A", beat % 2 == 0 ? "R" : "L", beat));
        }

        state = new CausalDiagramEditorState
        {
            PresetId = presetId,
            CycleLength = handPeriod,
            ActiveJugglerId = "A",
            Jugglers = [new EditorJuggler("A", "Jongleur A")],
            Throws = nodes,
            CausalEdges = SiteswapEdges(
                nodes,
                Enumerable
                    .Range(0, handPeriod)
                    .Select(beat => siteswap.Items[beat % siteswap.Length])
                    .ToArray(),
                handPeriod,
                numberOfHands: 2
            ),
        };
        state.NormalizeCausalEdgeOffsets();
        return true;
    }

    private static List<EditorCausalEdge> SiteswapEdges(
        IReadOnlyList<EditorThrow> nodes,
        int[] heights,
        decimal period,
        int numberOfHands
    )
    {
        var byTime = nodes.ToDictionary(node => (int)node.StartTime);
        var edges = new List<EditorCausalEdge>();
        foreach (var (node, beat) in nodes.Select((node, beat) => (node, beat)))
        {
            var height = heights[beat];
            if (height == 0)
                continue;
            var absoluteBeat = (int)node.StartTime + height - numberOfHands;
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
                        (beat * beatDuration + row.Offset * beatDuration) % cycleLength
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
        state.NormalizeCausalEdgeOffsets();
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

    public string Encode()
    {
        NormalizeCausalEdgeOffsets();
        using var output = new MemoryStream();
        using (var writer = new Utf8JsonWriter(output))
        {
            writer.WriteStartObject();
            writer.WriteNumber("v", Version);
            if (Version >= 5)
            {
                writer.WriteString("x", SequenceId);
                writer.WriteNumber("z", SequencePhaseIndex ?? 0);
            }
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
        return (Version >= 5 ? SequenceCompactTokenPrefix : CompactTokenPrefix)
            + ToBase64Url(output.ToArray());
    }

    public static bool TryDecode(string? token, out CausalDiagramEditorState? state)
    {
        state = null;
        if (string.IsNullOrWhiteSpace(token) || token.Length > MaximumTokenLength)
            return false;
        var isSequenceToken = token.StartsWith(
            SequenceCompactTokenPrefix,
            StringComparison.Ordinal
        );
        if (!isSequenceToken && !token.StartsWith(CompactTokenPrefix, StringComparison.Ordinal))
            return false;
        try
        {
            var prefix = isSequenceToken ? SequenceCompactTokenPrefix : CompactTokenPrefix;
            var decoded = DeserializeCompact(
                DecodePayload(token[prefix.Length..]),
                expectedVersion: isSequenceToken ? 5 : 4
            );
            if (decoded is not null)
                decoded.NormalizeCausalEdgeOffsets();
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
        if (root.ValueKind != JsonValueKind.Object)
            throw new JsonException("Invalid editor state shape.");

        var version = RequiredInt(root, "v");
        if (version != expectedVersion)
            throw new JsonException("Unsupported editor state version.");

        var preset = RequiredString(root, "p");
        var active = RequiredString(root, "a");
        var cycle = RequiredDecimal(root, "c");
        var repetitions = RequiredInt(root, "r");
        var snap = RequiredBoolean(root, "s");
        var snapStep = RequiredDecimal(root, "q");
        var jugglers = RequiredArray(root, "j", 16);
        var throws = RequiredArray(root, "t", 500);
        var edges = RequiredArray(root, "e", 2_000);
        var state = new CausalDiagramEditorState
        {
            Version = version,
            PresetId = preset,
            ActiveJugglerId = active,
            CycleLength = cycle,
            Repetitions = repetitions,
            SnapEnabled = snap,
            SnapStep = snapStep,
            TimelineWidth = ReadOptionalDecimal(root, "w", 560m),
            LaneSpacing = ReadOptionalDecimal(root, "l", 105m),
            SequenceId = version >= 5 ? RequiredString(root, "x") : null,
            SequencePhaseIndex = version >= 5 ? RequiredInt(root, "z") : null,
            Jugglers = [],
            Throws = [],
            CausalEdges = [],
        };
        ReadJugglers(state, jugglers);
        ReadThrows(state, throws);
        ReadCausalEdges(state, edges);
        return state;
    }

    private static void ReadJugglers(CausalDiagramEditorState state, JsonElement jugglers)
    {
        foreach (var item in jugglers.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Array || item.GetArrayLength() != 2)
                throw new JsonException();
            state.Jugglers.Add(
                new EditorJuggler(RequiredArrayString(item, 0), RequiredArrayString(item, 1))
            );
        }
    }

    private static void ReadThrows(CausalDiagramEditorState state, JsonElement throws)
    {
        foreach (var item in throws.EnumerateArray())
        {
            if (
                item.ValueKind != JsonValueKind.Array
                || item.GetArrayLength() != 4
                || !item[3].TryGetDecimal(out var start)
            )
                throw new JsonException();
            state.Throws.Add(
                new EditorThrow(
                    RequiredArrayString(item, 0),
                    RequiredArrayString(item, 1),
                    RequiredArrayString(item, 2),
                    start
                )
            );
        }
    }

    private static void ReadCausalEdges(CausalDiagramEditorState state, JsonElement edges)
    {
        foreach (var item in edges.EnumerateArray())
        {
            if (
                item.ValueKind != JsonValueKind.Array
                || item.GetArrayLength() != 4
                || !item[3].TryGetInt32(out var offset)
            )
                throw new JsonException();
            state.CausalEdges.Add(
                new EditorCausalEdge(
                    RequiredArrayString(item, 0),
                    RequiredArrayString(item, 1),
                    RequiredArrayString(item, 2),
                    offset
                )
            );
        }
    }

    private static string RequiredArrayString(JsonElement item, int index) =>
        item[index].ValueKind == JsonValueKind.String
            ? item[index].GetString() ?? throw new JsonException()
            : throw new JsonException();

    private static JsonElement RequiredProperty(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value)
            ? value
            : throw new JsonException($"Missing {name}.");

    private static string RequiredString(JsonElement root, string name)
    {
        var value = RequiredProperty(root, name);
        return value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? throw new JsonException($"Invalid {name}.")
            : throw new JsonException($"Invalid {name}.");
    }

    private static int RequiredInt(JsonElement root, string name)
    {
        var value = RequiredProperty(root, name);
        return value.TryGetInt32(out var parsed)
            ? parsed
            : throw new JsonException($"Invalid {name}.");
    }

    private static decimal RequiredDecimal(JsonElement root, string name)
    {
        var value = RequiredProperty(root, name);
        return value.TryGetDecimal(out var parsed)
            ? parsed
            : throw new JsonException($"Invalid {name}.");
    }

    private static bool RequiredBoolean(JsonElement root, string name)
    {
        var value = RequiredProperty(root, name);
        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw new JsonException($"Invalid {name}."),
        };
    }

    private static JsonElement RequiredArray(JsonElement root, string name, int maximumLength)
    {
        var value = RequiredProperty(root, name);
        if (value.ValueKind != JsonValueKind.Array || value.GetArrayLength() > maximumLength)
            throw new JsonException($"Invalid {name}.");
        return value;
    }

    private static bool IsValid(CausalDiagramEditorState? state)
    {
        if (
            state is null
            || state.Version is not (4 or 5)
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
            || (
                state.Version == 5
                && (
                    state.SequenceId is null
                    || state.SequencePhaseIndex is null
                    || !CausalDiagramEditorSequences.IsValidSelection(
                        state.SequenceId,
                        state.SequencePhaseIndex.Value
                    )
                )
            )
            || (
                state.Version == 4
                && (state.SequenceId is not null || state.SequencePhaseIndex is not null)
            )
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
        return true;
    }

    public decimal? HeightOf(string throwId)
    {
        var source = Throws.FirstOrDefault(node => node.Id == throwId);
        var edge = CausalEdges.SingleOrDefault(item => item.FromThrowId == throwId);
        if (source is null || edge is null)
            return null;
        var target = Throws.FirstOrDefault(node => node.Id == edge.ToThrowId);
        return target is null ? null : DerivedHeight(this, source, target, edge.CycleOffset);
    }

    public void NormalizeCausalEdgeOffsets()
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
        }
    }

    public decimal GetGlobalBeatDuration()
    {
        var durations = new List<BeatFraction> { BeatFraction.Approximate(CycleLength) };
        foreach (var juggler in Jugglers)
        {
            var nodes = Throws
                .Where(node => node.JugglerId == juggler.Id)
                .OrderBy(node => node.StartTime)
                .ToArray();
            var beatDuration = CycleLength / Math.Max(1, nodes.Length);
            durations.Add(BeatFraction.Approximate(beatDuration));
            if (nodes.Length == 0)
                continue;

            var phase = nodes[0].StartTime % beatDuration;
            durations.Add(BeatFraction.Approximate(phase));
        }

        var globalBeat = durations.Aggregate(BeatFraction.GreatestCommonDivisor);
        var result = globalBeat.ToDecimal();
        return result > 0 ? result : CycleLength / Math.Max(1, Throws.Count);
    }

    private readonly record struct BeatFraction(BigInteger Numerator, BigInteger Denominator)
    {
        public static BeatFraction Approximate(decimal value)
        {
            value = Math.Abs(value);
            for (var denominator = 1; denominator <= 1_000; denominator++)
            {
                var numerator = decimal.Round(
                    value * denominator,
                    0,
                    MidpointRounding.AwayFromZero
                );
                if (Math.Abs(value - numerator / denominator) <= 0.000000000000000000000001m)
                    return Create(new BigInteger(numerator), denominator);
            }

            var bits = decimal.GetBits(value);
            var integer =
                new BigInteger((uint)bits[0])
                | new BigInteger((uint)bits[1]) << 32
                | new BigInteger((uint)bits[2]) << 64;
            var scale = (bits[3] >> 16) & 0x7F;
            return Create(integer, BigInteger.Pow(10, scale));
        }

        public static BeatFraction GreatestCommonDivisor(BeatFraction left, BeatFraction right)
        {
            var commonDenominator = LeastCommonMultipleFractions(
                left.Denominator,
                right.Denominator
            );
            var leftNumerator = left.Numerator * (commonDenominator / left.Denominator);
            var rightNumerator = right.Numerator * (commonDenominator / right.Denominator);
            return Create(
                BigInteger.GreatestCommonDivisor(leftNumerator, rightNumerator),
                commonDenominator
            );
        }

        public decimal ToDecimal() => (decimal)Numerator / (decimal)Denominator;

        private static BeatFraction Create(BigInteger numerator, BigInteger denominator)
        {
            if (denominator.IsZero)
                throw new DivideByZeroException();
            if (denominator.Sign < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }
            var divisor = BigInteger.GreatestCommonDivisor(BigInteger.Abs(numerator), denominator);
            return new BeatFraction(numerator / divisor, denominator / divisor);
        }

        private static BigInteger LeastCommonMultipleFractions(BigInteger left, BigInteger right) =>
            left / BigInteger.GreatestCommonDivisor(left, right) * right;
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

    private static decimal DerivedHeight(
        CausalDiagramEditorState state,
        EditorThrow source,
        EditorThrow target,
        int cycleOffset
    )
    {
        var targetThrowCount = state.Throws.Count(node => node.JugglerId == target.JugglerId);
        if (targetThrowCount == 0)
            return 0;
        var targetBeatDuration = state.CycleLength / targetThrowCount;
        var globalBeatDuration = state.GetGlobalBeatDuration();
        if (globalBeatDuration <= 0)
            return 0;
        return (
                target.StartTime
                + cycleOffset * state.CycleLength
                - source.StartTime
                + 2m * targetBeatDuration
            ) / globalBeatDuration;
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
}

public sealed record EditorJuggler(string Id, string Name);

public sealed record EditorThrow(string Id, string JugglerId, string Hand, decimal StartTime);

public sealed record EditorCausalEdge(
    string Id,
    string FromThrowId,
    string ToThrowId,
    int CycleOffset = 0
);
