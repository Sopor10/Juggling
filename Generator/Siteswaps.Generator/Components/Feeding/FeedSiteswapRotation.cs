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
