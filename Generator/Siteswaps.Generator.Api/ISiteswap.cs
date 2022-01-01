using System.Collections.Immutable;

namespace Siteswaps.Generator.Api;

public interface ISiteswap
{
    public ImmutableList<int> Items { get; }
}