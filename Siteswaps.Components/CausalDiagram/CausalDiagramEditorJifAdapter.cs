using System.Collections.Immutable;
using Siteswap.Details.CausalDiagram;
using Siteswap.Details.Jif;

namespace Siteswaps.Components.CausalDiagram;

/// <summary>Projects an editor graph into the established domain DiagramSet used by JifMapper.</summary>
public static class CausalDiagramEditorJifAdapter
{
    private const int MaximumTimeScale = 100_000;
    private const decimal ScaledTimeTolerance = 0.000_001m;

    public static bool TryCreateDiagramSet(
        CausalDiagramEditorState state,
        out DiagramSet? diagram,
        out string error
    )
    {
        ArgumentNullException.ThrowIfNull(state);
        diagram = null;
        error = "Dieses Diagramm kann nicht als Passist-Animation dargestellt werden.";

        if (!state.HasBalancedCausalEdges())
        {
            error =
                "Jeder Kausalknoten muss gleich viele eingehende und ausgehende Pfeile haben; prüfe mehrere eingehende Pfeile oder fehlende Verbindungen.";
            return false;
        }

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
        var hands = jugglers
            .SelectMany(person => new[] { new Hand("R", person), new Hand("L", person) })
            .ToImmutableList();
        var handIndex = new Dictionary<(string JugglerId, string Hand), int>();
        for (var index = 0; index < state.Jugglers.Count; index++)
        {
            handIndex[(state.Jugglers[index].Id, "R")] = index * 2;
            handIndex[(state.Jugglers[index].Id, "L")] = index * 2 + 1;
        }

        var outgoing = state.CausalEdges.ToDictionary(
            edge => edge.FromThrowId,
            StringComparer.Ordinal
        );
        var nodesById = state.Throws.ToDictionary(node => node.Id, StringComparer.Ordinal);
        var rowCounts = state
            .Jugglers.Select(juggler =>
                state
                    .Throws.Where(node => node.JugglerId == juggler.Id)
                    .Select(node => node.StartTime)
                    .Distinct()
                    .Count()
            )
            .ToArray();
        var inferredTargets = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var node in state.Throws)
        {
            if (outgoing.TryGetValue(node.Id, out var edge))
            {
                inferredTargets[node.Id] = nodesById[edge.ToThrowId].JugglerId;
                continue;
            }

            inferredTargets[node.Id] = node.JugglerId;
        }

        var durations = new Dictionary<string, decimal>(StringComparer.Ordinal);
        var scaleInputs = new List<decimal> { state.CycleLength };
        foreach (var node in state.Throws)
        {
            scaleInputs.Add(node.StartTime);
            if (!inferredTargets.TryGetValue(node.Id, out var targetJugglerId))
                continue;

            if (
                !jugglerIndex.ContainsKey(node.JugglerId)
                || !jugglerIndex.ContainsKey(targetJugglerId)
                || !handIndex.ContainsKey((node.JugglerId, node.Hand))
            )
                return false;

            var targetBeatDuration =
                state.CycleLength
                / state.Throws.Count(candidate => candidate.JugglerId == targetJugglerId);
            decimal duration;
            if (outgoing.TryGetValue(node.Id, out var edge))
            {
                var target = nodesById[edge.ToThrowId];
                if (!handIndex.ContainsKey((target.JugglerId, target.Hand)))
                    return false;
                var targetTime = target.StartTime + edge.CycleOffset * state.CycleLength;
                duration = targetTime - node.StartTime + 2m * targetBeatDuration;
                scaleInputs.Add(targetTime);
            }
            else
            {
                if (node.Height == 0)
                    continue;

                var sourceBeatDuration =
                    state.CycleLength
                    / state.Throws.Count(candidate => candidate.JugglerId == node.JugglerId);
                duration = node.Height * sourceBeatDuration;
            }
            if (duration < 0 || duration > int.MaxValue)
                return false;

            durations.Add(node.Id, duration);
            scaleInputs.Add(duration);
        }

        var scale = FindIntegerScale(scaleInputs);
        if (
            scale is null
            || !TryScaleToInteger(state.CycleLength, scale.Value, out var scaledStepCount)
        )
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
                inferredTargets,
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
            .Select(limb => limb / 2 * 2 + (limb % 2 + rowCounts[limb / 2] % 2) % 2)
            .ToImmutableList();
        diagram = new DiagramSet(
            Hands: hands,
            Jugglers: jugglers.ToImmutableList(),
            Throws: domainThrows.Values.OrderBy(node => node.Time).ToImmutableList(),
            CausalArrows: causalArrows.ToImmutableList(),
            LadderArrows: ImmutableList<DiagramArrow>.Empty,
            TimeStretchFactor: scale.Value,
            StepCount: scaledStepCount,
            LimbPermutation: limbPermutation,
            StartProps: ImmutableDictionary<string, StartProps>.Empty
        );
        error = string.Empty;
        return true;
    }

    private static bool TryBuildDomainThrows(
        CausalDiagramEditorState state,
        int scale,
        Dictionary<string, EditorCausalEdge> outgoing,
        Dictionary<string, EditorThrow> nodesById,
        Dictionary<string, decimal> durations,
        Dictionary<string, string> inferredTargets,
        Person[] jugglers,
        Dictionary<string, int> jugglerIndex,
        ImmutableList<Hand> hands,
        Dictionary<(string JugglerId, string Hand), int> handIndex,
        int[] rowCounts,
        out Dictionary<string, Throw> domainThrows,
        out string error
    )
    {
        domainThrows = new Dictionary<string, Throw>(StringComparer.Ordinal);
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
                if (outgoing.TryGetValue(node.Id, out var edge))
                {
                    var target = nodesById[edge.ToThrowId];
                    var landingTime = node.StartTime + duration;
                    destination = LandingHandAtTime(
                        state,
                        target.JugglerId,
                        landingTime,
                        jugglerIndex,
                        handIndex,
                        rowCounts
                    );
                }
                else
                {
                    var landingTime = node.StartTime + duration;
                    destination = LandingHandAtTime(
                        state,
                        inferredTargets[node.Id],
                        landingTime,
                        jugglerIndex,
                        handIndex,
                        rowCounts
                    );
                }
            }

            domainThrows.Add(
                node.Id,
                new Throw(
                    hands[handIndex[(node.JugglerId, node.Hand)]],
                    height,
                    ScaleTime(node.StartTime, scale),
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
        Dictionary<string, Throw> domainThrows,
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
            var unscaledTargetTime = target.StartTime + edge.CycleOffset * state.CycleLength;
            var targetTime = ScaleTime(unscaledTargetTime, scale);
            var stepValue = targetTime - from.Time;
            if (!TryScaleToInteger(stepValue, 1, out var scaledStepValue))
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
            var to = new Throw(hands[targetHandIndex], 0, targetTime, targetHandIndex);
            causalArrows.Add(
                new DiagramArrow(
                    from,
                    to,
                    scaledStepValue,
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
            return person * 2 + (targetLimb % 2 == 0 ? 1 : 0);
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
        if (
            ordered.Length != period
            || ordered.Where((node, index) => node.StartTime != index).Any()
            || ordered.Any(node =>
                decimal.Truncate(node.Height) != node.Height
                || node.Height < 0
                || node.Height > int.MaxValue
            )
        )
            return false;

        var heights = ordered.Select(node => decimal.ToInt32(node.Height)).ToArray();
        if (
            !global::Siteswap.Details.Siteswap.TryCreate(heights, out var siteswap)
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
            var height = heights[beat];
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
                ExpandToHandPeriod(diagram),
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

    private static DiagramSet ExpandToHandPeriod(DiagramSet diagram)
    {
        var permutation = diagram.LimbPermutation;
        var limbMap = Enumerable.Range(0, permutation.Count).ToArray();
        var repetitions = 1;
        while (true)
        {
            limbMap = limbMap.Select(index => permutation[index]).ToArray();
            if (limbMap.SequenceEqual(Enumerable.Range(0, permutation.Count)))
                break;
            if (++repetitions > 64)
                throw new InvalidOperationException(
                    "The hand assignment has an unsupported period."
                );
        }

        if (repetitions == 1)
            return diagram;

        var throws = ImmutableList.CreateBuilder<Throw>();
        var map = Enumerable.Range(0, permutation.Count).ToArray();
        for (var repetition = 0; repetition < repetitions; repetition++)
        {
            foreach (var item in diagram.Throws)
            {
                throws.Add(
                    item with
                    {
                        Time = item.Time + repetition * diagram.StepCount,
                        Hand = diagram.Hands[map[diagram.Hands.IndexOf(item.Hand)]],
                        ToLimb = item.ToLimb is { } target ? map[target] : null,
                    }
                );
            }
            map = map.Select(index => permutation[index]).ToArray();
        }

        var arrows = ImmutableList.CreateBuilder<DiagramArrow>();
        map = Enumerable.Range(0, permutation.Count).ToArray();
        foreach (var item in diagram.CausalArrows)
        {
            for (var repetition = 0; repetition < repetitions; repetition++)
            {
                var from = item.From with
                {
                    Hand = diagram.Hands[map[diagram.Hands.IndexOf(item.From.Hand)]],
                    Time = item.From.Time + repetition * diagram.StepCount,
                    ToLimb = item.From.ToLimb is { } fromTarget ? map[fromTarget] : null,
                };
                var to = item.To with
                {
                    Hand = diagram.Hands[map[diagram.Hands.IndexOf(item.To.Hand)]],
                    Time = item.To.Time + repetition * diagram.StepCount,
                    ToLimb = item.To.ToLimb is { } toTarget ? map[toTarget] : null,
                };
                arrows.Add(item with { From = from, To = to });
            }
            map = map.Select(index => permutation[index]).ToArray();
        }

        return diagram with
        {
            Throws = throws.ToImmutable(),
            CausalArrows = arrows.ToImmutable(),
            StepCount = checked(diagram.StepCount * repetitions),
            LimbPermutation = Enumerable.Range(0, permutation.Count).ToImmutableList(),
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

    private static bool TryScaleToInteger(decimal value, int scale, out int scaled)
    {
        var scaledValue = decimal.Round(value * scale, 0, MidpointRounding.AwayFromZero);
        if (
            decimal.Abs(value * scale - scaledValue) > ScaledTimeTolerance
            || scaledValue is > int.MaxValue or < int.MinValue
        )
        {
            scaled = 0;
            return false;
        }

        scaled = decimal.ToInt32(scaledValue);
        return true;
    }

    private static decimal ScaleTime(decimal time, int scale) =>
        decimal.Round(time * scale, 0, MidpointRounding.AwayFromZero);

    private static bool IsNearInteger(decimal value) =>
        decimal.Abs(value - decimal.Round(value, 0, MidpointRounding.AwayFromZero))
        <= ScaledTimeTolerance;
}
