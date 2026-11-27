using System.Collections.Immutable;

namespace Siteswap.Details.CausalDiagram;

/// <summary>
/// Beat-based causal and ladder diagrams matching passist.org
/// (<c>CausalDiagramWidget.svelte</c> + <c>Siteswap.toJif</c>).
/// <list type="bullet">
/// <item>One throw node per beat.</item>
/// <item>Causal arrow length = height − numberOfHands (dwell).</item>
/// <item>Ladder arrow length = height (object flight).</item>
/// <item>Y-axis lines are jugglers; left/right is a node style.</item>
/// </list>
/// </summary>
public static class SiteswapDiagramBuilder
{
    public static DiagramSet Build(Siteswap siteswap, int numberOfJugglers)
    {
        var hands = PassingHandOrder.Create(numberOfJugglers);
        var steps = ComputeSteps(siteswap, hands.Length);
        return Build(siteswap, hands, steps);
    }

    public static DiagramSet Build(Siteswap siteswap, CyclicArray<Hand> hands, int steps)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(steps, 1);

        var numberOfHands = hands.Length;
        var timeStretchFactor = numberOfHands / 2m;
        var orbitByBeat = OrbitIndices(siteswap);
        var nodes = new List<DiagramThrow>(steps);
        for (var beat = 0; beat < steps; beat++)
        {
            var landingLimbIndex = (beat + siteswap.Items[beat]) % numberOfHands;
            nodes.Add(
                new DiagramThrow(
                    hands[beat],
                    beat,
                    landingLimbIndex,
                    orbitByBeat[beat % siteswap.Length]
                )
            );
        }

        var causal = new List<DiagramArrow>();
        var ladder = new List<DiagramArrow>();
        for (var beat = 0; beat < steps; beat++)
        {
            var height = siteswap.Items[beat];
            var from = nodes[beat];
            var landLimbIndex = (beat + height) % numberOfHands;
            var toJuggler = hands[landLimbIndex].Person;

            // Causal: duration - 2 * timeStretchFactor == height - numberOfHands
            var causalTarget = beat + height - numberOfHands;
            if (height > 0)
            {
                var targetBeat = ((causalTarget % steps) + steps) % steps;
                var target = nodes[targetBeat] with { Time = causalTarget };
                causal.Add(
                    new DiagramArrow(
                        from,
                        target,
                        Step: height - numberOfHands,
                        ToJuggler: toJuggler,
                        OrbitIndex: orbitByBeat[beat % siteswap.Length]
                    )
                );
            }

            // Ladder: full flight duration (passist comment: don't subtract dwell)
            var landBeat = beat + height;
            if (height > 0)
            {
                var targetBeat = ((landBeat % steps) + steps) % steps;
                var target = nodes[targetBeat] with { Time = landBeat };
                ladder.Add(
                    new DiagramArrow(
                        from,
                        target,
                        Step: height,
                        ToJuggler: toJuggler,
                        OrbitIndex: orbitByBeat[beat % siteswap.Length]
                    )
                );
            }
        }

        var jugglers = Enumerable
            .Range(0, numberOfHands)
            .Select(i => hands[i].Person)
            .DistinctBy(p => p.Name)
            .ToImmutableList();

        return new DiagramSet(
            Hands: Enumerable.Range(0, numberOfHands).Select(i => hands[i]).ToImmutableList(),
            Jugglers: jugglers,
            Throws: nodes.ToImmutableList(),
            CausalArrows: causal.ToImmutableList(),
            LadderArrows: ladder.ToImmutableList(),
            TimeStretchFactor: timeStretchFactor,
            StepCount: steps,
            LimbPermutation: Enumerable.Range(0, hands.Length).ToImmutableList(),
            StartProps: ComputeStartProps(siteswap, hands),
            JugglerBeatDurations: jugglers.ToImmutableDictionary(
                juggler => juggler.Name,
                _ => timeStretchFactor,
                StringComparer.Ordinal
            )
        );
    }

    /// <summary>
    /// Mirrors passist <c>toJif</c> repetition.period = LCM(orbit sums, nLimbs).
    /// </summary>
    public static int ComputeSteps(Siteswap siteswap, int numberOfHands)
    {
        var periods = siteswap
            .GetOrbits()
            .Select(o => o.Items.Sum())
            .Where(sum => sum > 0)
            .Append(numberOfHands)
            .ToList();

        return periods.Aggregate(1, Lcm);
    }

    private static int[] OrbitIndices(Siteswap siteswap)
    {
        var periodLength = Math.Clamp(siteswap.Length, 0, Siteswap.MaxPeriodLength);
        var indices = Enumerable.Repeat(-1, periodLength).ToArray();
        var orbits = siteswap.GetOrbits();
        var orbitCount = Math.Min(orbits.Count, periodLength);
        for (var orbitIndex = 0; orbitIndex < orbitCount; orbitIndex++)
        {
            for (var beat = 0; beat < periodLength; beat++)
            {
                if (orbits[orbitIndex].Items[beat] > 0)
                {
                    indices[beat] = orbitIndex;
                }
            }
        }

        return indices;
    }

    private static ImmutableDictionary<string, StartProps> ComputeStartProps(
        Siteswap siteswap,
        CyclicArray<Hand> hands
    )
    {
        var period = siteswap.Length;
        var nLimbs = hands.Length;
        var startLimbs = new int[nLimbs];
        var hasProp = new HashSet<int>();
        var missing = (int)siteswap.NumberOfObjects();
        for (var i = 0; missing > 0; i++)
        {
            var height = siteswap.Items[i];
            if (height <= 0)
            {
                continue;
            }

            if (!hasProp.Contains(i))
            {
                startLimbs[i % nLimbs]++;
                missing--;
            }

            hasProp.Add(i + height);
        }

        return Enumerable
            .Range(0, nLimbs)
            .GroupBy(i => hands[i].Person.Name)
            .ToImmutableDictionary(
                g => g.Key,
                g =>
                {
                    var left = 0;
                    var right = 0;
                    foreach (var i in g)
                    {
                        if (hands[i].Name == "L")
                        {
                            left += startLimbs[i];
                        }
                        else
                        {
                            right += startLimbs[i];
                        }
                    }

                    return new StartProps(Left: left, Right: right);
                }
            );
    }

    private static int Lcm(int a, int b) => a / Gcd(a, b) * b;

    private static int Gcd(int a, int b)
    {
        while (b != 0)
        {
            (a, b) = (b, a % b);
        }

        return Math.Abs(a);
    }
}

public record DiagramArrow(
    DiagramThrow From,
    DiagramThrow To,
    int Step,
    Person ToJuggler,
    int OrbitIndex
);

public record StartProps(int Left, int Right);

public record DiagramSet(
    ImmutableList<Hand> Hands,
    ImmutableList<Person> Jugglers,
    ImmutableList<DiagramThrow> Throws,
    ImmutableList<DiagramArrow> CausalArrows,
    ImmutableList<DiagramArrow> LadderArrows,
    decimal TimeStretchFactor,
    int StepCount,
    ImmutableList<int> LimbPermutation,
    ImmutableDictionary<string, StartProps> StartProps,
    ImmutableDictionary<string, decimal>? JugglerBeatDurations = null
)
{
    public int? HeightOf(DiagramThrow source)
    {
        var arrows = CausalArrows.Where(arrow => arrow.From == source).ToArray();
        if (arrows.Length == 0)
            return null;
        if (arrows.Length != 1)
            throw new InvalidOperationException(
                "A completed causal throw must have exactly one causal arrow."
            );

        var beatDuration =
            JugglerBeatDurations is not null
            && JugglerBeatDurations.TryGetValue(
                arrows[0].ToJuggler.Name,
                out var targetBeatDuration
            )
                ? targetBeatDuration
                : TimeStretchFactor;
        var height = arrows[0].Step + 2m * beatDuration;
        if (height <= 0 || decimal.Truncate(height) != height || height > int.MaxValue)
            throw new InvalidOperationException(
                "The causal arrow does not define a positive integral throw height."
            );
        return decimal.ToInt32(height);
    }
}
