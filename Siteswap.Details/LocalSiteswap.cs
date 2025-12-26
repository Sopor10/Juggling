using Dunet;

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

    public static Result<Siteswap> FromLocals(IEnumerable<LocalSiteswap> input)
    {
        var locals = input.ToList();
        if (locals.Count == 0)
        {
            return "No local siteswaps provided.";
        }

        var numberOfJugglers = locals[0].NumberOfJugglers;
        if (locals.Any(x => x.NumberOfJugglers != numberOfJugglers))
        {
            return "Different number of jugglers.";
        }

        if (locals.Count != numberOfJugglers)
        {
            return $"Expected {numberOfJugglers} local siteswaps, but got {locals.Count}.";
        }

        if (locals.Select(x => x.Juggler).Distinct().Count() != numberOfJugglers)
        {
            return "Juggler indices are not unique or incomplete.";
        }

        var localData = locals
            .OrderBy(x => x.Juggler)
            .Select(x => x.GetLocalSiteswapReal())
            .ToList();
        if (localData.Select(x => x.Count).Distinct().Count() > 1)
        {
            return "Local siteswaps have different lengths.";
        }

        if (
            locals
                .Select(x => x.Siteswap.Interface.AsPassOrSelf(numberOfJugglers))
                .Distinct()
                .Count() != 1
        )
        {
            return "Local siteswaps have different interfaces.";
        }

        var localPeriod = localData[0].Count;
        var globalPeriod = localPeriod * numberOfJugglers;
        var globalItems = new int[globalPeriod];

        for (var j = 0; j < numberOfJugglers; j++)
        {
            var data = localData[j];
            for (var i = 0; i < localPeriod; i++)
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

[Union]
public partial record Result<T>
{
    public partial record Success(T Value);

    public partial record Failure(string Error);
}
