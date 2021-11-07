using System.Linq;

namespace Siteswaps.Generator.Filter
{
    public class CollisionFilter : ISiteswapFilter
    {
        public bool CanFulfill(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput)
        {
            var result = value.Items.TakeWhile(x => x != PartialSiteswap.Free).Select((height, pos) => (height + pos) % value.Items.Count).ToList();
            return result.Distinct().Count() == result.Count;
        }
    }
}