namespace Siteswaps.Generator.Filter
{
    public abstract class NumberFilter : ISiteswapFilter
    {
        public NumberFilter(int number, int amount)
        {
            Number = number;
            Amount = amount;
        }
        public bool CanFulfill(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput)
        {
            return CanFulfillNumberFilter(value, siteswapGeneratorInput);
        }

        private protected abstract bool CanFulfillNumberFilter(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput);
        public int Number { get; }
        public int Amount { get; }
    }
}