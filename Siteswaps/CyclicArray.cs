using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Siteswaps
{
    public record CyclicArray<T> : IEnumerable<T>
    {
        public virtual bool Equals(CyclicArray<T>? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Items.SequenceEqual(other.Items);
        }

        public override int GetHashCode()
        {
            return Items.GetHashCode();
        }

        public CyclicArray(IEnumerable<T> items)
        {
            Items = items.ToImmutableArray();
        }

        private ImmutableArray<T> Items { get; }
        public int Length => Items.Length;

        public T this[int i]
        {
            get => Items[i % Items.Length];
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
                    yield return (j * Items.Length + k, Items[k]);

                }

            }
        }

        public IEnumerable<T> EnumerateValues(int i)
        {
            return Enumerate(i).Select(x => x.value);
        }

        public CyclicArray<T> Rotate(int i)
        {
            return new (this.Skip(i).Take(Length).ToImmutableArray());
        }
    }
    
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
}