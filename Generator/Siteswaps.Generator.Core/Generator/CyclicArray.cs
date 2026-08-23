using System.Collections;
using System.Runtime.CompilerServices;

namespace Siteswaps.Generator.Core.Generator;

public record CyclicArray<T> : IEnumerable<T>
{
    public CyclicArray(IEnumerable<T> items, int rotationIndex = 0)
    {
        RotationIndex = rotationIndex;
        Items = items.ToArray();
    }

    public int RotationIndex { get; set; }
    private T[] Items { get; }
    public int Length => Items.Length;

    internal void SetStorage(int index, T value) => Items[index] = value;

    public T this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Items[GetStorageIndex(i)];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Items[GetStorageIndex(i)] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetStorageIndex(int index)
    {
        index += RotationIndex;
        if ((uint)index < (uint)Items.Length)
            return index;

        index %= Items.Length;
        return (index + Items.Length) % Items.Length;
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

    public Span<T> AsSpan()
    {
        if (RotationIndex % Items.Length == 0)
            return Items.AsSpan();

        var rotated = new T[Items.Length];
        for (int i = 0; i < Items.Length; i++)
            rotated[i] = this[i];
        return rotated;
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
        return _position < Array.Length;
    }

    public void Reset()
    {
        _position = -1;
    }

    public T Current => Array[_position];

    object IEnumerator.Current => Current ?? throw new ArgumentNullException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public int Length => Array.Length;
}
