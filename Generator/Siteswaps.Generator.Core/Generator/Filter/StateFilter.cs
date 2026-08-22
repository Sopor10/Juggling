using System.Diagnostics;

namespace Siteswaps.Generator.Core.Generator.Filter;

internal sealed class StateFilter(SiteswapGeneratorInput generatorInput, State state)
    : ISiteswapFilter
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
    public bool IsRotationAware => true;
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

    public bool IsOccupiedAt(int position) => IsBitSet(Value, position);

    private static State CalculateState(PartialSiteswap siteswap)
    {
        uint state = 0;
        while (true)
        {
            var previousState = state;
            for (var index = 0; index < siteswap.Items.Length; index++)
            {
                state >>= 1;
                state |= (uint)(1 << (siteswap.Items[index] - 1));
            }

            if (state == previousState)
            {
                return new State(state);
            }
        }
    }

    public static State CalculateState(PartialSiteswap siteswap, int maxHeight) =>
        CalculateState(siteswap);

    public static State GroundState(int numberOfBalls)
    {
        var mask = 0xffffffff;
        mask >>= 32 - numberOfBalls;
        mask <<= 0;
        return new State(mask);
    }
}
