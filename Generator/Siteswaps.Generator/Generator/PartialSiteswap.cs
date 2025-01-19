using Shared;

namespace Siteswaps.Generator.Generator;

public class PartialSiteswap
{
    internal PartialSiteswap(int[] items, int lastFilledPosition = 0)
    {
        LastFilledPosition = lastFilledPosition;
        Interface = new CyclicArray<int>(Enumerable.Repeat(-1, items.Length));
        Items = new int[items.Length];

        for (int i = 0; i < items.Length; i++)
        {
            this[i] = items[i];
        }
    }

    public int[] Items { get; }
    private CyclicArray<int> Interface { get; }

    public int PartialSum { get; private set; }

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

    public int LastFilledPosition { get; private set; }

    public bool IsFilled() => Items[^1] != -1;

    public static PartialSiteswap Standard(int period, int maxHeight) =>
        new(Enumerable.Repeat(-1, period - 1).Prepend(maxHeight).ToArray());

    public bool FillCurrentPosition(int throwHeight)
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
