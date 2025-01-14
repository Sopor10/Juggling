namespace Siteswaps.Generator.Generator.Filter.NumberFilter;

internal class AtLeastXXXTimesFilter : NumberFilter
{
    public AtLeastXXXTimesFilter(IEnumerable<int> number, int amount)
        : base(number, amount) { }

    private protected override bool CanFulfillNumberFilter(PartialSiteswap value) =>
        value.Items.Count(x => Number.Contains(x) || x == -1) >= Amount;
}
