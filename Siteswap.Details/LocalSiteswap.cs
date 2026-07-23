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

    private List<int> GetLocalSiteswapReal()
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
}
