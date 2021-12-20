using System.Collections.Generic;

namespace Siteswaps.Generator.Filter;

public interface IFilterFactory
{
    ISiteswapFilter MinimumOccurenceFilter(int number, int amount);
    ISiteswapFilter MaximumOccurenceFilter(int number, int amount);
    ISiteswapFilter ExactOccurenceFilter(int number, int amount);
    ISiteswapFilter NoFilter();
    ISiteswapFilter ExactNumberOfPassesFilter(int numberOfPasses, int numberOfJugglers);
    ISiteswapFilter Combine(IEnumerable<ISiteswapFilter> filter);
}