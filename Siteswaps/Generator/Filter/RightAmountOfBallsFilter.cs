using System;
using System.Linq;

namespace Siteswaps.Generator.Filter;

internal class RightAmountOfBallsFilter : ISiteswapFilter
{
    public bool CanFulfill(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput)
    {
        if (!value.IsFilled()) return true;

        return Math.Abs(value.Items.Average() - siteswapGeneratorInput.NumberOfObjects) < 0.001;

    }
}