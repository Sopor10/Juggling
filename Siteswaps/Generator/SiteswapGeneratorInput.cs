using System.Collections.Immutable;
using System.Linq;
using Siteswaps.Generator.Filter;

namespace Siteswaps.Generator
{
    public record SiteswapGeneratorInput(
        int NumberOfObjects,
        int Period,
        int MinHeight,
        int MaxHeight,
        ISiteswapFilter Filter)
    {
        public static SiteswapGeneratorInput Standard => new SiteswapGeneratorInput(7, 5, 2, 9, new NoFilter());
        
    }

    public record SiteswapGeneratorInputBuilder
    {
        public SiteswapGeneratorInputBuilder()
        {
            NumberOfObjects = 7;
            Period = 5;
            MinHeight = 2;
            MaxHeight = 9;
            Filter = ImmutableList<ISiteswapFilter>.Empty;
        }

        public int NumberOfObjects { get; init; }
        public int Period { get; init; }
        public int MinHeight { get; init; }
        public int MaxHeight { get; init; }
        public ImmutableList<ISiteswapFilter> Filter { get; init; }

        public SiteswapGeneratorInputBuilder AddFilter(ISiteswapFilter filter) => this with { Filter = Filter.Add(filter) };
        

        public SiteswapGeneratorInput Build()
        {
            return new(NumberOfObjects, Period, MinHeight, MaxHeight, new FilterList(Filter.ToArray()));
        }
    }
}