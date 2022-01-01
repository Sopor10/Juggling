using System.Collections.Immutable;

namespace Siteswaps.Generator.Api;

public interface ISiteswapGenerator
{
    Task<IEnumerable<ISiteswap>> GenerateAsync();
}

public interface ISiteswap
{
    public ImmutableList<int> Items { get; }
}