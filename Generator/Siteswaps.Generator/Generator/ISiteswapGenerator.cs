namespace Siteswaps.Generator.Generator;

public interface ISiteswapGenerator
{
    IAsyncEnumerable<Siteswap> GenerateAsync();
}