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
        return value != string.Empty && TryCreate(value.Select(ToInt), out siteswap);
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

    public int Length => Items.Length;

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
            < 0 => throw new ArgumentException("Negative values are not allowed"),
            < 10 => $"{i}",
            _ => Convert.ToChar(i + 87).ToString(),
        };
    }

    public virtual bool Equals(Siteswap? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return ToString().Equals(other.ToString());
    }

    public override int GetHashCode() => ToString().GetHashCode();

    public int Max() => Items.EnumerateValues(1).Max();

    public int this[int i] => Items[i];

    public static CyclicArray<int> ToUniqueRepresentation(int[] input) =>
        ToUniqueRepresentation(input.ToCyclicArray());

    public List<Orbit> GetOrbits() => Orbit.CreateFrom(this).Where(x => x.HasBalls).ToList();

    public List<Transition> PossibleTransitions(Siteswap to, int length, int? height = null) =>
        TransitionCalculator.CreateTransitions(this, to, length, height);

    public State State => StateGenerator.CalculateState(Items.EnumerateValues(1).ToArray());

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

    private Siteswap Rotate(int i) => new(Items.Rotate(i));

    public LocalSiteswap GetLocalSiteswap(int juggler, int numberOfJugglers) =>
        new(this, juggler, numberOfJugglers);

    public Period Period => new(Items.Length);

    public (Siteswap nSiteswap, Throw nThrow) Throw()
    {
        var nSiteswap = new Siteswap(Items.Rotate(1));
        return (nSiteswap, new Throw(State, nSiteswap.State, Items[0]));
    }

    public IEnumerable<Siteswap> GetHighJacks()
    {
        var numberOfJugglers = 2;
        if (Period.GetLocalPeriod(numberOfJugglers).Value % 2 == 0)
        {
            yield break;
        }

        var highJackPassingValue = this.Period.GetLocalPeriod(numberOfJugglers).Value + 2;

        var highJackablePassPositions = Items
            .Enumerate(1)
            .Where((_, value) => value == highJackPassingValue)
            .Select((i, _) => i)
            .ToList();

        foreach (var highJackablePassPosition in highJackablePassPositions)
        {
            foreach (var i in Enumerable.Range(0, Period.GetLocalPeriod(numberOfJugglers).Value))
            {
                yield return this.Swap(
                    highJackablePassPosition.position,
                    highJackablePassPosition.position + i * numberOfJugglers + 1 // 1 should be 0...numberOfJugglers - 1 instead
                );
            }
        }
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
