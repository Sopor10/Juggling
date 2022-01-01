using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Filter;

internal class OrFilter : ISiteswapFilter
{

    private List<ISiteswapFilter> Filters { get; }

    public OrFilter(IEnumerable<ISiteswapFilter> filters)
    {
        Filters = filters.ToList();
    }

    public OrFilter(params ISiteswapFilter?[] filter) : this(filter.WhereNotNull().AsEnumerable())
    {
            
    }
    public bool CanFulfill(IPartialSiteswap value) => Filters.Any(x => x.CanFulfill(value));
}