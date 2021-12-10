using System.Collections.Generic;
using System.Threading.Tasks;

namespace Siteswaps.Generator;

public interface ISiteswapGenerator
{
    Task<IEnumerable<Siteswap>> GenerateAsync(SiteswapGeneratorInput input);
}