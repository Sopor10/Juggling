using Shared;

namespace Siteswaps.Generator.Generator.Filter.NumberFilter;

public class PersonalizedNumberFilter(
    int numberOfJugglers,
    int minHeight,
    int maxHeight,
    IEnumerable<int> number,
    int amount,
    PersonalizedNumberFilter.Type type,
    int from
) : ISiteswapFilter
{
    private HashSet<int> PassValues { get; } =
        Enumerable
            .Range(minHeight, maxHeight - minHeight + 1)
            .Where(x => x % numberOfJugglers != 0)
            .ToHashSet();

    private HashSet<int> SelfValues { get; } =
        Enumerable
            .Range(minHeight, maxHeight - minHeight + 1)
            .Where(x => x % numberOfJugglers != 0)
            .ToHashSet();

    public bool CanFulfill(PartialSiteswap value)
    {
        var throwsFromJuggler = value.Items.Where((_, i) => i % numberOfJugglers == from).ToList();

        var count = throwsFromJuggler.Count(number.Contains);
        var countAndEmpty = count + throwsFromJuggler.Count(x => x < 0);
        return type switch
        {
            Type.Exact => countAndEmpty >= amount && count <= amount,
            Type.AtLeast => countAndEmpty >= amount,
            Type.AtMost => count <= amount,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
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
