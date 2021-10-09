using Siteswaps.Generator.Filter;

namespace Siteswaps.Generator
{
    public record SiteswapGeneratorInput(
        int NumberOfObjects, 
        int Period, 
        int MinHeight, 
        int MaxHeight, 
        ISiteswapFilter Filter);
}