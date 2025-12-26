namespace Siteswap.Details;

public record LocalSiteswap(Siteswap Siteswap, int Juggler, int NumberOfJugglers)
{
    public string GlobalNotation => ToString();
    public string LocalNotation =>
        string.Join(
            " ",
            GetLocalSiteswapReal()
                .Select(x => x * 1.0 / NumberOfJugglers)
                .Select(x => x.ToString("0.##"))
        );

    internal List<int> GetLocalSiteswapReal()
    {
        var result = new List<int>();

        var siteswap = Siteswap.Items.ToCyclicArray();
        for (var i = 0; i < Siteswap.Period.GetLocalPeriod(NumberOfJugglers).Value; i++)
        {
            result.Add(siteswap[Juggler + i * NumberOfJugglers]);
        }

        return result;
    }

    public override string ToString() => GetLocalSiteswapReal().ToSiteswapString();

    public double Average() => GetLocalSiteswapReal().Average() * 1.0 / NumberOfJugglers;

    public bool IsValidAsGlobalSiteswap()
    {
        var items = GetLocalSiteswapReal();

        return items.Select((x, i) => (x + i) % items.Count).ToHashSet().Count == items.Count;
    }

    public static Result<Siteswap> FromLocals(params IEnumerable<LocalSiteswap> input)
    {
        return FromLocals(input.Select(x => (IList<int>)x.GetLocalSiteswapReal()).ToList());
    }

    public static Result<Siteswap> FromLocals(IList<IList<int>> input)
    {
        var lcm = input.Select(x => x.Count).Aggregate(Helper.Lcm);

        var numberOfJugglers = input.Count;
        var globalPeriod = lcm * numberOfJugglers;
        var globalItems = new int[globalPeriod];

        for (var j = 0; j < numberOfJugglers; j++)
        {
            var data = input[j];
            for (var i = 0; j + i * numberOfJugglers < globalPeriod; i++)
            {
                globalItems[j + i * numberOfJugglers] = data[i];
            }
        }

        if (Siteswap.TryCreate(globalItems, out var siteswap))
        {
            return siteswap;
        }

        return "Invalid global siteswap.";
    }
}
