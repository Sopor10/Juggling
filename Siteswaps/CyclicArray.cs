using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Siteswaps
{
    public class CyclicArray<T> : IEnumerable<T>
    {
        public CyclicArray(T[] items)
        {
            Items = items;
        }

        private T[] Items { get; }
        public int Length => Items.Length;

        public T this[int i]
        {
            get => Items[i % Items.Length];
            set => Items[i % Items.Length] = value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
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
    }
}