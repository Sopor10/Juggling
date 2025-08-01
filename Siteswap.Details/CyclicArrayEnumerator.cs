using System.Collections;

namespace Siteswap.Details;

public class CyclicArrayEnumerator<T> : IEnumerator<T>
{
    private CyclicArray<T> Array { get; }
    private int _position = -1;

    public CyclicArrayEnumerator(CyclicArray<T> array)
    {
        Array = array;
    }

    public bool MoveNext()
    {
        _position++;
        if (_position >= 10000)
        {
            return false;
        }
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
