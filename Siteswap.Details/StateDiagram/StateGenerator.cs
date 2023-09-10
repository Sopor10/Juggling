using System.Linq;

namespace Siteswap.Details.StateDiagram;

using System.Collections.Generic;

public static class StateGenerator
{
    public static State CalculateState(int[] siteswap)
    {
        var stable = false;

        var state = State.Empty();

        while (stable is false)
        {
            var previousState = state;
            foreach (var i in siteswap)
            {
                state = state.Advance();
                if (i>0)
                {
                    state = state.Throw(i);
                }
            }

            if (state == previousState) stable = true;
        }

        return state;
    }

    public static State CalculateState(Siteswap siteswap)
    {
        return CalculateState(siteswap.Items.EnumerateValues(1).ToArray());
    }

    public static IEnumerable<State> Create(int numberOfObjects, int maxHeight)
    {

        return Enumerable
            .Select<List<int>, State>(GenerateSelections(Enumerable.Range(0, maxHeight).ToList(), numberOfObjects), x => FromSelection(x, maxHeight));
    }

    private static State FromSelection(List<int> list, int maxHeight)
    {
        var state = Enumerable.Repeat(false, maxHeight + 1).ToArray();
        foreach (var i in list)
        {
            state[i] = true;
        }

        return new State(state);
    }

    private static List<List<T>> GenerateSelections<T>(IReadOnlyCollection<T> items, int n)
    {
        var inSelection = new bool[items.Count];

        List<List<T>> results = new();

        SelectItems(items, inSelection, results, n, 0);

        return results;
    }

    private static void SelectItems<T>(IReadOnlyCollection<T> items, IList<bool> inSelection,
        ICollection<List<T>> results, int n, int firstItem)
    {
        if (n == 0)
        {
            List<T> selection = items.Where((_, i) => inSelection[i]).ToList();
            results.Add(selection);
        }
        else
        {
            for (var i = firstItem; i < items.Count; i++)
            {
                inSelection[i] = true;

                SelectItems(items, inSelection, results, n - 1, i + 1);

                inSelection[i] = false;
            }
        }
    }
}
