namespace Siteswaps.Generator.Generator.Filter;

internal class NumberOfPassesFilter : ISiteswapFilter
{
    private int NumberOfPasses { get; }
    private int NumberOfJugglers { get; }
    private SiteswapGeneratorInput GeneratorInput { get; }

    public NumberOfPassesFilter(int numberOfPasses, int numberOfJugglers, SiteswapGeneratorInput generatorInput)
    {
        NumberOfPasses = numberOfPasses;
        NumberOfJugglers = numberOfJugglers;
        GeneratorInput = generatorInput;
    }

    public bool CanFulfill(PartialSiteswap value)
    {
        var passValues = Enumerable.Range(0, GeneratorInput.MaxHeight).Where(x => x % NumberOfJugglers != 0)
            .ToHashSet();
        var numberOfPassesSoFar = value.Items.Count(x => passValues.Contains(x));

        if (value.IsFilled())
        {
            return numberOfPassesSoFar == NumberOfPasses;
        }
        return numberOfPassesSoFar <= NumberOfPasses;
    }
}

