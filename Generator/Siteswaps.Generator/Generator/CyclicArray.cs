using System.Collections;

namespace Siteswaps.Generator.Generator;

public record CyclicArray<T> : IEnumerable<T>
{
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

public class CyclicArrayEnumerator<T>(CyclicArray<T> array) : IEnumerator<T>
{
    public CyclicArray<T> Array { get; } = array;
    private int _position = -1;

    public bool MoveNext()
    {
        _position++;
        return true;
    }

    public void Reset()
    {
        _position = -1;
    }

    public T Current => Array[_position];

    object IEnumerator.Current => Current ?? throw new ArgumentNullException();

    public void Dispose() { }

    public int Length => Array.Length;
}
