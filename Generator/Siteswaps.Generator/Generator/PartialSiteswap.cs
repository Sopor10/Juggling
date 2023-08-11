using Shared;

namespace Siteswaps.Generator.Generator;

public class PartialSiteswap
{
    internal PartialSiteswap(sbyte[] items)
    {
        var indexOf = (sbyte) Array.IndexOf(items, (sbyte)-1)switch
        {
            -1 => (sbyte) (items.Length - 1),
            var x =>(sbyte) (x - 1)
        };
        LastFilledPosition = indexOf;
        Interface = new CyclicArray<sbyte>(Enumerable.Repeat((sbyte)-1, items.Length));
        Items = Enumerable.Repeat((sbyte)-1, items.Length).ToArray();

        for (sbyte i = 0; i < items.Length; i++)
        {
            this[i] = items[i];
        }
    }

    public sbyte[] Items { get; }
    public CyclicArray<sbyte> Interface { get; }

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

    public sbyte LastFilledPosition { get; private set; }

    public bool IsFilled() => Items[^1] != -1;

    public static PartialSiteswap Standard(sbyte period, sbyte maxHeight) => new(Enumerable.Repeat((sbyte)-1, period - 1).Prepend(maxHeight).ToArray());


    public bool FillCurrentPosition(sbyte throwHeight)
    {
        var oldHeight = this[LastFilledPosition];
        if (oldHeight == throwHeight)
        {
            return true;
        }
        
        ResetCurrentPosition();

        if (Interface[LastFilledPosition + throwHeight] == -1)
        {
            this[LastFilledPosition] = throwHeight;
            return true;
        }

        this[LastFilledPosition] = oldHeight;

        return false;
    }

    public void ResetCurrentPosition()
    {
        var oldHeight = this[LastFilledPosition];
        if (oldHeight == -1)
        {
            return;
        }
        this[LastFilledPosition] = -1;
    }

    public void MoveForward()
    {
        LastFilledPosition++;
    }

    public void MoveBack()
    {
        ResetCurrentPosition();
        LastFilledPosition--;
    }
}
