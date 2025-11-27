namespace Siteswaps.Generator.Core.Generator.Filter;

public interface IFilterBuilder
{
    IFilterBuilder MinimumOccurence(int number, int amount)
    {
        return MinimumOccurence([number], amount);
    }

    IFilterBuilder MaximumOccurence(int number, int amount)
    {
        return MaximumOccurence([number], amount);
    }

    IFilterBuilder ExactOccurence(int number, int amount)
    {
        return ExactOccurence([number], amount);
    }
    IFilterBuilder MinimumOccurence(IEnumerable<int> number, int amount);
    IFilterBuilder MaximumOccurence(IEnumerable<int> number, int amount);
    IFilterBuilder ExactOccurence(IEnumerable<int> number, int amount);

    IFilterBuilder No();
    IFilterBuilder ExactNumberOfPasses(int numberOfPasses, int numberOfJugglers);
    IFilterBuilder And(params IEnumerable<ISiteswapFilter> filter);
    IFilterBuilder Or(ISiteswapFilter filter);
    public IFilterBuilder Pattern(IEnumerable<int> pattern, int numberOfJuggler);

    public IFilterBuilder WithState(State state);

    IFilterBuilder FlexiblePattern(
        List<List<int>> pattern,
        int numberOfJuggler,
        bool isGlobalPattern
    );
    IFilterBuilder WithDefault();
    ISiteswapFilter Build();
}
