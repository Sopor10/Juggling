using System.Runtime.CompilerServices;

namespace Siteswaps.Generator.Core.Generator;

public class PartialSiteswap
{
    private readonly bool useLandingMask;
    private ulong occupiedLandingMask;

    public PartialSiteswap(int[] items, int lastFilledPosition = 0)
    {
        LastFilledPosition = lastFilledPosition;
        useLandingMask = items.Length <= 64;
        Interface = new CyclicArray<int>(Enumerable.Repeat(-1, items.Length));
        Items = Enumerable.Repeat(-1, items.Length).ToCyclicArray();

        for (int i = 0; i < items.Length; i++)
        {
            this[i] = items[i];
        }
    }

    public CyclicArray<int> Items { get; }
    public CyclicArray<int> Interface { get; }

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
                var landingIndex = GetInterfaceStorageIndex(i + oldValue);
                ClearLanding(landingIndex);
                SetInterfaceValue(i + oldValue, landingIndex, -1);
                PartialSum -= oldValue;
            }

            Items[i] = value;
            if (value == -1)
            {
                return;
            }

            PartialSum += value;
            var newLandingIndex = GetInterfaceStorageIndex(i + value);
            SetLanding(newLandingIndex);
            SetInterfaceValue(i + value, newLandingIndex, value);
        }
    }

    public int LastFilledPosition { get; private set; }

    public int RotationIndex
    {
        get { return this.Interface.RotationIndex; }
        set
        {
            this.Interface.RotationIndex = value;
            this.Items.RotationIndex = value;
        }
    }

    public int Length => Items.Length;

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

        if (!IsLandingFree(LastFilledPosition + throwHeight))
        {
            return false;
        }

        ResetCurrentPosition();
        this[LastFilledPosition] = throwHeight;
        return true;
    }

    public void ResetCurrentPosition()
    {
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

    public Span<int> AsSpan()
    {
        return Items.AsSpan();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsLandingFree(int index)
    {
        if (!useLandingMask)
        {
            return Interface[index] == -1;
        }

        return (occupiedLandingMask & (1UL << GetInterfaceStorageIndex(index))) == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int FindFreeLandingAtOrBefore(int index, int lowerBound)
    {
        if (!useLandingMask)
        {
            while (!IsLandingFree(index))
            {
                index--;
                if (index < lowerBound)
                    return int.MinValue;
            }
            return index;
        }

        var storageIndex = GetInterfaceStorageIndex(index);
        while ((occupiedLandingMask & (1UL << storageIndex)) != 0)
        {
            index--;
            if (index < lowerBound)
                return int.MinValue;
            storageIndex--;
            if (storageIndex < 0)
                storageIndex = Length - 1;
        }
        return index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int FindFreeLandingAtOrAfter(int index, int upperBound)
    {
        if (!useLandingMask)
        {
            while (!IsLandingFree(index))
            {
                index++;
                if (index > upperBound)
                    return int.MaxValue;
            }
            return index;
        }

        var storageIndex = GetInterfaceStorageIndex(index);
        while ((occupiedLandingMask & (1UL << storageIndex)) != 0)
        {
            index++;
            if (index > upperBound)
                return int.MaxValue;
            storageIndex++;
            if (storageIndex == Length)
                storageIndex = 0;
        }
        return index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetLanding(int storageIndex)
    {
        if (useLandingMask)
        {
            occupiedLandingMask |= 1UL << storageIndex;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClearLanding(int storageIndex)
    {
        if (useLandingMask)
        {
            occupiedLandingMask &= ~(1UL << storageIndex);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetInterfaceValue(int logicalIndex, int storageIndex, int value)
    {
        if (Interface.RotationIndex == 0)
        {
            Interface.SetStorage(storageIndex, value);
        }
        else
        {
            Interface[logicalIndex] = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetInterfaceStorageIndex(int index)
    {
        index += Interface.RotationIndex;
        if ((uint)index < (uint)Length)
            return index;

        index %= Length;
        return index < 0 ? index + Length : index;
    }
}
