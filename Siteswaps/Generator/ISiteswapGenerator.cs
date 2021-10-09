using System.Collections.Generic;

namespace Siteswaps.Generator
{
    public interface ISiteswapGenerator
    {
        IEnumerable<Siteswap> Generate(SiteswapGeneratorInput input);
    }
}