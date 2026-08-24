namespace Siteswaps.Generator.Components.Feeding;

/// <summary>
/// Starting club counts per hand for one juggler in a two-person siteswap, derived from
/// the stable state (same bit layout as the classic feed UI).
/// </summary>
public static class StartingClubDistribution
{
    public static ClubHands ForJuggler(IReadOnlyList<int> heights, int juggler)
    {
        var positions = StableStatePositions(heights);
        positions.Reverse();
        var right = positions.Where((_, i) => Mod(i - juggler, 4) == 0).Count(x => x);
        var left = positions.Where((_, i) => Mod(i + 2 - juggler, 4) == 0).Count(x => x);
        return new ClubHands(left, right);
    }

    private static List<bool> StableStatePositions(IReadOnlyList<int> heights)
    {
        var state = 0u;
        var stable = false;
        while (!stable)
        {
            var previous = state;
            foreach (var height in heights)
            {
                state >>= 1;
                if (height > 0)
                {
                    state |= 1u << (height - 1);
                }
            }

            stable = state == previous;
        }

        if (state == 0)
        {
            return [];
        }

        var bits = new List<bool>();
        var foundTrue = false;
        for (var index = 31; index >= 0; index--)
        {
            var bit = (state & (1u << index)) != 0;
            if (bit)
            {
                foundTrue = true;
            }

            if (foundTrue)
            {
                bits.Add(bit);
            }
        }

        return bits;
    }

    private static int Mod(int value, int modulus)
    {
        var result = value % modulus;
        return result < 0 ? result + modulus : result;
    }
}
