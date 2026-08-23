namespace Siteswaps.Generator.Core.Generator.Filter;

internal readonly struct NumberMask
{
    private readonly ulong _mask;
    private readonly HashSet<int>? _overflow;

    public NumberMask(IEnumerable<int> values)
    {
        ulong mask = 0;
        HashSet<int>? overflow = null;

        foreach (var value in values)
        {
            if ((uint)value < 64)
            {
                mask |= 1UL << value;
            }
            else
            {
                (overflow ??= []).Add(value);
            }
        }

        _mask = mask;
        _overflow = overflow;
    }

    public bool Contains(int value) =>
        (uint)value < 64 ? (_mask & (1UL << value)) != 0 : _overflow?.Contains(value) == true;
}
