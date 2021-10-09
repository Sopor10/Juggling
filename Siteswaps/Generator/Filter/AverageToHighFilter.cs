using System.Linq;

namespace Siteswaps.Generator.Filter
{
    public class AverageToHighFilter : ISiteswapFilter
    {
        public bool CanFulfill(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput)
        {
            var average = value.Items.Where(x => x >= 0).Sum() * 1.0 / value.Period();
            return average <= siteswapGeneratorInput.NumberOfObjects;
        }
    }
}