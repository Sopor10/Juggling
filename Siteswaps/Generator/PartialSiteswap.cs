using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Siteswaps.Generator;

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
        return ToString().Equals(other.ToString());
    }

    public override int GetHashCode() => ToString().GetHashCode();

    public static PartialSiteswap Standard(int period, int maxHeight) =>
        new (Enumerable.Repeat(Free, period - 1).Prepend(maxHeight).ToImmutableList());

    public override string ToString()
    {
        return string.Join("", Items.Select(Transform)) ;
    }
    private string Transform(int i) =>
        i switch
        {
            < 10 => $"{i}",
            _ => Convert.ToChar(i + 87).ToString()
        };

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

        int possibleMax;
        if (CountOpenPositions() > 1)
        {
            possibleMax = Max();
        }
        else
        {
            possibleMax = Max() - 1;
        }
            
        for (var i = possibleMax; i >= 0; i--)
        {
            var newLast = last.SetItem(last.IndexOf(Free), i);
            if (newLast.Contains(Free))
            {
                newLast = newLast.Replace(Free, 0);
            }
            var result =  first.CompareSequences(newLast);

            if (result > 0)
            {
                return i;

            }
        }

        return possibleMax;
    }

    private int CountOpenPositions() => Items.Count(x => x < 0);

    public PartialSiteswap WithLastFilledPosition(int i) => SetPosition(CurrentIndex(), i);

    public int ValueAtCurrentIndex() => Items[CurrentIndex()];
}