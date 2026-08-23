namespace Siteswaps.Generator.Core.Generator.Filter.Combinatorics;

public class AndFilter : ISiteswapFilter
{
    private List<ISiteswapFilter> Filters { get; }
    private List<ISiteswapFilter> RotationInvariantFilters { get; }
    private List<ISiteswapFilter> RotationAwareFilters { get; }
    private List<ISiteswapFilter> RotationAwarePartialFilters { get; }
    private readonly bool _isRotationAware;

    public AndFilter(IEnumerable<ISiteswapFilter> filters)
    {
        Filters = filters.OrderBy(x => x.Order).ToList();
        RotationInvariantFilters = Filters.Where(filter => !filter.IsRotationAware).ToList();
        RotationAwareFilters = Filters.Where(filter => filter.IsRotationAware).ToList();
        RotationAwarePartialFilters = RotationAwareFilters
            .Where(filter => filter.CanRejectPartial)
            .ToList();
        _isRotationAware = RotationAwareFilters.Count > 0;
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

    public bool CanFulfillAnyRotation(PartialSiteswap value)
    {
        foreach (var filter in RotationInvariantFilters)
        {
            if (filter.CanFulfill(value) is false)
            {
                return false;
            }
        }

        if (!value.IsFilled() && RotationAwarePartialFilters.Count == 0)
            return true;

        var rotationFilters = value.IsFilled() ? RotationAwareFilters : RotationAwarePartialFilters;
        var originalRotation = value.RotationIndex;
        for (var rotation = 0; rotation < value.Length; rotation++)
        {
            value.RotationIndex = rotation;
            var rotationMatches = true;
            foreach (var filter in rotationFilters)
            {
                if (filter.CanFulfill(value) is false)
                {
                    rotationMatches = false;
                    break;
                }
            }

            if (rotationMatches)
            {
                value.RotationIndex = originalRotation;
                return true;
            }
        }

        value.RotationIndex = originalRotation;
        return false;
    }

    public int Order => 0;
    public bool CanRejectPartial => Filters.Any(filter => filter.CanRejectPartial);
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
    public bool CanRejectPartial => false;
    public bool IsRotationAware => filter.IsRotationAware;
}
