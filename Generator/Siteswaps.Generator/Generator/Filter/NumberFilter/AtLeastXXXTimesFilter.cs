namespace Siteswaps.Generator.Generator.Filter.NumberFilter;

internal class AtLeastXXXTimesFilter : NumberFilter
{
    public AtLeastXXXTimesFilter(int number, int amount) : base(number, amount)
    {
    }

    private protected override bool CanFulfillNumberFilter(PartialSiteswap value) 
        => value.Items.Count(x => x == Number || x == -1) >= Amount;
}