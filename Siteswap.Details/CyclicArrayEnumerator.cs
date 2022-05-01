using System;
using System.Collections;
using System.Collections.Generic;

namespace Siteswap.Details;

public class CyclicArrayEnumerator<T> : IEnumerator<T>
{
    public CyclicArray<T> Array { get; }
    private int _position = -1;
    public CyclicArrayEnumerator(CyclicArray<T> array)
    {
        Array = array;
    }
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

    public void Dispose()
    {
    }

    public int Length => Array.Length;
}