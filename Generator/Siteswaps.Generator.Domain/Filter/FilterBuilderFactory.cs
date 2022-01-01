using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain.Filter;

public class FilterBuilderFactory :IFilterBuilderFactory
{
    public IFilterBuilder Create(SiteswapGeneratorInput input)
    {
        return new FilterBuilder().WithInput(input).And(new FilterFactory(input).Standard());
    }
}