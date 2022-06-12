using System.Collections.Immutable;
using System.Diagnostics;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain;

[DebuggerDisplay("{ToString()}")]
public record PartialSiteswap : IPartialSiteswap
{
    public const int Free = -1;

    public PartialSiteswap(params int[] items) : this(items.ToImmutableList())
    {
    }

    private PartialSiteswap(IEnumerable<int> items)
    {
        Items = items.ToImmutableList();
        var currentIndex = Items.IndexOf(Free) - 1;

        LastFilledPosition = currentIndex < 0 ? Items.Count - 1 : currentIndex;
        Max = Items.Max();
    }

    private int Max { get; }

    public int LastFilledPosition { get; }
    public ImmutableList<int> Items { get; }

    public virtual bool Equals(PartialSiteswap? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ToString().Equals(other.ToString());
    }
    
    public bool IsFilled() => Items.IndexOf(Free) == -1;

    public override int GetHashCode() => ToString().GetHashCode();

    public static PartialSiteswap Standard(int period, int maxHeight) => new(Enumerable.Repeat(Free, period - 1).Prepend(maxHeight).ToImmutableList());

    public override string ToString() => string.Join("", Items.Select(Transform));

    private string Transform(int i) =>
        i switch
        {
            < 10 => $"{i}",
            _ => Convert.ToChar(i + 87).ToString()
        };

    /// <summary>
    ///     Calculates the max height for the next free hand according ti the unique representation
    /// </summary>
    public int MaxForNextFree()
    {
        if (LastFilledPosition < 0) throw new InvalidOperationException("this Siteswap is already filled");

        var absteigendeSeq = Items.AbsteigendeSeq().ToList();
        var first = absteigendeSeq.First().ToImmutableList();
        var last = absteigendeSeq.Last().ToImmutableList();

        int possibleMax;
        if (CountOpenPositions() > 1)
            possibleMax = Max;
        else
            possibleMax = Max - 1;

        for (var i = possibleMax; i >= 0; i--)
        {
            var newLast = last.SetItem(last.IndexOf(Free), i);
            if (newLast.Contains(Free)) newLast = newLast.Replace(Free, 0);
            var result = first.CompareSequences(newLast);

            if (result > 0) return i;
        }

        return possibleMax;
    }

    private int CountOpenPositions() => Items.Count(x => x < 0);

    public PartialSiteswap WithLastFilledPosition(int i) => new(Items.SetItem(LastFilledPosition, i));

    public int ValueAtCurrentIndex() => Items[LastFilledPosition];

    public PartialSiteswap? CreateNextFilledPosition(SiteswapGeneratorInput input)
    {
        if (LastFilledPosition < 0 || LastFilledPosition >= input.Period - 1) return null;

        int value = GetNextMax(input);
        return (PartialSiteswap)new(Items.SetItem(LastFilledPosition + 1, value));
    }

    private int GetNextMax(SiteswapGeneratorInput input) =>
        new[]
        {
            MaxForNextFree(),
            input.MaxHeight
        }.Min();
}