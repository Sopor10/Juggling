﻿using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Shared;

namespace Siteswaps.Generator.Generator;

public record Siteswap
{
    private CyclicArray<int> Items { get; }

    public static bool TryCreate(IEnumerable<int> items, [NotNullWhen(true)] out Siteswap? siteswap)
    {
        return TryCreate(new(items), out siteswap);
    }
    
    public static Siteswap CreateFromCorrect(IEnumerable<int> items)
    {
        return new Siteswap(items.ToCyclicArray(), true);
    }
    private Siteswap(CyclicArray<int> items, bool isAlreadyUnique = false)
    {
        Items = isAlreadyUnique ? items : ToUniqueRepresentation(items);
    }

    public static bool TryCreate(CyclicArray<int> items, [NotNullWhen(true)]out Siteswap? siteswap)
    {
        if (IsValid(items))
        {
            siteswap = new(items);
            return true;
        }

        siteswap = null;
        return false;
    }

    private static bool IsValid(CyclicArray<int> items) =>
        items
            .Enumerate(1)
            .Select(x => x.value)
            .Select((x, i) => (x + i) % items.Length)
            .ToHashSet()
            .Count == items.Length;

    private static CyclicArray<int> ToUniqueRepresentation(CyclicArray<int> input)
    {
        var biggest = input.EnumerateValues(1).ToList();

        foreach (var list in Enumerable.Repeat(1,input.Length).Select(input.Rotate).Select(x => x.EnumerateValues(1).ToList()))
        {
            if (biggest.CompareSequences(list) < 0)
            {
                biggest = list.ToList();
            }
        }

        return biggest.ToCyclicArray();
    }

    public override string ToString()
    {
        return string.Join("", Items.EnumerateValues(1).Select(Transform));
    }

    private string Transform(int i)
    {
        return i switch
        {
            < 10 => $"{i}",
            _ => Convert.ToChar(i + 87).ToString()
        };
    }

    public virtual bool Equals(Siteswap? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ToString().Equals(other.ToString());
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }


    public static Siteswap CreateFromCorrect(sbyte[] partialSiteswapItems)
    {
        return CreateFromCorrect(partialSiteswapItems.Select(x => (int)x));
    }
}