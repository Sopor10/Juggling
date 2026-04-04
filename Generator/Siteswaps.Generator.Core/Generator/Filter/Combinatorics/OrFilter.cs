namespace Siteswaps.Generator.Core.Generator.Filter.Combinatorics;

public class OrFilter : ISiteswapFilter
{
    private List<ISiteswapFilter> Filters { get; }
    private readonly bool _isRotationAware;

    public OrFilter(params IEnumerable<ISiteswapFilter> filters)
    {
        Filters = filters.ToList();
        _isRotationAware = Filters.Any(f => f.IsRotationAware);
    }

    public bool CanFulfill(PartialSiteswap value) => Filters.Any(x => x.CanFulfill(value));

    public int Order => 0;
    public bool IsRotationAware => _isRotationAware;
}
