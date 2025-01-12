namespace Siteswaps.Generator.Generator.Filter.NumberFilter;

internal class AtMostXXXTimesFilter : NumberFilter
{
    public AtMostXXXTimesFilter(IEnumerable<int> number, int amount): base(number, amount)
    {
    }

    private protected override bool CanFulfillNumberFilter(PartialSiteswap value) 
        => value.Items.Count(x => Number.Contains(x)) <= Amount;
}
