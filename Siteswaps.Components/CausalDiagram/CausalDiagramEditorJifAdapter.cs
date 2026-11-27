using System.Collections.Immutable;
using Siteswap.Details.CausalDiagram;
using Siteswap.Details.Jif;

namespace Siteswaps.Components.CausalDiagram;

/// <summary>Projects an editor graph into the established domain DiagramSet used by JifMapper.</summary>
public static class CausalDiagramEditorJifAdapter
{
    private const int MaximumTimeScale = 1_000;

    public static bool TryCreateDiagramSet(
        CausalDiagramEditorState state,
        out DiagramSet? diagram,
        out string error
    )
    {
        ArgumentNullException.ThrowIfNull(state);
        diagram = null;
        error = "Dieses Diagramm kann nicht als Passist-Animation dargestellt werden.";

        if (state.Jugglers.Count == 0 || state.Throws.Count == 0 || state.CycleLength <= 0)
            return false;

        if (state.Jugglers.Any(juggler => state.Throws.All(node => node.JugglerId != juggler.Id)))
        {
            error =
                "Jeder Jongleur braucht mindestens einen Wurf, damit die Passist-Animation erstellt werden kann.";
            return false;
        }

        if (TryBuildCanonicalSiteswap(state, out diagram))
        {
            error = string.Empty;
            return true;
        }

        if (state.Throws.Any(node => state.CausalEdges.All(edge => edge.FromThrowId != node.Id)))
        {
            error =
                "Jeder Wurf braucht einen kausalen Pfeil, damit seine Höhe berechnet werden kann.";
            return false;
        }

        if (
            state
                .CausalEdges.GroupBy(edge => (edge.ToThrowId, edge.CycleOffset))
                .Any(group => group.Count() > 1)
        )
        {
            error =
                "Ein Wurfknoten hat mehrere eingehende Pfeile. Passe die Verbindungen an, damit die Passist-Animation wieder eindeutig ist.";
            return false;
        }

        return TryCreateCustomDiagramSet(state, out diagram, out error);
    }

    private static bool TryCreateCustomDiagramSet(
        CausalDiagramEditorState state,
        out DiagramSet? diagram,
        out string error
    )
    {
        diagram = null;
        error = "Dieses Diagramm kann nicht als Passist-Animation dargestellt werden.";
        var jugglers = state.Jugglers.Select(juggler => new Person(juggler.Name)).ToArray();
        var jugglerIndex = state
            .Jugglers.Select((juggler, index) => (juggler.Id, index))
            .ToDictionary(item => item.Id, item => item.index, StringComparer.Ordinal);
        var handOrder = state
            .Throws.Select((node, index) => (Node: node, Index: index))
            .OrderBy(item => item.Node.StartTime)
            .ThenBy(item => item.Index)
            .Select(item => (item.Node.JugglerId, item.Node.Hand))
            .Distinct()
            .ToList();
        handOrder.AddRange(
            state
                .Jugglers.SelectMany(juggler => new[] { (juggler.Id, "R"), (juggler.Id, "L") })
                .Where(hand => !handOrder.Contains(hand))
        );

        var hands = handOrder
            .Select(hand => new Hand(hand.Hand, jugglers[jugglerIndex[hand.JugglerId]]))
            .ToImmutableList();
        var handIndex = new Dictionary<(string JugglerId, string Hand), int>();
        for (var index = 0; index < state.Jugglers.Count; index++)
        {
            handIndex[(state.Jugglers[index].Id, "R")] = hands
                .Select((hand, limb) => (hand, limb))
                .Single(item => item.hand.Person == jugglers[index] && item.hand.Name == "R")
                .limb;
            handIndex[(state.Jugglers[index].Id, "L")] = hands
                .Select((hand, limb) => (hand, limb))
                .Single(item => item.hand.Person == jugglers[index] && item.hand.Name == "L")
                .limb;
        }

        var outgoing = state.CausalEdges.ToDictionary(
            edge => edge.FromThrowId,
            StringComparer.Ordinal
        );
        var nodesById = state.Throws.ToDictionary(node => node.Id, StringComparer.Ordinal);
        var rowCounts = state
            .Jugglers.Select(juggler => state.Throws.Count(node => node.JugglerId == juggler.Id))
            .ToArray();
        var inferredTargets = state.Throws.ToDictionary(
            node => node.Id,
            node => nodesById[outgoing[node.Id].ToThrowId].JugglerId,
            StringComparer.Ordinal
        );

        var durations = new Dictionary<string, decimal>(StringComparer.Ordinal);
        var scaleInputs = new List<decimal> { state.CycleLength };
        foreach (var node in state.Throws)
        {
            scaleInputs.Add(node.StartTime);
            var targetJugglerId = inferredTargets[node.Id];

            if (
                !jugglerIndex.ContainsKey(node.JugglerId)
                || !jugglerIndex.ContainsKey(targetJugglerId)
                || !handIndex.ContainsKey((node.JugglerId, node.Hand))
            )
                return false;

            decimal duration;
            var edge = outgoing[node.Id];
            var target = nodesById[edge.ToThrowId];
            if (!handIndex.ContainsKey((target.JugglerId, target.Hand)))
                return false;
            var targetTime = target.StartTime + edge.CycleOffset * state.CycleLength;
            var targetBeatDuration =
                state.CycleLength
                / state.Throws.Count(candidate => candidate.JugglerId == targetJugglerId);
            duration = targetTime - node.StartTime + 2m * targetBeatDuration;
            scaleInputs.Add(targetTime);
            if (duration <= 0 || duration > int.MaxValue)
                return false;

            durations.Add(node.Id, duration);
            scaleInputs.Add(duration);
        }

        var scale = FindIntegerScale(scaleInputs);
        if (scale is null || state.CycleLength * scale > int.MaxValue)
        {
            error = "Die Zeitwerte dieses Diagramms benötigen eine zu große Passist-Zeitskala.";
            return false;
        }

        if (
            !TryBuildDomainThrows(
                state,
                scale.Value,
                outgoing,
                nodesById,
                durations,
                jugglers,
                jugglerIndex,
                hands,
                handIndex,
                rowCounts,
                out var domainThrows,
                out error
            )
            || !TryBuildCausalArrows(
                state,
                scale.Value,
                outgoing,
                nodesById,
                domainThrows,
                jugglers,
                jugglerIndex,
                hands,
                handIndex,
                rowCounts,
                out var causalArrows,
                out error
            )
        )
            return false;

        var limbPermutation = Enumerable
            .Range(0, hands.Count)
            .Select(limb =>
            {
                var hand = hands[limb];
                var personIndex = Array.FindIndex(jugglers, person => person == hand.Person);
                if (rowCounts[personIndex] % 2 == 0)
                    return limb;
                var jugglerId = state.Jugglers[personIndex].Id;
                var otherHand = hand.Name == "R" ? "L" : "R";
                return handIndex[(jugglerId, otherHand)];
            })
            .ToImmutableList();
        diagram = new DiagramSet(
            Hands: hands,
            Jugglers: jugglers.ToImmutableList(),
            Throws: domainThrows.Values.OrderBy(node => node.Time).ToImmutableList(),
            CausalArrows: causalArrows.ToImmutableList(),
            LadderArrows: ImmutableList<DiagramArrow>.Empty,
            TimeStretchFactor: scale.Value,
            StepCount: decimal.ToInt32(state.CycleLength * scale.Value),
            LimbPermutation: limbPermutation,
            StartProps: ImmutableDictionary<string, StartProps>.Empty,
            JugglerBeatDurations: GetJugglerBeatDurations(state, rowCounts, scale.Value)
        );
        error = string.Empty;
        return true;
    }

    private static ImmutableDictionary<string, decimal> GetJugglerBeatDurations(
        CausalDiagramEditorState state,
        int[] rowCounts,
        int scale
    ) =>
        state
            .Jugglers.Select(
                (juggler, index) =>
                    (Juggler: juggler.Name, Duration: state.CycleLength / rowCounts[index] * scale)
            )
            .ToImmutableDictionary(
                item => item.Juggler,
                item => item.Duration,
                StringComparer.Ordinal
            );

    private static bool TryBuildDomainThrows(
        CausalDiagramEditorState state,
        int scale,
        Dictionary<string, EditorCausalEdge> outgoing,
        Dictionary<string, EditorThrow> nodesById,
        Dictionary<string, decimal> durations,
        Person[] jugglers,
        Dictionary<string, int> jugglerIndex,
        ImmutableList<Hand> hands,
        Dictionary<(string JugglerId, string Hand), int> handIndex,
        int[] rowCounts,
        out Dictionary<string, DiagramThrow> domainThrows,
        out string error
    )
    {
        domainThrows = new Dictionary<string, DiagramThrow>(StringComparer.Ordinal);
        error = string.Empty;
        foreach (var node in state.Throws)
        {
            int? destination = null;
            var height = 0;
            if (durations.TryGetValue(node.Id, out var duration))
            {
                if (!TryScaleToInteger(duration, scale, out height))
                {
                    error =
                        "Eine Wurfhöhe lässt sich nicht als ganzzahliger Passist-Zeitschritt darstellen.";
                    return false;
                }
                var edge = outgoing[node.Id];
                var target = nodesById[edge.ToThrowId];
                destination = LandingHandAtTime(
                    state,
                    target.JugglerId,
                    node.StartTime + duration,
                    jugglerIndex,
                    handIndex,
                    rowCounts
                );
            }

            domainThrows.Add(
                node.Id,
                new DiagramThrow(
                    hands[handIndex[(node.JugglerId, node.Hand)]],
                    node.StartTime * scale,
                    destination
                )
            );
        }
        return true;
    }

    private static bool TryBuildCausalArrows(
        CausalDiagramEditorState state,
        int scale,
        Dictionary<string, EditorCausalEdge> outgoing,
        Dictionary<string, EditorThrow> nodesById,
        Dictionary<string, DiagramThrow> domainThrows,
        Person[] jugglers,
        Dictionary<string, int> jugglerIndex,
        ImmutableList<Hand> hands,
        Dictionary<(string JugglerId, string Hand), int> handIndex,
        int[] rowCounts,
        out List<DiagramArrow> causalArrows,
        out string error
    )
    {
        causalArrows = [];
        error = string.Empty;
        foreach (var edge in state.CausalEdges)
        {
            var target = nodesById[edge.ToThrowId];
            var from = domainThrows[edge.FromThrowId];
            var rawTargetTime = target.StartTime + edge.CycleOffset * state.CycleLength;
            var targetTime = rawTargetTime * scale;
            if (
                !TryScaleToInteger(
                    rawTargetTime - nodesById[edge.FromThrowId].StartTime,
                    scale,
                    out var step
                )
            )
            {
                error = "Ein Kausalziel lässt sich nicht auf der Passist-Zeitskala darstellen.";
                return false;
            }

            var targetHandIndex = LandingLimbIndex(
                target,
                edge.CycleOffset,
                jugglerIndex,
                handIndex,
                rowCounts
            );
            var to = new DiagramThrow(hands[targetHandIndex], targetTime, targetHandIndex);
            causalArrows.Add(
                new DiagramArrow(
                    from,
                    to,
                    step,
                    jugglers[jugglerIndex[target.JugglerId]],
                    OrbitIndex: -1
                )
            );
        }
        return true;
    }

    private static int LandingLimbIndex(
        EditorThrow target,
        int cycleOffset,
        Dictionary<string, int> jugglerIndex,
        Dictionary<(string JugglerId, string Hand), int> handIndex,
        int[] rowCounts
    )
    {
        var person = jugglerIndex[target.JugglerId];
        var targetLimb = handIndex[(target.JugglerId, target.Hand)];
        if (rowCounts[person] % 2 == 1 && cycleOffset % 2 != 0)
            return handIndex[(target.JugglerId, target.Hand == "R" ? "L" : "R")];
        return targetLimb;
    }

    private static int LandingHandAtTime(
        CausalDiagramEditorState state,
        string targetJugglerId,
        decimal landingTime,
        Dictionary<string, int> jugglerIndex,
        Dictionary<(string JugglerId, string Hand), int> handIndex,
        int[] rowCounts
    )
    {
        var targetNodes = state
            .Throws.Where(node => node.JugglerId == targetJugglerId)
            .OrderBy(node => node.StartTime)
            .ToArray();
        var cycle = decimal.ToInt32(
            decimal.Floor((landingTime - targetNodes[0].StartTime) / state.CycleLength)
        );
        var localTime = landingTime - cycle * state.CycleLength;
        var targetIndex = Array.FindIndex(targetNodes, node => node.StartTime >= localTime);
        if (targetIndex < 0)
        {
            targetIndex = 0;
            cycle++;
        }
        var target = targetNodes[targetIndex];
        return LandingLimbIndex(target, cycle, jugglerIndex, handIndex, rowCounts);
    }

    private static bool TryBuildCanonicalSiteswap(
        CausalDiagramEditorState state,
        out DiagramSet? diagram
    )
    {
        diagram = null;
        if (
            state.Jugglers.Count != 1
            || state.CycleLength != decimal.Truncate(state.CycleLength)
            || state.CycleLength is < 1 or > global::Siteswap.Details.Siteswap.MaxPeriodLength
        )
            return false;

        var period = decimal.ToInt32(state.CycleLength);
        var ordered = state.Throws.OrderBy(node => node.StartTime).ToArray();
        var heights = ordered.Select(node => state.HeightOf(node.Id) ?? 0m).ToArray();
        if (
            ordered.Length != period
            || ordered.Where((node, index) => node.StartTime != index).Any()
            || heights.Any(height =>
                decimal.Truncate(height) != height || height < 0 || height > int.MaxValue
            )
        )
            return false;

        var siteSwapHeights = heights.Select(decimal.ToInt32).ToArray();
        if (
            !global::Siteswap.Details.Siteswap.TryCreate(siteSwapHeights, out var siteswap)
            || siteswap is null
        )
            return false;

        var hands = PassingHandOrder.Create(numberOfJugglers: 1);
        if (ordered.Where((node, index) => node.Hand != hands[index].Name).Any())
            return false;

        var edges = state.CausalEdges.ToDictionary(
            edge => edge.FromThrowId,
            StringComparer.Ordinal
        );
        var byBeat = ordered
            .Select((node, beat) => (node, beat))
            .ToDictionary(item => item.beat, item => item.node);
        foreach (var (node, beat) in ordered.Select((node, beat) => (node, beat)))
        {
            var height = siteSwapHeights[beat];
            if (height == 0)
            {
                if (edges.ContainsKey(node.Id))
                    return false;
                continue;
            }

            var targetTime = beat + height - 2;
            var targetBeat = ((targetTime % period) + period) % period;
            var cycleOffset = (int)Math.Floor((decimal)targetTime / period);
            if (
                !edges.TryGetValue(node.Id, out var edge)
                || edge.ToThrowId != byBeat[targetBeat].Id
                || edge.CycleOffset != cycleOffset
            )
                return false;
        }

        if (edges.Count != heights.Count(height => height > 0))
            return false;

        diagram = SiteswapDiagramBuilder.Build(siteswap, numberOfJugglers: 1);
        return true;
    }

    public static bool TryMap(
        CausalDiagramEditorState state,
        out JifPattern? pattern,
        out string error,
        bool colorOrbits = false
    )
    {
        pattern = null;
        try
        {
            if (!TryCreateDiagramSet(state, out var diagram, out error) || diagram is null)
                return false;
            pattern = JifMapper.Map(
                diagram,
                propType: "club",
                patternType: "passingSiteswap",
                colorOrbits: colorOrbits
            );
            pattern = NormalizeAnimationTimeScale(state, pattern);
            error = string.Empty;
            return true;
        }
        catch (ArgumentException)
        {
            error = "Die kausalen Verbindungen ergeben kein unterstütztes Passist-Muster.";
            return false;
        }
        catch (InvalidOperationException)
        {
            error =
                "Die physische Wurf- und Zielzuordnung schließt sich über den Wiederholungszyklus nicht zu einem periodischen Prop-Muster.";
            return false;
        }
        catch (KeyNotFoundException)
        {
            error = "Ein Kausalknoten verweist auf einen nicht vorhandenen Wurf.";
            return false;
        }
        catch (OverflowException)
        {
            error = "Die Passist-Animation überschreitet den unterstützten Umfang.";
            return false;
        }
    }

    private static JifPattern NormalizeAnimationTimeScale(
        CausalDiagramEditorState state,
        JifPattern pattern
    )
    {
        if (state.Jugglers.Count < 2 || pattern.Repetition?.Period is not int period)
            return pattern;

        // Normalize local beat heights to Passist's shared clock using the fastest row beat.
        var fastestBeat = state
            .Jugglers.Select(juggler =>
                state.CycleLength / state.Throws.Count(node => node.JugglerId == juggler.Id)
            )
            .Min();
        if (fastestBeat <= 1m)
            return pattern;

        var factor = 1m / fastestBeat;
        var scaledPeriod = period * factor;
        if (decimal.Truncate(scaledPeriod) != scaledPeriod || scaledPeriod is < 1 or > int.MaxValue)
            return pattern;

        return pattern with
        {
            TimeStretchFactor = pattern.TimeStretchFactor,
            Repetition = pattern.Repetition with { Period = decimal.ToInt32(scaledPeriod) },
            Throws = pattern
                .Throws?.Select(throwEvent =>
                    throwEvent with
                    {
                        Time = throwEvent.Time * factor,
                        Duration = throwEvent.Duration * factor,
                    }
                )
                .ToArray(),
        };
    }

    private static int? FindIntegerScale(IEnumerable<decimal> values)
    {
        for (var scale = 1; scale <= MaximumTimeScale; scale++)
        {
            if (values.All(value => IsNearInteger(value * scale)))
                return scale;
        }

        return null;
    }

    private static bool TryScaleToInteger(decimal value, int scale, out int result)
    {
        var scaled = value * scale;
        if (scaled is > int.MaxValue or < int.MinValue || !IsNearInteger(scaled))
        {
            result = 0;
            return false;
        }

        result = decimal.ToInt32(decimal.Round(scaled, 0, MidpointRounding.AwayFromZero));
        return true;
    }

    private static bool IsNearInteger(decimal value) =>
        Math.Abs(value - decimal.Round(value, 0, MidpointRounding.AwayFromZero))
        <= 0.000000000000000000000001m;
}
