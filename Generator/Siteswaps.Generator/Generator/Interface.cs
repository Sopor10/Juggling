namespace Siteswaps.Generator.Generator;

using Shared;

public record Interface(CyclicArray<sbyte> Values)
{
    public static Interface FromSiteswap(Siteswap siteswap)
    {
        var values = siteswap.Values;
        foreach (var (i, value) in siteswap.Values.Enumerate(1))
        {
            values[i + value] = value ;
        }
        return new(values);
    }
}
