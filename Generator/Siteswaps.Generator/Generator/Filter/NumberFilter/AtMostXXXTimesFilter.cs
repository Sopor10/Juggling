namespace Siteswaps.Generator.Generator.Filter.NumberFilter;

internal class AtMostXXXTimesFilter : NumberFilter
{
    public AtMostXXXTimesFilter(int number, int amount): base(number, amount)
    {
    }

    private protected override bool CanFulfillNumberFilter(PartialSiteswap value) 
        => value.Items.Count(x => x == Number) <= Amount;
}