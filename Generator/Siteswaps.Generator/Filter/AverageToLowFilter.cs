using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Filter;

internal class AverageToLowFilter : ISiteswapFilter
{
    public AverageToLowFilter(SiteswapGeneratorInput generatorInput)
    {
        GeneratorInput = generatorInput;
    }

    private SiteswapGeneratorInput GeneratorInput { get; }

    public bool CanFulfill(IPartialSiteswap value)
    {
        if (value.IsFilled())
        {
            return true;
        }
            
        var maxAdditionalValue =
            value.Items.Count(x => x == PartialSiteswap.Free) * GeneratorInput.MaxHeight; 

        var average = (value.Items.Where(x => x >= 0).Sum() * 1.0 + maxAdditionalValue)/ GeneratorInput.Period;
        return average >= GeneratorInput.NumberOfObjects;
    }
}