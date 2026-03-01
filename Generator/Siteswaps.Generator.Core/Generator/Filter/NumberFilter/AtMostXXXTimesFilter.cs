namespace Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

internal class AtMostXXXTimesFilter(IEnumerable<int> number, int amount)
    : NumberFilter(number, amount)
{
    private protected override bool CanFulfillNumberFilter(PartialSiteswap value) =>
        value.Items.Count(x => Number.Contains(x)) <= Amount;
}
