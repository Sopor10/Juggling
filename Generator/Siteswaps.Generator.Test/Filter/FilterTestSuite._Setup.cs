using Siteswaps.Generator.Generator;
using Siteswaps.Generator.Generator.Filter;

namespace Siteswaps.Generator.Test.Filter;

public partial class FilterTestSuite
{
    protected IFilterBuilder FilterBuilder =>
        new FilterBuilder(
            Input
                ?? throw new InvalidOperationException(
                    "Please set a Input via ConfigureSiteswapGeneratorInput"
                )
        );

    private SiteswapGeneratorInput? Input { get; set; } = new(3, 3, 0, 10);
}
