using System.Collections;

namespace Shared;

using System.Diagnostics;

[DebuggerDisplay("{DebugDisplay}")]
public record CyclicArray<T> : IEnumerable<T>
{

    private string DebugDisplay => string.Join(" ", this.EnumerateValues(1).Select(x => x.ToString()));
    public CyclicArray(IEnumerable<T> items, int rotationIndex = 0)
    {
        RotationIndex = rotationIndex;
        Items = items.ToArray();
    }

    private int RotationIndex { get; set; }
    private T[] Items { get; }
    public int Length => Items.Length;

    public T this[int i]
    {
        get => Items[(i + Items.Length + RotationIndex) % Items.Length];
        set => Items[(i + Items.Length + RotationIndex) % Items.Length] = value;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new CyclicArrayEnumerator<T>(this);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerable<(int position, T value)> Enumerate(int i)
    {
        for (var j = 0; j < i; j++)
        {
            for (var k = 0; k < Items.Length; k++)
            {
                yield return (j * Items.Length + k, this[k]);

            }

        }
    }

    public IEnumerable<T> EnumerateValues(int i)
    {
        return Enumerate(i).Select(x => x.value);
    }

    public CyclicArray<T> Rotate(int i)
    {
        RotationIndex += i;
        return this;
    }
}

public static class CyclicArrayExtensions
{
    public static CyclicArray<T> ToCyclicArray<T>(this IEnumerable<T> source)
    {
        return new CyclicArray<T>(source);
    }
}
