using System.Collections.ObjectModel;
using Linq.Extras;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain.NewGenerator;

public class PartialSiteswap : IPartialSiteswap
{
    private PartialSiteswap(int[] items, int lastFilledPosition = 0)
    {
        LastFilledPosition = lastFilledPosition;
        Items = items;
    }

    public int[] Items { get; }
    public int CurrentHeight => Items[LastFilledPosition];

    ReadOnlyCollection<int> IPartialSiteswap.Items => Items.AsReadOnly();
    public int LastFilledPosition { get; private set; }

    public bool IsFilled()
    {
        return Items.Last() != -1;
    }

    public static PartialSiteswap Standard(int period, int maxHeight)
    {
        return new PartialSiteswap(Enumerable.Prepend(Enumerable.Repeat(-1, period - 1), maxHeight).ToArray());
    }


    public void FillCurrentPosition(int throwHeight)
    {
        Items[LastFilledPosition] = throwHeight;
    }

    public void MoveForward(int height)
    {
        Items[LastFilledPosition + 1] = height;
        LastFilledPosition++;
    }

    public void MoveBack()
    {
        Items[LastFilledPosition] = -1;
        LastFilledPosition--;
    }
}

public class SiteswapGenerator : ISiteswapGenerator
{
    public SiteswapGenerator(ISiteswapFilter filter, SiteswapGeneratorInput input)
    {
        Filter = filter;
        Input = input;
        PartialSiteswap = PartialSiteswap.Standard(Input.Period, Input.MaxHeight);
    }

    public HashSet<ISiteswap> Siteswaps { get; } = new();
    public ISiteswapFilter Filter { get; }
    public SiteswapGeneratorInput Input { get; }
    public PartialSiteswap PartialSiteswap { get; }

    public async Task<IEnumerable<ISiteswap>> GenerateAsync()
    {
        BackTrack(0);

        return Siteswaps;
    }

    private void BackTrack(int uniqueMaxIndex)
    {
        var min = Input.MinHeight;
        var max = PartialSiteswap.Items[uniqueMaxIndex];

        for (var i = max; i >= min; i--)
        {
            PartialSiteswap.FillCurrentPosition(i);
            if (Filter.CanFulfill(PartialSiteswap))
            {
                if (PartialSiteswap.IsFilled())
                {
                    if (Siteswap.TryCreate(PartialSiteswap.Items, out var s)) Siteswaps.Add(s);
                    return;
                }

                PartialSiteswap.MoveForward(max);
                BackTrack(i==max ? uniqueMaxIndex + 1: 0);
                PartialSiteswap.MoveBack();
            }
        }
    }
}
