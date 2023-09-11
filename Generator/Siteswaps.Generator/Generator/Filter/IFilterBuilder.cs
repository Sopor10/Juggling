namespace Siteswaps.Generator.Generator.Filter;

public interface IFilterBuilder
{
    IFilterBuilder MinimumOccurence(int number, int amount);
    IFilterBuilder MaximumOccurence(int number, int amount);
    IFilterBuilder ExactOccurence(int number, int amount);
    IFilterBuilder No();
    IFilterBuilder ExactNumberOfPasses(int numberOfPasses, int numberOfJugglers);
    IFilterBuilder And(ISiteswapFilter filter);
    IFilterBuilder Or(ISiteswapFilter filter);
    IFilterBuilder Pattern(IEnumerable<int> pattern, int numberOfJuggler);
    IFilterBuilder Interface(IEnumerable<int> pattern, int numberOfJuggler);
    IFilterBuilder FlexiblePattern(Pattern pattern, int numberOfJuggler, bool isGlobalPattern);
    IFilterBuilder WithDefault();
    ISiteswapFilter Build();
}
