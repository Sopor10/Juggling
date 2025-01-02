using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Siteswap.Details;

public record Siteswap(CyclicArray<int> Items)
{
    public Siteswap(params int[] items) : this(new CyclicArray<int>(items))
    {
        IsValid(new CyclicArray<int>(items));
    }
    
    public static bool TryCreate(string value, [NotNullWhen(true)] out Siteswap? siteswap)
    {
        siteswap = null;
        if (value == string.Empty)
        {
            return false;
        }

        return TryCreate(value.Select(ToInt), out siteswap);
    }

    private static int ToInt(char c)
    {
        var tryParse = int.TryParse(c.ToString(), out var value);
        if (tryParse)
        {
            return value;
        }

        return c - 87;
    }

    public static bool TryCreate(IEnumerable<int> items, [NotNullWhen(true)] out Siteswap? siteswap) => 
        TryCreate(items.ToCyclicArray(), out siteswap);

    private static bool TryCreate(CyclicArray<int> items, [NotNullWhen(true)]out Siteswap? siteswap)
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

        foreach (var list in Enumerable.Range(0,input.Length).Select(input.Rotate).Select(x => x.EnumerateValues(1).ToList()))
        {
            if (biggest.CompareSequences(list) < 0)
            {
                biggest = list;
            }
        }

        return biggest.ToCyclicArray();
    }

    private bool IsGroundState() => HasNoRethrow();

    private bool HasNoRethrow() => !Items.Enumerate(1).Any(x => x.position + x.value < NumberOfObjects());

    public bool IsExcitedState() => !IsGroundState();
    public decimal NumberOfObjects() => (decimal)Items.Enumerate(1).Average(x => x.value);

    public override string ToString()
    {
        return string.Join("", Items.EnumerateValues(1).Select(Transform));
    }

    public static string Transform(int i)
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

    public int Max() => Items.EnumerateValues(1).Max();
    
    public int this[int i] => Items[i];

    public static CyclicArray<int> ToUniqueRepresentation(int[] input) => ToUniqueRepresentation(input.ToCyclicArray());


    public List<Orbit> GetOrbits() => GetOrbitsInternal().ToList();
    
    private IEnumerable<Orbit> GetOrbitsInternal()
    {
        var visited = new bool[Items.Length];
        
        for (int i = 0; i < Items.Length; i++)
        {
            if (visited[i])
            {
                continue;
            }

            var orbitIndices = new List<int>();
            var current = i;
            
            // Sammle alle Indizes im aktuellen Orbit
            do
            {
                visited[current] = true;
                orbitIndices.Add(current);
                current = (current + Items[current]) % Items.Length;
            } while (current != i);

            // Erstelle eine Orbit-Liste mit 0 als Standardwert
            var orbitValues = new int[Items.Length];
            for (int j = 0; j < orbitValues.Length; j++)
            {
                orbitValues[j] = 0;
            }

            // Setze die Werte an den Orbit-Positionen
            foreach (var index in orbitIndices)
            {
                orbitValues[index] = Items[index];
            }

            yield return new Orbit(orbitValues.ToList());
        }
    }
    
}

public class Orbit(List<int> items)
{
    public List<int> Items => items;
}
