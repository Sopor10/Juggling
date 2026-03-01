namespace Siteswap.Details.StateDiagram;

public class StateFactory
{
    public IEnumerable<State> Create(int numberOfObjects, int maxHeight)
    {
        return GenerateSelections(Enumerable.Range(0, maxHeight).ToList(), numberOfObjects)
            .Select(x => FromSelection(x, maxHeight));
    }

    private State FromSelection(List<int> list, int maxHeight)
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

    private static void SelectItems<T>(
        IReadOnlyCollection<T> items,
        IList<bool> inSelection,
        ICollection<List<T>> results,
        int n,
        int firstItem
    )
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
