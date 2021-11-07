using System.Linq;

namespace Siteswaps.Generator.Filter
{
    public class AtLeastXXXTimesFilter : NumberFilter
    {
        private protected override bool CanFulfillNumberFilter(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput)
        {
            return value.Items.Count(x => x == Number || x == PartialSiteswap.Free) >= Number;
        }

        public AtLeastXXXTimesFilter(int number, int amount) : base(number, amount)
        {
        }
    }
}