namespace Siteswaps.Generator.Core.Generator.Filter;

public class NumberOfPassesFilter(
    int numberOfPasses,
    int numberOfJugglers,
    SiteswapGeneratorInput generatorInput
) : ISiteswapFilter
{
    private readonly HashSet<int> passValues = Enumerable
        .Range(0, generatorInput.MaxHeight + 1)
        .Where(x => x % numberOfJugglers != 0)
        .ToHashSet();


    public bool CanFulfill(PartialSiteswap value)
    {
        var numberOfPassesSoFar = value.Items.Count(x => passValues.Contains(x));

        if (value.IsFilled()) return numberOfPassesSoFar == numberOfPasses;
        return numberOfPassesSoFar <= numberOfPasses;
    }

    public int Order => 0;
}