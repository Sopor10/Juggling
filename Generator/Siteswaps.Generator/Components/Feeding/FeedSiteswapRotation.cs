using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Components.Feeding;

internal static class FeedSiteswapRotation
{
    public static Siteswap Rotate(Siteswap siteswap, int steps)
    {
        var items = siteswap.Items;
        var period = items.Length;
        if (period == 0 || steps % period == 0)
        {
            return siteswap;
        }

        var offset = ((steps % period) + period) % period;
        var rotated = new int[period];
        for (var i = 0; i < period; i++)
        {
            rotated[i] = items[(i + offset) % period];
        }

        return Siteswap.CreateFromCorrect(rotated);
    }

    /// <summary>
    /// Whether <paramref name="reference"/> equals some rotation of <paramref name="candidate"/>.
    /// Used when the session stores an interface-aligned rotation while lists keep canonical globals.
    /// </summary>
    public static bool IsRotationOf(Siteswap reference, Siteswap candidate)
    {
        var period = candidate.Items.Length;
        if (reference.Items.Length != period)
        {
            return false;
        }

        for (var offset = 0; offset < period; offset++)
        {
            if (reference.Equals(Rotate(candidate, offset)))
            {
                return true;
            }
        }

        return false;
    }

    public static void RotateInPlace<T>(T[] values, int steps)
    {
        var period = values.Length;
        if (period == 0 || steps % period == 0)
        {
            return;
        }

        var offset = ((steps % period) + period) % period;
        var copy = values.ToArray();
        for (var i = 0; i < period; i++)
        {
            values[i] = copy[(i + offset) % period];
        }
    }
}
