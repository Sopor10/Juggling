using System.Diagnostics;

namespace Siteswaps.Generator.Generator.Filter;

internal class StateFilter(SiteswapGeneratorInput generatorInput, State state) : ISiteswapFilter
{
    private readonly int maxHeight = generatorInput.MaxHeight;

    public bool CanFulfill(PartialSiteswap value)
    {
        if (!value.IsFilled())
        {
            return true;
        }

        return state == State.CalculateState(value, maxHeight);
    }

    public int Order => 5;
}

/// <summary>
/// true indicates an object is scheduled to land on this timeslot.
/// false is therefore a free slot.
/// </summary>
/// <param name="Value"></param>
[DebuggerDisplay("{StateRepresentation()}")]
public record State(uint Value)
{
    public State(params int[] values)
        : this(values.Select(x => x != 0)) { }

    public State(IEnumerable<bool> values)
        : this(
            (uint)
                values
                    .Select((b, i) => (b, i))
                    .Where(x => x.b)
                    .Select(x => (int)Math.Pow(2, x.i))
                    .Sum()
        ) { }

    private string StateRepresentation()
    {
        return string.Concat(Convert.ToString(Value, 2).Reverse().ToArray());
    }

    public override string ToString() => StateRepresentation();

    private bool IsBitSet(uint b, int pos)
    {
        return (b & (1 << pos)) != 0;
    }

    private static State CalculateState(int[] siteswap, int? length = null)
    {
        var stable = false;

        var state = State.Empty();

        while (stable is false)
        {
            var previousState = state;
            state = siteswap.Aggregate(state, (current, value) => current.Advance().Throw(value));

            if (state == previousState)
                stable = true;
        }

        return state;
    }

    private State Advance()
    {
        var advance = this with { Value = Value >> 1 };
        return advance;
    }

    private State Throw(int i)
    {
        var state = this with { Value = Value | (uint)(1 << (i - 1)) };
        return state;
    }

    private static State Empty()
    {
        return new((uint)0);
    }

    public static State CalculateState(PartialSiteswap siteswap, int maxHeight)
    {
        return CalculateState(
            siteswap
                .Items.ToCyclicArray()
                .Rotate(siteswap.RotationIndex)
                .EnumerateValues(1)
                .ToArray(),
            maxHeight
        );
    }

    public static State GroundState(int numberOfBalls)
    {
        var mask = 0xffffffff;
        mask >>= 32 - numberOfBalls;
        mask <<= 0;
        return new State(mask);
    }
}
