namespace Siteswaps.Generator.Generator.Filter;

using Shared;

internal abstract class PatternFilter : ISiteswapFilter
{

    //each positin in a pattern can have multiple possible values
    private Pattern Pattern { get; }
    private List<Pattern> Patterns { get; }

    private int NumberOfJuggler { get; }

    private HashSet<int> PassValues { get; }

    private HashSet<int> SelfValues { get; }

    public PatternFilter(List<List<int>> pattern, int numberOfJuggler, SiteswapGeneratorInput input, bool isGlobalPattern)
    {

        this.NumberOfJuggler = numberOfJuggler;
        this.PassValues = Enumerable.Range(input.MinHeight, input.MaxHeight - input.MinHeight + 1)
            .Where(x => x % this.NumberOfJuggler != 0).ToHashSet();
        this.SelfValues = Enumerable.Range(input.MinHeight, input.MaxHeight - input.MinHeight + 1)
            .Where(x => x % this.NumberOfJuggler == 0).ToHashSet();

        var p = Enumerable.Repeat(new List<int>{-1}, input.Period).ToList();

        for (var i = 0; i < pattern.Count; i++)
        {
            var pos = isGlobalPattern ? i :i * numberOfJuggler % input.Period;
            p[pos] = pattern[i];
        }
        
        this.Pattern = new Pattern(p, this.SelfValues, this.PassValues);
        this.Patterns = new List<Pattern>();

        for (var i = 0; i < input.Period; i++)
        {
            var rotate = new Pattern(p.Rotate(i), this.SelfValues, this.PassValues);
            this.Patterns.Add(rotate);
        }
    }

    public bool CanFulfill(PartialSiteswap value)
    {
        if (!value.IsFilled())
        {
            return true;
        }

        return this.Patterns.Any(pattern => pattern.Matches(this.Matches(value)));
    }

    protected abstract CyclicArray<sbyte> Matches(PartialSiteswap value);
}