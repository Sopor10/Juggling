using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Siteswap.Details.StateDiagram;

namespace Siteswap.Details;

public record Siteswap(CyclicArray<int> Items)
{
    public Siteswap(params int[] items)
        : this(new CyclicArray<int>(items))
    {
        IsValid(new CyclicArray<int>(items));
    }

    public static bool TryCreate(string value, [NotNullWhen(true)] out Siteswap? siteswap)
    {
        siteswap = null;
        if (value == string.Empty)
        {
            return false;
        }

        return TryCreate(value.Select(ToInt), out siteswap);
    }

    private static int ToInt(char c)
    {
        var tryParse = int.TryParse(c.ToString(), out var value);
        if (tryParse)
        {
            return value;
        }

        return c - 87;
    }

    public static bool TryCreate(
        IEnumerable<int> items,
        [NotNullWhen(true)] out Siteswap? siteswap
    ) => TryCreate(items.ToCyclicArray(), out siteswap);

    private static bool TryCreate(
        CyclicArray<int> items,
        [NotNullWhen(true)] out Siteswap? siteswap
    )
    {
        if (IsValid(items))
        {
            siteswap = new(items);
            return true;
        }

        siteswap = null;
        return false;
    }

    private static bool IsValid(CyclicArray<int> items) =>
        items
            .Enumerate(1)
            .Select(x => x.value)
            .Select((x, i) => (x + i) % items.Length)
            .ToHashSet()
            .Count == items.Length;

    private static CyclicArray<int> ToUniqueRepresentation(CyclicArray<int> input)
    {
        var biggest = input.EnumerateValues(1).ToList();

        foreach (
            var list in Enumerable
                .Range(0, input.Length)
                .Select(input.Rotate)
                .Select(x => x.EnumerateValues(1).ToList())
        )
        {
            if (biggest.CompareSequences(list) < 0)
            {
                biggest = list;
            }
        }

        return biggest.ToCyclicArray();
    }

    private bool IsGroundState() => HasNoRethrow();

    private bool HasNoRethrow() =>
        !Items.Enumerate(1).Any(x => x.position + x.value < NumberOfObjects());

    public bool IsExcitedState() => !IsGroundState();

    public decimal NumberOfObjects() => (decimal)Items.Enumerate(1).Average(x => x.value);

    public override string ToString()
    {
        return string.Join("", Items.EnumerateValues(1).Select(Transform));
    }

    public static string Transform(int i)
    {
        return i switch
        {
            < 10 => $"{i}",
            _ => Convert.ToChar(i + 87).ToString(),
        };
    }

    public virtual bool Equals(Siteswap? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return ToString().Equals(other.ToString());
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public int Max() => Items.EnumerateValues(1).Max();

    public int this[int i] => Items[i];

    public static CyclicArray<int> ToUniqueRepresentation(int[] input) =>
        ToUniqueRepresentation(input.ToCyclicArray());

    public List<Orbit> GetOrbits() => GetOrbitsInternal().ToList();

    private IEnumerable<Orbit> GetOrbitsInternal()
    {
        var visited = new bool[Items.Length];

        for (int i = 0; i < Items.Length; i++)
        {
            if (visited[i])
            {
                continue;
            }

            var orbitIndices = new List<int>();
            var current = i;

            // Sammle alle Indizes im aktuellen Orbit
            do
            {
                visited[current] = true;
                orbitIndices.Add(current);
                current = (current + Items[current]) % Items.Length;
            } while (current != i);

            // Erstelle eine Orbit-Liste mit 0 als Standardwert
            var orbitValues = new int[Items.Length];
            for (int j = 0; j < orbitValues.Length; j++)
            {
                orbitValues[j] = 0;
            }

            // Setze die Werte an den Orbit-Positionen
            foreach (var index in orbitIndices)
            {
                orbitValues[index] = Items[index];
            }

            yield return new Orbit(orbitValues.ToList());
        }
    }

    public Throw[] PossibleThrows(int? height = null)
    {
        height ??= Items.EnumerateValues(1).Max();

        var transitions = State.Transitions(height.Value);
        return transitions.Select(x => new Throw(x.N1, x.N2, x.Data)).ToArray();
    }

    public List<Transition> PossibleTransitions(Siteswap to, int length, int? height = null) =>
        CreateTransitions(to, length, height);

    private State State => StateGenerator.CalculateState(Items.EnumerateValues(1).ToArray());

    private List<Transition> CreateTransitions(Siteswap to, int length, int? maxHeight = null)
    {
        maxHeight ??= new[]
        {
            Items.EnumerateValues(1).Max(),
            to.Items.EnumerateValues(1).Max(),
        }.Max();
        var result = new List<ImmutableList<Throw>>();

        var fromState = State;
        var toState = to.State;

        result.AddRange(
            Recurse(fromState, toState, ImmutableList<Throw>.Empty, length, maxHeight.Value)
        );

        return result.Select(x => new Transition(this, to, x.ToArray())).ToList();
    }

    private static IEnumerable<ImmutableList<Throw>> Recurse(
        State fromState,
        State toState,
        ImmutableList<Throw> transitionSoFar,
        int length,
        int maxHeight
    )
    {
        foreach (var transition in fromState.Transitions(maxHeight))
        {
            if (toState == transition.N2)
            {
                yield return transitionSoFar.Add(
                    new Throw(transition.N1, transition.N2, transition.Data)
                );
            }

            if (length == 1)
                continue;

            foreach (
                var immutableList in Recurse(
                    transition.N2,
                    toState,
                    transitionSoFar.Add(new Throw(transition.N1, transition.N2, transition.Data)),
                    length - 1,
                    maxHeight
                )
            )
            {
                yield return immutableList;
            }
        }
    }

    public Dictionary<State, List<Siteswap>> AllStates()
    {
        var siteswaps = Enumerable.Range(0, Period.Value).Select(Rotate).ToList();
        var dictionary = new Dictionary<State, List<Siteswap>>();
        foreach (var siteswap in siteswaps)
        {
            if (dictionary.ContainsKey(siteswap.State))
            {
                dictionary[siteswap.State].Add(siteswap);
            }
            else
            {
                dictionary[siteswap.State] = [siteswap];
            }
        }
        return dictionary;
    }

    private Siteswap Rotate(int i)
    {
        return new Siteswap(Items.Rotate(i));
    }

    public LocalSiteswap GetLocalSiteswap(int juggler, int numberOfJugglers)
    {
        return new LocalSiteswap(this, juggler, numberOfJugglers);
    }

    public Period Period => new(Items.Length);

    public (Siteswap nSiteswap, Throw nThrow) Throw()
    {
        var nSiteswap = new Siteswap(Items.Rotate(1));
        return (nSiteswap, new Throw(State, nSiteswap.State, Items[0]));
    }
}

public record Period(int Value)
{
    public LocalPeriod GetLocalPeriod(int numberOfJugglers) =>
        Value % numberOfJugglers == 0 ? new(Value / numberOfJugglers) : new(Value);
}

public record LocalPeriod(int Value);

public class Orbit(List<int> items)
{
    public List<int> Items => items;
}

[DebuggerDisplay("{PrettyPrint()}")]
public record Throw(State StartingState, State EndingState, int Value)
{
    public string PrettyPrint()
    {
        return $"{StartingState} -> {EndingState} : {Value}";
    }
}

public record LocalSiteswap(Siteswap Siteswap, int Juggler, int NumberOfJugglers)
{
    public string GlobalNotation => ToString();
    public string LocalNotation =>
        string.Join(
            " ",
            GetLocalSiteswapReal()
                .Select(x => x * 1.0 / NumberOfJugglers)
                .Select(x => x.ToString("0.##"))
        );

    private List<int> GetLocalSiteswapReal()
    {
        var result = new List<int>();

        var siteswap = Siteswap.Items.ToCyclicArray();
        for (var i = 0; i < Siteswap.Period.GetLocalPeriod(NumberOfJugglers).Value; i++)
        {
            result.Add(siteswap[Juggler + i * NumberOfJugglers]);
        }

        return result;
    }

    public override string ToString()
    {
        return ToString(GetLocalSiteswapReal());
    }

    private string ToString(IEnumerable<int> items)
    {
        return string.Join("", items.Select(Transform));
    }

    private string Transform(int i)
    {
        return i switch
        {
            < 10 => $"{i}",
            _ => Convert.ToChar(i + 87).ToString(),
        };
    }

    public double Average()
    {
        return GetLocalSiteswapReal().Average() * 1.0 / NumberOfJugglers;
    }

    public bool IsValidAsGlobalSiteswap()
    {
        var items = GetLocalSiteswapReal();

        return items.Select((x, i) => (x + i) % items.Count).ToHashSet().Count == items.Count;
    }
}
