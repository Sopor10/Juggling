using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Siteswaps.Generator.Api;

public interface ISiteswapGenerator
{
    Task<IEnumerable<ISiteswap>> GenerateAsync(SiteswapGeneratorInput input);
}

public interface ISiteswap
{
    public ImmutableList<int> Items { get; }
}