using System.Linq;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Filter;

internal class AverageToHighFilter : ISiteswapFilter
{
    public bool CanFulfill(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput)
    {
        var minAdditionalValue =
            value.Items.Count(x => x == PartialSiteswap.Free) * siteswapGeneratorInput.MinHeight; 
            
        var average = (value.Items.Where(x => x >= 0).Sum() * 1.0 + minAdditionalValue) / value.Period();
        return average <= siteswapGeneratorInput.NumberOfObjects;
    }
}