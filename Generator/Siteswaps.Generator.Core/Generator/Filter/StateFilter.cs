using System.Diagnostics;

namespace Siteswaps.Generator.Core.Generator.Filter;

internal sealed class StateFilter(SiteswapGeneratorInput generatorInput, State state) : ISiteswapFilter
{
    private readonly int maxHeight = generatorInput.MaxHeight;
    public bool CanFulfill(PartialSiteswap value) => !value.IsFilled() || state == State.CalculateState(value, maxHeight);
    public int Order => 5;
    public bool IsRotationAware => true;
}

[DebuggerDisplay("{StateRepresentation()}")]
public record State(uint Value)
{
    public State(params int[] values) : this(values.Select(x => x != 0)) { }
    public State(IEnumerable<bool> values) : this((uint)values.Select((b, i) => (b, i)).Where(x => x.b).Select(x => (int)Math.Pow(2, x.i)).Sum()) { }
    private string StateRepresentation() => string.Concat(Convert.ToString(Value, 2).Reverse().ToArray());
    public override string ToString() => StateRepresentation();
    private static bool IsBitSet(uint b, int pos) => (b & (1 << pos)) != 0;
    private static State CalculateState(int[] siteswap, int? length = null)
    {
        var stable = false;
        var state = State.Empty();
        while (stable is false)
        {
            var previousState = state;
            state = siteswap.Aggregate(state, (current, value) => current.Advance().Throw(value));
            if (state == previousState) stable = true;
        }
        return state;
    }
    private State Advance() => this with { Value = Value >> 1 };
    private State Throw(int i) => this with { Value = Value | (uint)(1 << (i - 1)) };
    private static State Empty() => new((uint)0);
    public static State CalculateState(PartialSiteswap siteswap, int maxHeight)
    {
        var length = siteswap.Items.Length;
        var items = new int[length];
        for (int i = 0; i < length; i++) items[i] = siteswap.Items[i];
        return CalculateState(items, maxHeight);
    }
    public static State GroundState(int numberOfBalls)
    {
        var mask = 0xffffffff;
        mask >>= 32 - numberOfBalls;
        mask <<= 0;
        return new State(mask);
    }
}
