namespace Siteswaps.Generator.Generator;

using Shared;

public record Interface(CyclicArray<sbyte> Values)
{
    public static Interface From(Siteswap siteswap)
    {
        var result = new CyclicArray<sbyte>(Enumerable.Repeat((sbyte) 0, siteswap.Values.Length));
        foreach (var (i, value) in siteswap.Values.Enumerate(1))
        {
            result[i + value] = value;
        }

        return new(result);
    }
    
    public static Interface From(LocalSiteswap localSiteswap)
    {
        var result = From(localSiteswap.Siteswap);

        return result.GetLocal(localSiteswap.Siteswap, localSiteswap.Juggler, localSiteswap.NumberOfJugglers);
    }

    private Interface GetLocal(Siteswap siteswap, int juggler, int numberOfJugglers)
    {
        var result = new List<sbyte>();

        for (var i = 0; i < siteswap.LocalPeriod(numberOfJugglers).Value; i++)
        {
            result.Add(this.Values[juggler + i * (numberOfJugglers)]);
        }

        return new Interface(new CyclicArray<sbyte>(result));
    }
    
}
