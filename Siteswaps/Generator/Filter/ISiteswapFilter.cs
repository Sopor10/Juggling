namespace Siteswaps.Generator.Filter
{
    public interface ISiteswapFilter
    {
        public bool CanFulfill(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput);


        public static ISiteswapFilter Standard() => new FilterList(new AverageToHighFilter());
    }
}