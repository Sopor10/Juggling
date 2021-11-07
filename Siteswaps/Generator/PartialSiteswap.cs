using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Linq.Extras;

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
        public int CurrentIndex()
        {
            var currentIndex = Items.IndexOf(Free) - 1;
            if (currentIndex < 0)
            {
                return Items.Count - 1;
            }
            
            return currentIndex;
        }

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
            if (CurrentIndex()<0)
            {
                throw new InvalidOperationException("this Siteswap is already filled");
            }


            var absteigendeSeq = Items.AbsteigendeSeq().ToList();
            var first = absteigendeSeq.First().ToImmutableList();
            var last = absteigendeSeq.Last().ToImmutableList();
            
            if (first.SequenceEqual(last))
            {
                if (CountOpenPositions() == 1)
                {
                    return Max() - 1;
                }
            }

            for (var i = Max(); i >= 0; i--)
            {
                var result =  first.CompareSequences(last.SetItem(last.IndexOf(Free), i));

                if (result > 0)
                {
                    return i;
                }
            }

            return Max();
        }



        private int CountOpenPositions() => Items.Count(x => x < 0);

        public PartialSiteswap WithLastFilledPosition(int i) => SetPosition(CurrentIndex(), i);

        public int ValueAtCurrentIndex() => Items[CurrentIndex()];
    }
}