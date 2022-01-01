using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain.Filter;

internal class AndFilter : ISiteswapFilter
{

    private List<ISiteswapFilter> Filters { get; }

    public AndFilter(IEnumerable<ISiteswapFilter> filters)
    {
        Filters = filters.ToList();
    }

    public AndFilter(params ISiteswapFilter?[] filter) : this(filter.WhereNotNull().AsEnumerable())
    {
            
    }
    public bool CanFulfill(IPartialSiteswap value) => Filters.All(x => x.CanFulfill(value));
}