namespace Siteswaps.Generator.Api.Filter;

public interface IFilterBuilderFactory
{
    public IFilterBuilder Create(SiteswapGeneratorInput input);
}