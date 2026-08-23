namespace Siteswaps.Generator.Core.Generator.Filter;

public class RotationAwareFlexiblePatternFilter : ISiteswapFilter
{
    private List<List<int>> Pattern { get; }
    private int NumberOfJugglers { get; }
    private Siteswaps.Generator.Core.Generator.SiteswapGeneratorInput Input { get; }
    private NumberMask PassValues { get; }
    private NumberMask SelfValues { get; }
    private int Juggler { get; }
    private readonly PatternRecord _pattern;

    public RotationAwareFlexiblePatternFilter(
        List<List<int>> pattern,
        int numberOfJugglers,
        Siteswaps.Generator.Core.Generator.SiteswapGeneratorInput input,
        int juggler
    )
    {
        Pattern = pattern;
        NumberOfJugglers = numberOfJugglers;
        Input = input;
        Juggler = juggler;
        PassValues = new NumberMask(
            Enumerable
                .Range(input.MinHeight, input.MaxHeight - input.MinHeight + 1)
                .Where(x => x % NumberOfJugglers != 0)
        );
        SelfValues = new NumberMask(
            Enumerable
                .Range(input.MinHeight, input.MaxHeight - input.MinHeight + 1)
                .Where(x => x % NumberOfJugglers == 0)
        );
        var p = Enumerable.Repeat(new List<int> { -1 }, input.Period).ToList();
        for (var i = 0; i < Pattern.Count; i++)
        {
            var pos = (Juggler + i * NumberOfJugglers) % input.Period;
            p[pos] = Pattern[i];
        }
        _pattern = new PatternRecord(p, SelfValues, PassValues);
    }

    public bool CanFulfill(PartialSiteswap value) =>
        !value.IsFilled() || _pattern.Matches(value.Items);

    public int Order => 10;
    public bool CanRejectPartial => false;
    public bool IsRotationAware => true;
}
