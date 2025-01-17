﻿namespace Siteswaps.Generator.Generator.Filter.Combinatorics;

internal class OrFilter : ISiteswapFilter
{
    private List<ISiteswapFilter> Filters { get; }

    public OrFilter(IEnumerable<ISiteswapFilter> filters)
    {
        Filters = filters.ToList();
    }

    public OrFilter(params ISiteswapFilter?[] filter)
        : this(filter.WhereNotNull().AsEnumerable()) { }

    public bool CanFulfill(PartialSiteswap value) => Filters.Any(x => x.CanFulfill(value));
}
