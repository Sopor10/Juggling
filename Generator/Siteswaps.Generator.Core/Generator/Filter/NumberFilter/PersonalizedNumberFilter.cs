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
        this.numberOfJugglers = numberOfJugglers;
        this.amount = amount;
        this.type = type;
        this.from = from;
        numberValues = number.ToHashSet();
    }

    public bool CanFulfill(PartialSiteswap value)
    {
        var count = 0;
        var empty = 0;
        for (var index = from; index < value.Length; index += numberOfJugglers)
        {
            var throwHeight = value.Items[index];
            if (throwHeight < 0)
                empty++;
            else if (numberValues.Contains(throwHeight))
                count++;
        }

        var countAndEmpty = count + empty;
        return type switch
        {
            Type.Exact => countAndEmpty >= amount && count <= amount,
            Type.AtLeast => countAndEmpty >= amount,
            Type.AtMost => count <= amount,
            _ => throw new InvalidOperationException($"Unsupported filter type: {type}"),
        };
    }

    public enum Type
    {
        Exact,
        AtLeast,
        AtMost,
    }

    public int Order => 0;
}
