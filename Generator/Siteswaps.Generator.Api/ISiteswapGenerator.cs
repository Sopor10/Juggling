namespace Siteswaps.Generator.Api;

public interface ISiteswapGenerator
{
    Task<IEnumerable<ISiteswap>> GenerateAsync();
}