using System.Linq;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Filter;

internal class CollisionFilter : ISiteswapFilter
{
    public bool CanFulfill(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput)
    {
        var bools = new bool [ value.Items.Count ];

        var currentIndex = value.LastFilledPosition;
        for (var i = 0; i <= currentIndex; i++)
        {
            var count = (i + value.Items[i]) % value.Items.Count;
            bools[count] = true;
        }

        return bools.Count(x => x) == currentIndex + 1;
    }
}
