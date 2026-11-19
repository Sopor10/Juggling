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

    /// <summary>Maps the Siteswap Lab's phased, person-to-person throw grid to JIF.</summary>
    public static JifPattern MapPassing(
        IReadOnlyList<int> timeZones,
        IReadOnlyList<IReadOnlyList<int>> heights,
        IReadOnlyList<IReadOnlyList<int>> targets,
        string propType = "club"
    )
    {
        ArgumentNullException.ThrowIfNull(timeZones);
        ArgumentNullException.ThrowIfNull(heights);
        ArgumentNullException.ThrowIfNull(targets);
        ArgumentException.ThrowIfNullOrWhiteSpace(propType);
        if (timeZones.Count == 0 || heights.Count != timeZones.Count || targets.Count != timeZones.Count)
            throw new ArgumentException("Time zones, heights, and targets must describe the same non-empty set of jugglers.");

        var period = heights[0].Count;
        if (period == 0 || heights.Any(row => row.Count != period) || targets.Any(row => row.Count != period))
            throw new ArgumentException("Every juggler must have the same non-empty number of beats.");
        if (timeZones.Any(zone => zone < 0))
            throw new ArgumentOutOfRangeException(nameof(timeZones), "Time zones cannot be negative.");

        var distinctZones = timeZones.Distinct().Order().ToArray();
        var timeStretch = Math.Max(1, distinctZones.Length);
        var normalizedTimeZones = timeZones
            .Select(zone => Array.IndexOf(distinctZones, zone))
            .ToArray();
        if (targets.SelectMany(row => row).Any(target => target < 0 || target >= timeZones.Count))
            throw new ArgumentOutOfRangeException(nameof(targets), "Every throw must target a known juggler.");

        var limbs = timeZones.SelectMany((_, person) => new[]
        {
            new JifLimb { Juggler = person, Type = "right hand" },
            new JifLimb { Juggler = person, Type = "left hand" },
        }).ToList();

        var throws = new List<JifThrow>(timeZones.Count * period);
        for (var person = 0; person < timeZones.Count; person++)
        {
            for (var beat = 0; beat < period; beat++)
            {
                var height = heights[person][beat];
                if (height < 0)
                    throw new ArgumentOutOfRangeException(nameof(heights), "Throw heights cannot be negative.");
                if (height == 0)
                    continue;

                var time = beat * timeStretch + normalizedTimeZones[person];
                var landingTimeZone = (normalizedTimeZones[person] + height) % timeStretch;
                if (landingTimeZone != normalizedTimeZones[targets[person][beat]])
                    throw new ArgumentException($"Throw from juggler {person} at beat {beat} cannot land in the selected juggler's time zone.", nameof(targets));
                var landingLocalBeat = beat + (normalizedTimeZones[person] + height) / timeStretch;
                var from = person * 2 + beat % 2;
                var to = targets[person][beat] * 2 + landingLocalBeat % 2;
                throws.Add(new JifThrow
                {
                    Time = time,
                    Duration = height,
                    From = from,
                    To = to,
                    Label = global::Siteswap.Details.Siteswap.Transform(height),
                });
            }
        }

        if (throws.Count == 0)
            throw new ArgumentException("The pattern must contain at least one positive-height throw.", nameof(heights));

        var basePeriod = period * timeStretch;
        var orderedThrows = throws.OrderBy(throwEvent => throwEvent.Time).ToArray();
        var limbPermutation = Enumerable.Range(0, timeZones.Count * 2)
            .Select(limb => limb / 2 * 2 + (limb % 2 + period % 2) % 2)
            .ToArray();
        var (firstPeriodProps, propPermutation, propCount) = AssignPropPermutation(
            orderedThrows,
            limbPermutation,
            basePeriod,
            timeZones.Count * 2
        );
        var expansionCount = LeastCommonMultiple(
            PermutationOrder(limbPermutation),
            PermutationOrder(propPermutation)
        );
        if (expansionCount > 512 || (long)orderedThrows.Length * expansionCount > 30_000)
            throw new InvalidOperationException("Passist needs an expansion larger than this widget supports for the current pattern.");

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
                    Time = throwEvent.Time!.Value + repetition * basePeriod,
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
            Meta = new JifMeta { Name = "Siteswap Lab passing pattern", Generator = "Siteswap Generator" },
            HighLevelDescription = new JifHighLevelDescription
            {
                Type = "passingSiteswap",
                Description = $"{timeZones.Count} jugglers, period {period}",
            },
            Jugglers = timeZones.Select((_, index) => CreateJuggler($"{(char)('A' + index)}", index, timeZones.Count)).ToList(),
            Limbs = limbs,
            Props = Enumerable.Range(0, propCount).Select(index => new JifObject
            {
                Color = PropColors[index % PropColors.Length],
                Type = propType,
            }).ToList(),
            TimeStretchFactor = timeStretch,
            Valid = true,
            Repetition = new JifRepetition
            {
                Period = basePeriod * expansionCount,
                LimbPermutation = Enumerable.Range(0, timeZones.Count * 2).ToArray(),
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
