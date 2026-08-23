namespace Siteswaps.Generator.Core.Generator.Filter;

internal sealed class NumberOfPassesFilter(int numberOfPasses, int numberOfJugglers)
    : ISiteswapFilter
{
    private readonly int _numberOfJugglers = numberOfJugglers;

    public bool CanFulfill(PartialSiteswap value)
    {
        int numberOfPassesSoFar = 0;
        for (var index = 0; index < value.Length; index++)
        {
            var x = value.Items[index];
            if (x >= 0 && x % _numberOfJugglers != 0 && ++numberOfPassesSoFar > numberOfPasses)
                return false;
        }

        if (value.IsFilled())
            return numberOfPassesSoFar == numberOfPasses;
        return numberOfPassesSoFar <= numberOfPasses;
    }

    public int Order => 0;
}
