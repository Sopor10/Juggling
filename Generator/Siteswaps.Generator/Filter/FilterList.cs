using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Filter;

internal class FilterList : ISiteswapFilter
{

    private List<ISiteswapFilter> Filters { get; }

    public FilterList(IEnumerable<ISiteswapFilter> filters)
    {
        Filters = filters.ToList();
    }

    public FilterList(params ISiteswapFilter?[] filter) : this(filter.WhereNotNull().AsEnumerable())
    {
            
    }
    public bool CanFulfill(IPartialSiteswap value) => Filters.All(x => x.CanFulfill(value));
}