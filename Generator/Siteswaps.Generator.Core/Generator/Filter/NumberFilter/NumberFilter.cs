using System.Runtime.CompilerServices;

namespace Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

public abstract class NumberFilter : ISiteswapFilter
{
    protected NumberFilter(IEnumerable<int> number, int amount)
    {
        Number = number.ToHashSet();
        Amount = amount;
        HasSingleNumber = Number.Count == 1;
        SingleNumber = HasSingleNumber ? Number.First() : 0;
    }

    protected bool HasSingleNumber { get; }
    protected int SingleNumber { get; }

    public bool CanFulfill(PartialSiteswap value)
    {
        return CanFulfillNumberFilter(value);
    }

    private protected abstract bool CanFulfillNumberFilter(PartialSiteswap value);
    protected HashSet<int> Number { get; }
    protected int Amount { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool ContainsNumber(int value)
    {
        return HasSingleNumber ? value == SingleNumber : Number.Contains(value);
    }

    public int Order => 0;
    public bool IsRotationAware => false;
}
