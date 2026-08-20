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
        var nodes = new List<Throw>(steps);
        for (var beat = 0; beat < steps; beat++)
        {
            nodes.Add(new Throw(hands[beat], siteswap.Items[beat], beat));
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
            if (causalTarget >= 0 && causalTarget < steps)
            {
                causal.Add(
                    new DiagramArrow(
                        from,
                        nodes[causalTarget],
                        Step: height - numberOfHands,
                        ToJuggler: toJuggler
                    )
                );
            }

            // Ladder: full flight duration (passist comment: don't subtract dwell)
            var landBeat = beat + height;
            if (height > 0 && landBeat < steps)
            {
                ladder.Add(
                    new DiagramArrow(from, nodes[landBeat], Step: height, ToJuggler: toJuggler)
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
            StartProps: ComputeStartProps(siteswap, hands)
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

public record DiagramArrow(Throw From, Throw To, int Step, Person ToJuggler);

public record StartProps(int Left, int Right);

public record DiagramSet(
    ImmutableList<Hand> Hands,
    ImmutableList<Person> Jugglers,
    ImmutableList<Throw> Throws,
    ImmutableList<DiagramArrow> CausalArrows,
    ImmutableList<DiagramArrow> LadderArrows,
    decimal TimeStretchFactor,
    ImmutableDictionary<string, StartProps> StartProps
);
