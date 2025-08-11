using System.Collections;
using System.Diagnostics;

namespace Siteswap.Details;

public class CyclicArrayEnumerator<T>(CyclicArray<T> array) : IEnumerator<T>
{
    private CyclicArray<T> Array { get; } = array;
    private int _position = -1;

    public bool MoveNext()
    {
        _position++;
        Debug.Assert(_position < 10000, "Probably infinite loop");
        return true;
    }

    public void Reset()
    {
        _position = -1;
    }

    public T Current => Array[_position];

    object IEnumerator.Current => Current ?? throw new ArgumentNullException();

    public void Dispose() { }
}
