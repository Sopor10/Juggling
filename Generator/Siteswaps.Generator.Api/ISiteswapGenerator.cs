namespace Siteswaps.Generator.Api;

public interface ISiteswapGenerator
{
    IAsyncEnumerable<ISiteswap> GenerateAsync();
}