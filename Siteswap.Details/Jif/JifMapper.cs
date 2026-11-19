using Siteswap.Details.CausalDiagram;
using DiagramThrow = Siteswap.Details.CausalDiagram.Throw;

namespace Siteswap.Details.Jif;

/// <summary>Maps a siteswap and its hand topology to the JIF subset consumed by Passist.</summary>
public static class JifMapper
{
    private static readonly string[] PropColors =
    [
        "#c0392b",
        "#0c0d5d",
        "#f45d20",
        "#ed4694",
        "#6f5499",
        "#00dc3c",
        "#ffd700",
        "#f2f2f2",
    ];

    public static JifPattern Map(
        global::Siteswap.Details.Siteswap siteswap,
        int numberOfJugglers,
        string propType = "ball",
        bool colorOrbits = false
    )
    {
        ArgumentNullException.ThrowIfNull(siteswap);
        ArgumentOutOfRangeException.ThrowIfLessThan(numberOfJugglers, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(propType);

        var diagram = SiteswapDiagramBuilder.Build(siteswap, numberOfJugglers);
        var hands = diagram.Hands;
        var jugglers = diagram.Jugglers;
        var jugglerPositions = jugglers
            .Select((juggler, index) => CreateJuggler(juggler.Name, index, numberOfJugglers))
            .ToList();
        var propCount = (int)siteswap.NumberOfObjects();
        var propOrbitColors = GetPropOrbitColors(siteswap, diagram.Throws, propCount);
        var props = Enumerable.Range(0, propCount)
            .Select(i => new JifObject
            {
                Color = colorOrbits ? propOrbitColors[i] : PropColors[i % PropColors.Length],
                Type = propType,
            })
            .ToList();
        var limbs = hands
            .Select(hand => new JifLimb
            {
                Juggler = jugglers.ToList().FindIndex(juggler => juggler.Name == hand.Person.Name),
                Type = hand.Name == "R" ? "right hand" : "left hand",
            })
            .ToList();
        var throws = diagram.Throws
            .Select(node => new JifThrow
            {
                Time = node.Time,
                Duration = node.Height,
                From = HandIndex(node.Hand),
                To = (HandIndex(node.Hand) + node.Height) % hands.Count,
                Label = global::Siteswap.Details.Siteswap.Transform(node.Height),
            })
            .ToList();

        return new JifPattern
        {
            Meta = new JifMeta { Name = $"siteswap {siteswap}" },
            HighLevelDescription = new JifHighLevelDescription
            {
                Type = "vanillaSiteswap",
                Description = siteswap.ToString(),
            },
            Jugglers = jugglerPositions,
            Limbs = limbs,
            Props = props,
            TimeStretchFactor = diagram.TimeStretchFactor,
            Valid = true,
            Repetition = new JifRepetition { Period = diagram.Throws.Count },
            Throws = throws,
        };

        int HandIndex(Hand hand)
        {
            for (var i = 0; i < hands.Count; i++)
            {
                if (hands[i] == hand)
                    return i;
            }

            return 0;
        }
    }

    private static string[] GetPropOrbitColors(
        global::Siteswap.Details.Siteswap siteswap,
        IReadOnlyList<DiagramThrow> throws,
        int propCount
    )
    {
        var orbitByBeat = Enumerable.Repeat(-1, siteswap.Length).ToArray();
        var orbits = siteswap.GetOrbits();
        for (var orbitIndex = 0; orbitIndex < orbits.Count; orbitIndex++)
        {
            for (var beat = 0; beat < siteswap.Length; beat++)
            {
                if (orbits[orbitIndex].Items[beat] > 0)
                    orbitByBeat[beat] = orbitIndex;
            }
        }

        var assignedProps = new bool[throws.Count];
        var colors = new List<string>(propCount);
        for (var beat = 0; beat < throws.Count; beat++)
        {
            if (assignedProps[beat] || throws[beat].Height <= 0)
                continue;

            var orbitIndex = orbitByBeat[beat % siteswap.Length];
            colors.Add(OrbitColorPalette.For(orbitIndex));
            var landingBeat = beat;
            while (landingBeat < throws.Count && throws[landingBeat % throws.Count].Height > 0)
            {
                assignedProps[landingBeat % throws.Count] = true;
                landingBeat += throws[landingBeat % throws.Count].Height;
            }
        }

        if (colors.Count != propCount)
            throw new InvalidOperationException("Siteswap orbit sizes do not match its prop count.");

        return colors.ToArray();
    }

    /// <summary>Maps a causal throw diagram to a complete JIF pattern.</summary>
    public static JifPattern Map(
        DiagramSet diagram,
        string propType = "club",
        string patternType = "passingSiteswap",
        string? name = null,
        string? description = null
    )
    {
        ArgumentNullException.ThrowIfNull(diagram);
        ArgumentException.ThrowIfNullOrWhiteSpace(propType);
        ArgumentException.ThrowIfNullOrWhiteSpace(patternType);
        if (diagram.Hands.Count == 0 || diagram.Jugglers.Count == 0 || diagram.StepCount <= 0)
            throw new ArgumentException("The causal diagram needs jugglers, limbs, and a positive period.", nameof(diagram));
        if (diagram.LimbPermutation.Count != diagram.Hands.Count)
            throw new ArgumentException("The causal diagram limb permutation must match its limbs.", nameof(diagram));

        var handIndices = diagram.Hands
            .Select((hand, index) => (hand, index))
            .ToDictionary(entry => entry.hand, entry => entry.index);
        var orderedThrows = diagram.Throws
            .Where(node => node.Height > 0)
            .OrderBy(node => node.Time)
            .Select(node =>
            {
                if (!handIndices.TryGetValue(node.Hand, out var from))
                    throw new ArgumentException("Every causal throw must start at a diagram limb.", nameof(diagram));
                if (node.ToLimb is not int to || to < 0 || to >= diagram.Hands.Count)
                    throw new ArgumentException("Every causal throw must identify its destination limb.", nameof(diagram));

                return new JifThrow
                {
                    Time = node.Time,
                    Duration = node.Height,
                    From = from,
                    To = to,
                    Label = global::Siteswap.Details.Siteswap.Transform(node.Height),
                };
            })
            .ToArray();
        if (orderedThrows.Length == 0)
            throw new ArgumentException("The causal diagram contains no positive-height throws.", nameof(diagram));

        var limbPermutation = diagram.LimbPermutation.ToArray();
        var (firstPeriodProps, propPermutation, propCount) = AssignPropPermutation(
            orderedThrows,
            limbPermutation,
            diagram.StepCount,
            diagram.Hands.Count
        );
        var expansionCount = LeastCommonMultiple(
            PermutationOrder(limbPermutation),
            PermutationOrder(propPermutation)
        );
        if (expansionCount > 512 || (long)orderedThrows.Length * expansionCount > 30_000)
            throw new InvalidOperationException("Passist needs an expansion larger than this pattern supports.");

        var expandedThrows = new List<JifThrow>(orderedThrows.Length * expansionCount);
        var limbMap = Enumerable.Range(0, limbPermutation.Length).ToArray();
        var propMap = Enumerable.Range(0, propCount).ToArray();
        for (var repetition = 0; repetition < expansionCount; repetition++)
        {
            for (var throwIndex = 0; throwIndex < orderedThrows.Length; throwIndex++)
            {
                var throwEvent = orderedThrows[throwIndex];
                expandedThrows.Add(throwEvent with
                {
                    Time = throwEvent.Time!.Value + repetition * diagram.StepCount,
                    From = limbMap[throwEvent.From!.Value],
                    To = limbMap[throwEvent.To!.Value],
                    Prop = propMap[firstPeriodProps[throwIndex]],
                });
            }

            limbMap = limbMap.Select(index => limbPermutation[index]).ToArray();
            propMap = propMap.Select(index => propPermutation[index]).ToArray();
        }

        return new JifPattern
        {
            Meta = new JifMeta { Name = name ?? "Causal diagram pattern", Generator = "Siteswap Generator" },
            HighLevelDescription = new JifHighLevelDescription
            {
                Type = patternType,
                Description = description ?? $"{diagram.Jugglers.Count} jugglers, period {diagram.StepCount}",
            },
            Jugglers = diagram.Jugglers
                .Select((juggler, index) => CreateJuggler(juggler.Name, index, diagram.Jugglers.Count))
                .ToList(),
            Limbs = diagram.Hands.Select(hand => new JifLimb
            {
                Juggler = diagram.Jugglers.IndexOf(hand.Person),
                Type = hand.Name == "L" ? "left hand" : "right hand",
            }).ToList(),
            Props = Enumerable.Range(0, propCount).Select(index => new JifObject
            {
                Color = PropColors[index % PropColors.Length],
                Type = propType,
            }).ToList(),
            TimeStretchFactor = diagram.TimeStretchFactor,
            Valid = true,
            Repetition = new JifRepetition
            {
                Period = diagram.StepCount * expansionCount,
                LimbPermutation = Enumerable.Range(0, diagram.Hands.Count).ToArray(),
                PropPermutation = Enumerable.Range(0, propCount).ToArray(),
            },
            Throws = expandedThrows,
        };
    }

    private static (int[] FirstPeriodProps, int[] PropPermutation, int PropCount) AssignPropPermutation(
        IReadOnlyList<JifThrow> throws,
        IReadOnlyList<int> limbPermutation,
        int period,
        int limbCount
    )
    {
        const int maxIterations = 512;
        var holdings = Enumerable.Range(0, limbCount).Select(_ => new Queue<int>()).ToArray();
        var catches = new PriorityQueue<(int Target, int Prop), (decimal Time, long Order)>();
        var propOrder = new List<int>();
        var firstPeriodProps = new int[throws.Count];
        var propMappings = new Dictionary<int, int>();
        var propCount = 0;
        var unassignedCount = 1;
        var eventOrder = 0L;
        var limbMap = Enumerable.Range(0, limbCount).ToArray();

        for (var iteration = 0; iteration < maxIterations; iteration++)
        {
            if (iteration > 0 && unassignedCount == 0 && propMappings.Count == propCount)
            {
                return (
                    firstPeriodProps,
                    Enumerable.Range(0, propCount).Select(index => propMappings[index]).ToArray(),
                    propCount
                );
            }

            unassignedCount = 0;
            for (var throwIndex = 0; throwIndex < throws.Count; throwIndex++)
            {
                var throwEvent = throws[throwIndex];
                var time = throwEvent.Time!.Value + iteration * period;
                while (catches.TryPeek(out var landing, out var priority) && priority.Time <= time)
                {
                    catches.Dequeue();
                    holdings[landing.Target].Enqueue(landing.Prop);
                }

                var from = limbMap[throwEvent.From!.Value];
                var to = limbMap[throwEvent.To!.Value];
                int prop;
                if (holdings[from].Count > 0)
                {
                    prop = holdings[from].Dequeue();
                }
                else
                {
                    prop = propCount++;
                    unassignedCount++;
                }
                if (iteration == 0)
                {
                    firstPeriodProps[throwIndex] = prop;
                }
                else
                {
                    var previousProp = propOrder[propOrder.Count - throws.Count];
                    if (propMappings.TryGetValue(previousProp, out var mappedProp))
                    {
                        if (mappedProp != prop)
                            throw new InvalidOperationException("The passing pattern does not have a consistent periodic prop assignment.");
                    }
                    else
                    {
                        propMappings.Add(previousProp, prop);
                    }
                }

                propOrder.Add(prop);
                catches.Enqueue((to, prop), (time + throwEvent.Duration!.Value, eventOrder++));
            }

            limbMap = limbMap.Select(index => limbPermutation[index]).ToArray();
        }

        throw new InvalidOperationException("Passist could not find a repeating prop assignment within 512 pattern periods.");
    }

    private static int PermutationOrder(IReadOnlyList<int> permutation)
    {
        var visited = new bool[permutation.Count];
        var order = 1;
        for (var index = 0; index < permutation.Count; index++)
        {
            if (visited[index])
                continue;

            var length = 0;
            for (var current = index; !visited[current]; current = permutation[current])
            {
                visited[current] = true;
                length++;
            }

            order = LeastCommonMultiple(order, length);
        }

        return order;
    }

    private static int LeastCommonMultiple(int left, int right) =>
        left / GreatestCommonDivisor(left, right) * right;

    private static int GreatestCommonDivisor(int left, int right) =>
        right == 0 ? Math.Abs(left) : GreatestCommonDivisor(right, left % right);

    private static JifJuggler CreateJuggler(string name, int index, int numberOfJugglers)
    {
        if (numberOfJugglers == 1)
            return new JifJuggler { Name = name, Position = [0, 0, 0], LookAt = [0, 0, 1] };

        var radius = 1.2 + numberOfJugglers * 0.2;
        var angle = 2 * Math.PI * index / numberOfJugglers;
        return new JifJuggler
        {
            Name = name,
            Position = [RoundLikePassist(radius * Math.Cos(angle)), 0, RoundLikePassist(radius * Math.Sin(angle))],
            LookAt = [0, 0, 0],
        };
    }

    private static decimal RoundLikePassist(double value) =>
        decimal.Round((decimal)value, 3, MidpointRounding.AwayFromZero);
}
