using System.Diagnostics.CodeAnalysis;

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

    [SuppressMessage("Naming", "CA1716", Justification = "Not is part of the public filter DSL.")]
    IFilterBuilder Not(ISiteswapFilter filter);
    IFilterBuilder ExactNumberOfPasses(int numberOfPasses, int numberOfJugglers);

    [SuppressMessage("Naming", "CA1716", Justification = "And is part of the public filter DSL.")]
    IFilterBuilder And(params IEnumerable<ISiteswapFilter> filter);

    [SuppressMessage("Naming", "CA1716", Justification = "Or is part of the public filter DSL.")]
    IFilterBuilder Or(ISiteswapFilter filter);
    public IFilterBuilder Pattern(IEnumerable<int> pattern, int numberOfJuggler);

    public IFilterBuilder WithState(State state);

    IFilterBuilder WithState(StatePattern pattern);

    IFilterBuilder FlexiblePattern(
        List<List<int>> pattern,
        int numberOfJuggler,
        bool isGlobalPattern
    );
    IFilterBuilder WithDefault();
    ISiteswapFilter Build();
}
