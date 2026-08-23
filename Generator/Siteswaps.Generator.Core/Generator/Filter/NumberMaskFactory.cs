namespace Siteswaps.Generator.Core.Generator.Filter;

internal static class NumberMaskFactory
{
    public static (NumberMask PassValues, NumberMask SelfValues) Create(
        int minHeight,
        int maxHeight,
        int numberOfJugglers
    )
    {
        var heights = Enumerable.Range(minHeight, maxHeight - minHeight + 1);
        return (
            new NumberMask(heights.Where(x => x % numberOfJugglers != 0)),
            new NumberMask(heights.Where(x => x % numberOfJugglers == 0))
        );
    }
}
