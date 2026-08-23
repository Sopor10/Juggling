namespace Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

public class PersonalizedNumberFilter : ISiteswapFilter
{
    private readonly int numberOfJugglers;
    private readonly int from;
    private readonly HashSet<int> numberValues;
    private readonly int amount;
    private readonly Type type;

    public PersonalizedNumberFilter(
        int numberOfJugglers,
        int minHeight,
        int maxHeight,
        IEnumerable<int> number,
        int amount,
        Type type,
        int from
    )
    {
        _ = minHeight;
        _ = maxHeight;
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numberOfJugglers);
        this.numberOfJugglers = numberOfJugglers;
        this.amount = amount;
        this.type = type;
        this.from = from;
        numberValues = number.ToHashSet();
    }

    public bool CanFulfill(PartialSiteswap value)
    {
        return type switch
        {
            Type.Exact => CanFulfillExact(value),
            Type.AtLeast => CanFulfillAtLeast(value),
            Type.AtMost => CanFulfillAtMost(value),
            _ => throw new InvalidOperationException($"Unsupported filter type: {type}"),
        };
    }

    private bool CanFulfillAtLeast(PartialSiteswap value)
    {
        var possible = 0;
        for (var index = from; index < value.Length; index += numberOfJugglers)
        {
            var throwHeight = value.Items[index];
            if ((throwHeight < 0 || numberValues.Contains(throwHeight)) && ++possible >= amount)
                return true;
        }

        return false;
    }

    private bool CanFulfillAtMost(PartialSiteswap value)
    {
        var count = 0;
        for (var index = from; index < value.Length; index += numberOfJugglers)
        {
            var throwHeight = value.Items[index];
            if (throwHeight >= 0 && numberValues.Contains(throwHeight) && ++count > amount)
                return false;
        }

        return count <= amount;
    }

    private bool CanFulfillExact(PartialSiteswap value)
    {
        var count = 0;
        var possible = 0;
        for (var index = from; index < value.Length; index += numberOfJugglers)
        {
            var throwHeight = value.Items[index];
            if (throwHeight < 0)
            {
                possible++;
            }
            else if (numberValues.Contains(throwHeight))
            {
                count++;
                possible++;
                if (count > amount)
                    return false;
            }
        }

        return possible >= amount && count <= amount;
    }

    public enum Type
    {
        Exact,
        AtLeast,
        AtMost,
    }

    public int Order => 0;
}
