namespace Siteswaps.Generator.Core.Generator.Filter;

internal class NumberOfPassesFilter(
    int numberOfPasses,
    int numberOfJugglers,
    SiteswapGeneratorInput generatorInput
) : ISiteswapFilter
{
    private int NumberOfPasses { get; } = numberOfPasses;
    private int NumberOfJugglers { get; } = numberOfJugglers;
    private SiteswapGeneratorInput GeneratorInput { get; } = generatorInput;

    public bool CanFulfill(PartialSiteswap value)
    {
        var passValues = Enumerable
            .Range(0, GeneratorInput.MaxHeight)
            .Where(x => x % NumberOfJugglers != 0)
            .ToHashSet();
        var numberOfPassesSoFar = value.Items.Count(x => passValues.Contains(x));

        if (value.IsFilled())
        {
            return numberOfPassesSoFar == NumberOfPasses;
        }
        return numberOfPassesSoFar <= NumberOfPasses;
    }

    public int Order => 0;
}
