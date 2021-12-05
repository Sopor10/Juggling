using System.Linq;

namespace Siteswaps.Generator.Filter;

internal class AverageToLowFilter : ISiteswapFilter
{
    public bool CanFulfill(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput)
    {
        if (value.IsFilled())
        {
            return true;
        }
            
        var maxAdditionalValue =
            value.Items.Count(x => x == PartialSiteswap.Free) * siteswapGeneratorInput.MaxHeight; 

        var average = (value.Items.Where(x => x >= 0).Sum() * 1.0 + maxAdditionalValue)/ value.Period();
        return average >= siteswapGeneratorInput.NumberOfObjects;
    }
}