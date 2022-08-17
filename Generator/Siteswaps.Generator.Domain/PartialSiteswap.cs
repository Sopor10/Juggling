using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Linq.Extras;
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

    private PartialSiteswap(ImmutableList<int> items, int posOfMaxPossibleValue = 0)
    {
        Items = items;
        PosOfMaxPossibleValue = posOfMaxPossibleValue;
        var currentIndex = Items.IndexOf(Free) - 1;

        LastFilledPosition = currentIndex < 0 ? Items.Count - 1 : currentIndex;
    }


    public int LastFilledPosition { get; }
    public ImmutableList<int> Items { get; }
    ReadOnlyCollection<int> IPartialSiteswap.Items => this.Items.AsReadOnly();
    private int PosOfMaxPossibleValue { get; }

    public virtual bool Equals(PartialSiteswap? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ToString().Equals(other.ToString());
    }

    public bool IsFilled() => Items.IndexOf(Free) == -1;

    public override int GetHashCode() => ToString().GetHashCode();

    public static PartialSiteswap Standard(int period, int maxHeight) =>
        new(Enumerable.Repeat(Free, period - 1).Prepend(maxHeight).ToImmutableList());

    public override string ToString() => string.Join("", Items.Select(Transform));

    private string Transform(int i) =>
        i switch
        {
            < 10 => $"{i}",
            _ => Convert.ToChar(i + 87).ToString()
        };

    public PartialSiteswap WithLastFilledPosition(int i) => new(Items.SetItem(LastFilledPosition, i));

    public int ValueAtCurrentIndex() => Items[LastFilledPosition];

    public PartialSiteswap? CreateNextFilledPosition(SiteswapGeneratorInput input)
    {
        if (LastFilledPosition < 0 || LastFilledPosition >= input.Period - 1) return null;

        return new PartialSiteswap(Items.SetItem(LastFilledPosition + 1, Items[PosOfMaxPossibleValue]), LastFilledPosition + 1);
    }
}
