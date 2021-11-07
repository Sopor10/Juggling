using System;
using System.Linq;

namespace Siteswaps.Generator.Filter
{
    public class AverageToHighFilter : ISiteswapFilter
    {
        public bool CanFulfill(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput)
        {
            var minAdditionalValue =
                value.Items.Count(x => x == PartialSiteswap.Free) * siteswapGeneratorInput.MinHeight; 
            
            var average = (value.Items.Where(x => x >= 0).Sum() * 1.0 + minAdditionalValue) / value.Period();
            return average <= siteswapGeneratorInput.NumberOfObjects;
        }
    }
    
    public class RightAmountOfBallsFilter : ISiteswapFilter
    {
        public bool CanFulfill(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput)
        {
            if (!value.IsFilled()) return true;

            return Math.Abs(value.Items.Average() - siteswapGeneratorInput.NumberOfObjects) < 0.001;

        }
    }
}
