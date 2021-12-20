using System.Linq;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Filter;

internal class NumberOfPassesFilter : ISiteswapFilter
{
    public int NumberOfPasses { get; }
    public int NumberOfJugglers { get; }

    public NumberOfPassesFilter(int numberOfPasses, int numberOfJugglers)
    {
        NumberOfPasses = numberOfPasses;
        NumberOfJugglers = numberOfJugglers;
    }

    public bool CanFulfill(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput)
    {
        var passValues = Enumerable.Range(0, siteswapGeneratorInput.MaxHeight).Where(x => x % NumberOfJugglers != 0)
            .ToHashSet();
        var numberOfPassesSoFar = value.Items.Count(x => passValues.Contains(x));

        if (value.IsFilled())
        {
            return numberOfPassesSoFar == NumberOfPasses;
        }
        return numberOfPassesSoFar <= NumberOfPasses;
    }
}

