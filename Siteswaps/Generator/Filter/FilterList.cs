using System.Collections.Generic;
using System.Linq;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Filter;

internal class FilterList : ISiteswapFilter
{

    private List<ISiteswapFilter> Filters { get; }

    private FilterList(IEnumerable<ISiteswapFilter> filters)
    {
        Filters = filters.ToList();
    }

    public FilterList(params ISiteswapFilter[] filter) : this(filter.AsEnumerable())
    {
            
    }
    public bool CanFulfill(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput) => Filters.All(x => x.CanFulfill(value, siteswapGeneratorInput));
}