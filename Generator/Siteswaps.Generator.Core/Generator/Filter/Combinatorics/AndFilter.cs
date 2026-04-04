namespace Siteswaps.Generator.Core.Generator.Filter.Combinatorics;

public class AndFilter : ISiteswapFilter
{
    private List<ISiteswapFilter> Filters { get; }
    private readonly bool _isRotationAware;

    public AndFilter(IEnumerable<ISiteswapFilter> filters)
    {
        Filters = filters.OrderBy(x => x.Order).ToList();
        _isRotationAware = Filters.Any(f => f.IsRotationAware);
    }

    public AndFilter(params ISiteswapFilter?[] filter)
        : this(filter.WhereNotNull().AsEnumerable()) { }

    public bool CanFulfill(PartialSiteswap value)
    {
        foreach (var filter in Filters)
        {
            if (filter.CanFulfill(value) is false)
            {
                return false;
            }
        }

        return true;
    }

    public int Order => 0;
    public bool IsRotationAware => _isRotationAware;
}

public class NotFilter(ISiteswapFilter filter) : ISiteswapFilter
{
    public bool CanFulfill(PartialSiteswap value)
    {
        if (value.IsFilled() is false)
        {
            return true;
        }

        return filter.CanFulfill(value) is false;
    }

    public int Order => 0;
    public bool IsRotationAware => filter.IsRotationAware;
}
