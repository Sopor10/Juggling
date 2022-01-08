namespace Siteswaps.Generator.Api.Filter;

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
    IFilterBuilder WithDefault();
    ISiteswapFilter Build();
}

public interface IFilterBuilderFactory
{
    public IFilterBuilder Create(SiteswapGeneratorInput input);
}

