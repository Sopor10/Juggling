using System.Collections.ObjectModel;
using Linq.Extras;
using Shared;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain.NewGenerator;

public class PartialSiteswap : IPartialSiteswap
{
    internal PartialSiteswap(int[] items, int lastFilledPosition = 0)
    {
        LastFilledPosition = lastFilledPosition;
        Items = items;
        Interface = new CyclicArray<int>(Enumerable.Repeat(-1, Items.Length));

        for (var i = 0; i < Items.Length; i++)
        {
            Interface[i + this[i]] = this[i];
        }

        PartialSum = Items.Where(x => x > 0).Sum();
    }

    public int[] Items { get; }
    private CyclicArray<int> Interface { get; }

    public int PartialSum { get; set; }
    

    private int this[int i]
    {
        get => Items[i];
        set
        {
            var oldValue = Items[i];
            if (oldValue == value)
            {
                return;
            }
            
            if (oldValue != -1)
            {
                Interface[i + oldValue] = -1;
            }
            
            Items[i] = value;
            PartialSum += value;
            PartialSum -= Math.Abs(oldValue);
            if (value == -1)
            {
                return;
            }
            Interface[i + value] = value;
        }
    }

    ReadOnlyCollection<int> IPartialSiteswap.Items => Items.AsReadOnly();
    public int LastFilledPosition { get; private set; }

    public bool IsFilled()
    {
        return Items.Last() != -1;
    }

    public static PartialSiteswap Standard(int period, int maxHeight)
    {
        return new PartialSiteswap(Enumerable.Repeat(-1, period - 1).Prepend(maxHeight).ToArray());
    }


    public bool FillCurrentPosition(int throwHeight)
    {
        var oldHeight = this[LastFilledPosition];
        if (oldHeight == throwHeight)
        {
            return true;
        }
        this[LastFilledPosition] = -1;

        if (Interface[LastFilledPosition + throwHeight] == -1)
        {
            this[LastFilledPosition] = throwHeight;
            return true;
        }

        this[LastFilledPosition] = oldHeight;

        return false;
    }

    public void MoveForward(int max)
    {
        LastFilledPosition++;
        var result= FillCurrentPosition(max);
    }

    public void MoveBack()
    {
        this[LastFilledPosition] = -1;
        LastFilledPosition--;
    }
}