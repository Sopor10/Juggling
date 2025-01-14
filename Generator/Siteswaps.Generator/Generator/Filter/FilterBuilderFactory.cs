namespace Siteswaps.Generator.Generator.Filter;

public class FilterBuilderFactory
{
    public IFilterBuilder Create(SiteswapGeneratorInput input)
    {
        return new FilterBuilder(input);
    }
}
