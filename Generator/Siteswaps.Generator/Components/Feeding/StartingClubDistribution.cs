using Siteswaps.Generator.Components.SiteswapLab;

namespace Siteswaps.Generator.Components.Feeding;

/// <summary>
/// Starting club counts per hand, derived from cyclic pattern simulation.
/// </summary>
public static class StartingClubDistribution
{
    /// <summary>
    /// <para>
    /// Computes how many clubs each hand holds at pattern start for one juggler in a
    /// <em>global</em> multi-hand siteswap sequence (interleaved throws from all jugglers,
    /// e.g. a feeder pattern).
    /// </para>
    /// <para><b>Algorithm (passist-style start-props simulation):</b></para>
    /// <list type="number">
    /// <item>
    /// Every valid siteswap is cyclic and eventually reaches a stable state where each hand
    /// holds exactly zero or one club. When a hand is empty, the next throw from that hand
    /// must be height zero until a club returns.
    /// </item>
    /// <item>
    /// To find the starting distribution, simulate throwing the pattern forward from an empty
    /// pattern start, tracking which global beat indices receive a landing.
    /// </item>
    /// <item>
    /// Continue until every object in the pattern has been assigned a starting club (each
    /// object is thrown at least once; in practice the passist loop decrements a remaining-object
    /// counter whenever a throw needs a club that has no scheduled landing at that beat).
    /// </item>
    /// <item>
    /// At pattern start, any throw beat without a prior landing on that hand slot needs a club
    /// in that hand — the count of such beats per juggler equals that juggler's total clubs.
    /// </item>
    /// <item>
    /// Split left/right by assuming alternating hands per juggler, starting with the
    /// <b>right</b> hand (mirrors passist <c>defaultLimbs</c> / <c>PassingHandOrder</c>).
    /// </item>
    /// </list>
    /// </summary>
    /// <param name="heights">Global throw heights, one per interleaved beat.</param>
    /// <param name="juggler">Zero-based juggler index (time-zone layer).</param>
    /// <param name="numberOfJugglers">Number of jugglers in the interleaved sequence.</param>
    public static ClubHands ForJuggler(
        IReadOnlyList<int> heights,
        int juggler,
        int numberOfJugglers = 1
    )
    {
        ArgumentOutOfRangeException.ThrowIfNegative(juggler);
        ArgumentOutOfRangeException.ThrowIfLessThan(numberOfJugglers, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(juggler, numberOfJugglers);

        if (heights.Count == 0)
        {
            return new ClubHands(0, 0);
        }

        var nLimbs = numberOfJugglers * 2;
        var limbs = BuildLimbs(numberOfJugglers);
        var missing = (int)heights.Average(x => (double)x);
        var startLimbs = SimulateGlobalSiteswapStartLimbs(heights, nLimbs, missing);

        var left = 0;
        var right = 0;
        for (var limb = 0; limb < nLimbs; limb++)
        {
            if (limbs[limb].Juggler != juggler)
            {
                continue;
            }

            if (limbs[limb].IsRight)
            {
                right += startLimbs[limb];
            }
            else
            {
                left += startLimbs[limb];
            }
        }

        return new ClubHands(left, right);
    }

    /// <summary>
    /// Computes starting clubs for one person in a passing-editor pattern using the full
    /// multi-person context (cross-person landings, shared time zones, global heights).
    /// </summary>
    /// <para>
    /// Uses the same simulation idea as <see cref="ForJuggler"/>, but throw beats are taken
    /// from each person's local cells, landings from <see cref="PassingEditorState.LandingFor"/>,
    /// and simultaneous throws at the same global phase (people sharing a time zone) are
    /// processed in person-index order.
    /// </para>
    public static ClubHands ForPerson(PassingEditorState state, int person)
    {
        return ComputeAllPersonStarts(state)[Math.Clamp(person, 0, state.People.Count - 1)];
    }

    internal static ClubHands[] ComputeAllPersonStarts(PassingEditorState state)
    {
        var people = state.People;
        var personCount = people.Count;
        if (personCount == 0)
        {
            return [];
        }

        var period = state.Period;
        if (period == 0)
        {
            return people.Select(_ => new ClubHands(0, 0)).ToArray();
        }

        var tzCount = state.ActiveTimeZoneCount;
        var globalPeriod = period * tzCount;
        var beatOriginOffset = state.BeatOriginOffset;
        var totalObjects =
            people.SelectMany(person => person.Cells).Sum(cell => cell.Height) / globalPeriod;

        var startLeft = new int[personCount];
        var startRight = new int[personCount];
        var landingScheduled = new HashSet<(int GlobalBeat, int Person, bool IsRight)>();

        var missing = totalObjects;
        var globalBeat = 0;
        while (missing > 0)
        {
            var phase = globalBeat % tzCount;
            var patternBeat = PositiveModulo((globalBeat / tzCount) - beatOriginOffset, period);

            for (var personIndex = 0; personIndex < personCount; personIndex++)
            {
                var person = people[personIndex];
                if (person.TimeZone != phase)
                {
                    continue;
                }

                var cell = person.Cells[patternBeat];
                var height = cell.Height;
                if (height <= 0)
                {
                    continue;
                }

                var isRight = patternBeat % 2 == 0;
                if (!landingScheduled.Contains((globalBeat, personIndex, isRight)))
                {
                    if (isRight)
                    {
                        startRight[personIndex]++;
                    }
                    else
                    {
                        startLeft[personIndex]++;
                    }

                    missing--;
                }

                var landingBeat = state.LandingBeatForHeight(personIndex, patternBeat, height);
                var landingTimeZone = PositiveModulo(person.TimeZone + height, tzCount);
                var landingGlobalBeat = landingBeat * tzCount + landingTimeZone;
                var landingIsRight = landingBeat % 2 == 0;
                var targetPerson = cell.TargetPerson ?? personIndex;
                landingScheduled.Add((landingGlobalBeat, targetPerson, landingIsRight));
            }

            globalBeat++;
        }

        return Enumerable
            .Range(0, personCount)
            .Select(index => new ClubHands(startLeft[index], startRight[index]))
            .ToArray();
    }

    private static int[] SimulateGlobalSiteswapStartLimbs(
        IReadOnlyList<int> heights,
        int nLimbs,
        int missing
    )
    {
        var startLimbs = new int[nLimbs];
        var hasProp = new HashSet<int>();

        for (var beat = 0; missing > 0; beat++)
        {
            var height = heights[beat % heights.Count];
            if (height <= 0)
            {
                continue;
            }

            if (!hasProp.Contains(beat))
            {
                startLimbs[beat % nLimbs]++;
                missing--;
            }

            hasProp.Add(beat + height);
        }

        return startLimbs;
    }

    private static LimbAssignment[] BuildLimbs(int numberOfJugglers)
    {
        var limbs = new LimbAssignment[numberOfJugglers * 2];
        for (var index = 0; index < limbs.Length; index++)
        {
            var juggler = index % numberOfJugglers;
            var isRight = numberOfJugglers % 2 != 0 ? index % 2 == 0 : index < numberOfJugglers;
            limbs[index] = new LimbAssignment(juggler, isRight);
        }

        return limbs;
    }

    private static int PositiveModulo(int value, int modulus) =>
        (value % modulus + modulus) % modulus;

    private readonly record struct LimbAssignment(int Juggler, bool IsRight);
}
