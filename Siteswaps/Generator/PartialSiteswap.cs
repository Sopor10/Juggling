using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace Siteswaps.Generator
{
    [DebuggerDisplay("{ToString()}")]
    public record PartialSiteswap
    {

        public PartialSiteswap(params int[] items) : this(items.ToImmutableList())
        {
            
        }

        private PartialSiteswap(IEnumerable<int> items)
        {
            Items = items.ToImmutableList();
        }

        public const int Free = -1;
        public ImmutableList<int> Items { get; private init; }

        public virtual bool Equals(PartialSiteswap? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Items.SequenceEqual(other.Items);
        }

        public override int GetHashCode()
        {
            var hashcode = 0;
            foreach (var item in Items)
            {
                hashcode = item.GetHashCode() * 397 ^ hashcode;
            }
            return hashcode;
        }

        public static PartialSiteswap Standard(int period, int maxHeight) =>
            new (Enumerable.Repeat(Free, period - 1).Prepend(maxHeight).ToImmutableList());

        public override string ToString()
        {
            return string.Join("", Items) ;
        }

        public bool IsFilled()
        {
            return Items.IndexOf(Free) == -1;
        }

        /// <summary>
        /// returns the position of the first Free hand.
        /// If all positions are filled, this function returns -1
        /// </summary>
        /// <returns></returns>
        public int CurrentIndex() => Items.IndexOf(Free) - 1;

        public int Max() => Items.Max();

        public int Period() => Items.Count;

        public PartialSiteswap SetPosition(int index, int value) =>
            this with
            {
                Items = Items.SetItem(index, value)
            };

        /// <summary>
        /// Calculates the max height for the next free hand according ti the unique representation
        /// </summary>
        public int MaxForNextFree()
        {
            return Items[CurrentIndex()];
        }
    }
}