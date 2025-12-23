using System.Collections.Immutable;
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

    public int Length => Items.Length;

    public int this[int i] => Items[i];

    public State State => StateGenerator.CalculateState(Items.EnumerateValues(1).ToArray());

    public Period Period => new(Items.Length);

    public Interface Interface
    {
        get
        {
            var result = new int[Items.Length];

            for (var i = 0; i < Items.Length; i++) result[(i + Items[i]) % Items.Length] = Items[i];

            return new Interface(result.ToImmutableList());
        }
    }

    public virtual bool Equals(Siteswap? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return ToString().Equals(other.ToString());
    }

    public static bool TryCreate(string value, [NotNullWhen(true)] out Siteswap? siteswap)
    {
        siteswap = null;
        return value != string.Empty && TryCreate(value.Select(ToInt), out siteswap);
    }

    private static int ToInt(char c)
    {
        var tryParse = int.TryParse(c.ToString(), out var value);
        if (tryParse) return value;

        return c - 87;
    }

    public static bool TryCreate(
        IEnumerable<int> items,
        [NotNullWhen(true)] out Siteswap? siteswap
    )
    {
        return TryCreate(items.ToCyclicArray(), out siteswap);
    }

    private static bool TryCreate(
        CyclicArray<int> items,
        [NotNullWhen(true)] out Siteswap? siteswap
    )
    {
        if (IsValid(items))
        {
            siteswap = new Siteswap(items);
            return true;
        }

        siteswap = null;
        return false;
    }

    private static bool IsValid(CyclicArray<int> items)
    {
        return items
            .Enumerate(1)
            .Select(x => x.value)
            .Select((x, i) => (x + i) % items.Length)
            .ToHashSet()
            .Count == items.Length;
    }

    private static CyclicArray<int> ToUniqueRepresentation(CyclicArray<int> input)
    {
        var biggest = input.EnumerateValues(1).ToList();

        foreach (
            var list in Enumerable
                .Range(0, input.Length)
                .Select(input.Rotate)
                .Select(x => x.EnumerateValues(1).ToList())
        )
            if (biggest.CompareSequences(list) < 0)
                biggest = list;

        return biggest.ToCyclicArray();
    }

    private bool IsGroundState()
    {
        return HasNoRethrow();
    }

    private bool HasNoRethrow()
    {
        return !Items.Enumerate(1).Any(x => x.position + x.value < NumberOfObjects());
    }

    public bool IsExcitedState()
    {
        return !IsGroundState();
    }

    public decimal NumberOfObjects()
    {
        return (decimal)Items.Enumerate(1).Average(x => x.value);
    }

    public override string ToString()
    {
        return string.Join("", Items.EnumerateValues(1).Select(Transform));
    }

    public static string Transform(int i)
    {
        return i switch
        {
            < 0 => throw new ArgumentException("Negative values are not allowed"),
            < 10 => $"{i}",
            _ => Convert.ToChar(i + 87).ToString()
        };
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public int Max()
    {
        return Items.EnumerateValues(1).Max();
    }

    public static CyclicArray<int> ToUniqueRepresentation(int[] input)
    {
        return ToUniqueRepresentation(input.ToCyclicArray());
    }

    public List<Orbit> GetOrbits()
    {
        return Orbit.CreateFrom(this).Where(x => x.HasBalls).ToList();
    }

    public List<Transition> PossibleTransitions(Siteswap to, int length, int? height = null)
    {
        return TransitionCalculator.CreateTransitions(this, to, length, height);
    }

    public Dictionary<State, List<Siteswap>> AllStates()
    {
        var siteswaps = Enumerable.Range(0, Period.Value).Select(Rotate).ToList();
        var dictionary = new Dictionary<State, List<Siteswap>>();
        foreach (var siteswap in siteswaps)
            if (dictionary.ContainsKey(siteswap.State))
                dictionary[siteswap.State].Add(siteswap);
            else
                dictionary[siteswap.State] = [siteswap];

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

    public (Siteswap nSiteswap, Throw nThrow) Throw()
    {
        var nSiteswap = new Siteswap(Items.Rotate(1));
        return (nSiteswap, new Throw(State, nSiteswap.State, Items[0]));
    }

    public IEnumerable<Siteswap> GetHighJacks()
    {
        var numberOfJugglers = 2;
        if (Period.GetLocalPeriod(numberOfJugglers).Value % 2 == 0) yield break;

        var highJackPassingValue = Period.GetLocalPeriod(numberOfJugglers).Value + 2;

        var highJackablePassPositions = Items
            .Enumerate(1)
            .Where((_, value) => value == highJackPassingValue)
            .Select((i, _) => i)
            .ToList();

        foreach (var highJackablePassPosition in highJackablePassPositions)
        foreach (var i in Enumerable.Range(0, Period.GetLocalPeriod(numberOfJugglers).Value))
            yield return Swap(
                highJackablePassPosition.position,
                highJackablePassPosition.position + i * numberOfJugglers + 1 // 1 should be 0...numberOfJugglers - 1 instead
            );

        yield return new Siteswap(5, 8, 8, 8, 2, 5);
    }

    public Siteswap Swap(int x, int y)
    {
        x = x % Period.Value;
        y = y % Period.Value;

        var items = Items.EnumerateValues(1).ToArray();
        (items[x], items[y]) = (items[y] + y - x, items[x] - (y - x));
        return new Siteswap(items);
    }
}

/// <summary>
///     An interface is the order of catches of a siteswap e.g. 53 will be 35
/// </summary>
/// <param name="Values"></param>
public record Interface(ImmutableList<int> Values)
{
    public override string ToString()
    {
        return string.Join("", Values.Select(Siteswap.Transform));
    }
}