using Siteswaps.Generator.Components.State;

namespace Siteswaps.Generator.Components.Feeding;

/// <summary>
/// Maps throw-time Pass/Self labels onto landing (interface) positions using the causal
/// siteswap interface: each throw at beat i lands at i + height.
/// </summary>
public static class FeedInterface
{
    public static IReadOnlyList<Throw> RotateToLanding(
        IReadOnlyList<int> heights,
        IReadOnlyList<Throw> throwTime
    )
    {
        if (heights.Count != throwTime.Count)
        {
            throw new ArgumentException(
                "Heights and throw-time pattern must have the same length."
            );
        }

        var period = heights.Count;
        var landing = new Throw?[period];
        var written = new bool[period];

        for (var i = 0; i < period; i++)
        {
            var landAt = Mod(i + heights[i], period);
            if (written[landAt])
            {
                throw new InvalidOperationException(
                    $"Landing collision at beat {landAt}: throws at beats map to the same interface slot."
                );
            }

            landing[landAt] = throwTime[i];
            written[landAt] = true;
        }

        return landing.Select(t => t ?? Throw.AnySelf).ToList();
    }

    private static int Mod(int value, int modulus)
    {
        var result = value % modulus;
        return result < 0 ? result + modulus : result;
    }
}
