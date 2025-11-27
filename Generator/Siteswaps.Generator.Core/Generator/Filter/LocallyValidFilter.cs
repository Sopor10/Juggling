namespace Siteswaps.Generator.Core.Generator.Filter;

public class LocallyValidFilter(int numberOfJugglers, int juggler) : ISiteswapFilter
{
    private int NumberOfJugglers { get; } = numberOfJugglers;
    private int Juggler { get; } = juggler;

    public bool CanFulfill(PartialSiteswap value)
    {
        if (!value.IsFilled())
        {
            return true;
        }

        var siteswap = Siteswap.CreateFromCorrect(value.Items);
        var localSiteswap = siteswap.GetLocalSiteswap(Juggler, NumberOfJugglers);
        return localSiteswap.IsValidAsGlobalSiteswap();
    }

    public int Order => 2;
}
