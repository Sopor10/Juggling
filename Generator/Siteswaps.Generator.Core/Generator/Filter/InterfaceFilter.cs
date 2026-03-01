using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Core.Generator.Filter;

public class InterfaceFilter : ISiteswapFilter
{
    private List<List<int>> Pattern { get; }
    private int NumberOfJugglers { get; }

    public InterfaceFilter(List<List<int>> pattern, int numberOfJugglers = 2)
    {
        Pattern = pattern;
        NumberOfJugglers = numberOfJugglers;
    }

    public bool CanFulfill(PartialSiteswap value)
    {
        if (Matches(value.Interface))
        {
            return true;
        }

        return false;
    }

    private bool Matches(CyclicArray<int> interfaceArray)
    {
        for (var i = 0; i < Pattern.Count; i++)
        {
            var interfaceValue = interfaceArray[i];
            if (interfaceValue == -1)
            {
                continue;
            }

            var patternAtPos = Pattern[i];
            if (patternAtPos.Contains(-1))
            {
                continue;
            }

            var matched = false;
            foreach (var p in patternAtPos)
            {
                if (p == -2) // Pass
                {
                    if (interfaceValue % NumberOfJugglers != 0)
                        matched = true;
                }
                else if (p == -3) // Self
                {
                    if (interfaceValue % NumberOfJugglers == 0)
                        matched = true;
                }
                else if (p == interfaceValue)
                {
                    matched = true;
                }
            }

            if (!matched)
                return false;
        }

        return true;
    }

    public int Order => 10;
}
