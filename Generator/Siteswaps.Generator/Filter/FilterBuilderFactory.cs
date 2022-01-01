using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Filter;

public class FilterBuilderFactory :IFilterBuilderFactory
{
    public IFilterBuilder Create(SiteswapGeneratorInput input)
    {
        return new FilterBuilder().WithInput(input).Add(new FilterFactory(input).Standard());
    }
}