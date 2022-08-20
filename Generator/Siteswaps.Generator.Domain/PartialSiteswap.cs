using System.Collections.ObjectModel;
using Linq.Extras;
using Shared;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain;

public class PartialSiteswap : IPartialSiteswap
{
    internal PartialSiteswap(sbyte[] items, sbyte lastFilledPosition = 0)
    {
        LastFilledPosition = lastFilledPosition;
        Items = items;
        Interface = new CyclicArray<sbyte>(Enumerable.Repeat((sbyte)-1, Items.Length));

        for (sbyte i = 0; i < Items.Length; i++)
        {
            Interface[i + this[i]] = this[i];
        }

        foreach (var item in Items)
        {
            if (item>0)
            {
                PartialSum += item;
            }
        }
    }

    public sbyte[] Items { get; }
    private CyclicArray<sbyte> Interface { get; }

    public sbyte PartialSum { get; set; }
    

    private sbyte this[sbyte i]
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
                PartialSum -= oldValue;

            }
            
            Items[i] = value;
            if (value == -1)
            {
                return;
            }

            PartialSum += value;
            Interface[i + value] = value;
        }
    }

    ReadOnlyCollection<sbyte> IPartialSiteswap.Items => Items.AsReadOnly();
    public sbyte LastFilledPosition { get; private set; }

    public bool IsFilled()
    {
        return Items.Last() != -1;
    }

    public static PartialSiteswap Standard(sbyte period, sbyte maxHeight)
    {
        return new PartialSiteswap(Enumerable.Repeat((sbyte)-1, period - 1).Prepend(maxHeight).ToArray());
    }


    public bool FillCurrentPosition(sbyte throwHeight)
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

    public void MoveForward(sbyte max)
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