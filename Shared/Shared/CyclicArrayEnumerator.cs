using System.Collections;

namespace Shared;

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
