namespace Siteswaps.Generator.Generator.Filter;

using Shared;

internal abstract class PatternFilter : ISiteswapFilter
{

    //each positin in a pattern can have multiple possible values
    private Pattern Pattern { get; }
    private List<Pattern> Patterns { get; }

    private int NumberOfJuggler { get; }

    public PatternFilter(Pattern pattern, int numberOfJuggler, SiteswapGeneratorInput input, bool isGlobalPattern)
    {

        this.NumberOfJuggler = numberOfJuggler;
        var passValues = Throw.PassValues(input.MinHeight, input.MaxHeight, this.NumberOfJuggler).ToHashSet();
            
        var selfValues = Throw.SelfValues(input.MinHeight, input.MaxHeight, this.NumberOfJuggler).ToHashSet();;

        var p = Enumerable.Repeat(new List<int>{-1}, input.Period).ToList();

        for (var i = 0; i < pattern.Value.Count; i++)
        {
            var pos = isGlobalPattern ? i :i * numberOfJuggler % input.Period;
            p[pos] = pattern.Value[i];
        }
        
        this.Pattern = new Pattern(p, selfValues, passValues);
        this.Patterns = new List<Pattern>();

        for (var i = 0; i < input.Period; i++)
        {
            var rotate = new Pattern(p.Rotate(i), selfValues, passValues);
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
