namespace Siteswaps.Generator.Core.Generator.Filter;

internal sealed class NumberOfPassesFilter(
    int numberOfPasses,
    int numberOfJugglers,
    SiteswapGeneratorInput generatorInput
) : ISiteswapFilter
{
    private readonly HashSet<int> _passValues = CreatePassValues(generatorInput, numberOfJugglers);

    private static HashSet<int> CreatePassValues(
        SiteswapGeneratorInput generatorInput,
        int numberOfJugglers
    )
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numberOfJugglers);
        return Enumerable
            .Range(0, generatorInput.MaxHeight)
            .Where(x => x % numberOfJugglers != 0)
            .ToHashSet();
    }

    public bool CanFulfill(PartialSiteswap value)
    {
        int numberOfPassesSoFar = 0;
        foreach (var x in value.AsSpan())
        {
            if (_passValues.Contains(x) && ++numberOfPassesSoFar > numberOfPasses)
                return false;
        }

        if (value.IsFilled())
            return numberOfPassesSoFar == numberOfPasses;
        return numberOfPassesSoFar <= numberOfPasses;
    }

    public int Order => 0;
}
