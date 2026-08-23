namespace Siteswaps.Generator.Core.Generator.Filter;

internal sealed class FlexiblePatternFilter : ISiteswapFilter
{
    private PatternRecord Pattern { get; }
    private List<PatternRecord> Patterns { get; }
    private int NumberOfJuggler { get; }
    private NumberMask PassValues { get; }
    private NumberMask SelfValues { get; }

    public FlexiblePatternFilter(
        List<List<int>> pattern,
        int numberOfJuggler,
        SiteswapGeneratorInput input,
        bool isGlobalPattern
    )
    {
        NumberOfJuggler = numberOfJuggler;
        (PassValues, SelfValues) = NumberMaskFactory.Create(
            input.MinHeight,
            input.MaxHeight,
            NumberOfJuggler
        );
        var p = Enumerable.Repeat(new List<int> { -1 }, input.Period).ToList();
        for (var i = 0; i < pattern.Count; i++)
        {
            var pos = isGlobalPattern ? i : i * numberOfJuggler % input.Period;
            p[pos] = pattern[i];
        }
        Pattern = new PatternRecord(p, SelfValues, PassValues);
        Patterns = new List<PatternRecord>();
        for (var i = 0; i < input.Period; i++)
            Patterns.Add(new PatternRecord(p.Rotate(i), SelfValues, PassValues));
    }

    public bool CanFulfill(PartialSiteswap value)
    {
        if (!value.IsFilled())
            return true;
        for (var i = 0; i < Patterns.Count; i++)
            if (Patterns[i].Matches(value.Items))
                return true;
        return false;
    }

    public int Order => 10;
    public bool CanRejectPartial => false;
}
