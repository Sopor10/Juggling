using System.Linq;

namespace Siteswaps.Generator.Filter
{
    public class ExactlyXXXTimesFilter : NumberFilter
    {
        private protected override bool CanFulfillNumberFilter(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput)
        {
            return value.Items.Count(x => x == Number || x == PartialSiteswap.Free) == Number;
        }

        public ExactlyXXXTimesFilter(int number, int amount) : base(number, amount)
        {
        }
    }
}