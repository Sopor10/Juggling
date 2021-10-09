using System.Collections.Generic;
using System.Linq;

namespace Siteswaps.Generator.Filter
{
    public class FilterList : ISiteswapFilter
    {

        public IEnumerable<ISiteswapFilter> Filters { get; }
        public FilterList(IEnumerable<ISiteswapFilter> filters)
        {
            Filters = filters;
        }

        public FilterList(params ISiteswapFilter[] filter) : this(filter.AsEnumerable())
        {
            
        }
        public bool CanFulfill(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput) => Filters.All(x => x.CanFulfill(value, siteswapGeneratorInput));
    }
}