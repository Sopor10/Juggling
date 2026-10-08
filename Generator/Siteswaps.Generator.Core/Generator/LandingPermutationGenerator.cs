using System.Runtime.CompilerServices;

namespace Siteswaps.Generator.Core.Generator;

/// <summary>
/// Generates unfiltered patterns by walking injective landing residues directly.
/// This is deliberately separate from the filter-aware generator: filters receive
/// the full <see cref="PartialSiteswap"/> contract and keep the established path.
/// </summary>
internal sealed class LandingPermutationGenerator(SiteswapGeneratorInput input)
{
    private readonly int _period = input.Period;
    private readonly int _minHeight = input.MinHeight;
    private readonly int _maxHeight = input.MaxHeight;
    private readonly int _targetSum = input.NumberOfObjects * input.Period;
    private readonly bool _useLandingMask = input.Period <= 64;
    private readonly int[] _items = new int[input.Period];
    private readonly bool[]? _occupiedLandings = input.Period > 64 ? new bool[input.Period] : null;
    private ulong _occupiedLandingMask;
    private int _position;
    private int _partialSum;

    public void Generate(CancellationToken token, List<Siteswap> results, int maxResults)
    {
        Array.Fill(_items, -1);
        _items[0] = _maxHeight;
        _partialSum = _maxHeight;
        SetLanding(_maxHeight);
        BackTrack(0, token, results, maxResults);
    }

    private void BackTrack(
        int uniqueMaxIndex,
        CancellationToken token,
        List<Siteswap> results,
        int maxResults
    )
    {
        if (token.IsCancellationRequested || results.Count >= maxResults)
            return;

        var uniqueMax =
            _items[uniqueMaxIndex] != -1 ? _items[uniqueMaxIndex] : _items[uniqueMaxIndex - 1];
        if (_items[_position] != -1)
            ClearItem(_items[_position]);

        var min = _minHeight;
        var max = uniqueMax;
        var remainingAfterCurrent = _period - _position - 1;

        if (remainingAfterCurrent > 0)
        {
            var maxSumRest = GetMaxSumToGenerate(_position + 1);
            var minSumRest = GetMinSumToGenerate(_position + 1);

            var tightMin = _targetSum - _partialSum - maxSumRest;
            if (tightMin > min)
                min = tightMin;

            if (minSumRest < int.MaxValue)
            {
                var tightMax = _targetSum - _partialSum - minSumRest;
                if (tightMax < max)
                    max = tightMax;
            }
        }

        if (min > max)
            return;

        for (var height = max; height >= min; height--)
        {
            if (results.Count >= maxResults)
                return;
            if (!IsLandingFree(_position + height))
                continue;

            SetItem(height);
            if (_position == _period - 1)
            {
                if (_partialSum == _targetSum && _items[^1] != uniqueMax)
                    results.Add(Siteswap.CreateFromGenerated(_items));
            }
            else
            {
                _position++;
                BackTrack(height == uniqueMax ? uniqueMaxIndex + 1 : 0, token, results, maxResults);
                _position--;
            }
            ClearItem(height);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetItem(int height)
    {
        _items[_position] = height;
        _partialSum += height;
        SetLanding(_position + height);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClearItem(int height)
    {
        ClearLanding(_position + height);
        _items[_position] = -1;
        _partialSum -= height;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsLandingFree(int index)
    {
        var storageIndex = GetStorageIndex(index);
        return _useLandingMask
            ? (_occupiedLandingMask & (1UL << storageIndex)) == 0
            : !_occupiedLandings![storageIndex];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FindFreeLandingAtOrBefore(int index, int lowerBound)
    {
        while (!IsLandingFree(index))
        {
            index--;
            if (index < lowerBound)
                return int.MinValue;
        }
        return index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FindFreeLandingAtOrAfter(int index, int upperBound)
    {
        while (!IsLandingFree(index))
        {
            index++;
            if (index > upperBound)
                return int.MaxValue;
        }
        return index;
    }

    private int GetMaxSumToGenerate(int fromIndex)
    {
        var maxSum = 0;
        var interfaceIndex = _period + _maxHeight - 2;
        for (var i = _period - 1; i >= fromIndex; i--)
        {
            interfaceIndex = FindFreeLandingAtOrBefore(interfaceIndex, i);
            if (interfaceIndex < i)
                return 0;

            maxSum += interfaceIndex - i;
            interfaceIndex--;
        }
        return maxSum;
    }

    private int GetMinSumToGenerate(int fromIndex)
    {
        var minSum = 0;
        var interfaceIndex = fromIndex + _minHeight;
        for (var i = fromIndex; i < _period; i++)
        {
            interfaceIndex = FindFreeLandingAtOrAfter(interfaceIndex, i + _maxHeight);
            if (interfaceIndex == int.MaxValue)
                return int.MaxValue;

            minSum += interfaceIndex - i;
            interfaceIndex++;
        }
        return minSum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetLanding(int index)
    {
        var storageIndex = GetStorageIndex(index);
        if (_useLandingMask)
            _occupiedLandingMask |= 1UL << storageIndex;
        else
            _occupiedLandings![storageIndex] = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClearLanding(int index)
    {
        var storageIndex = GetStorageIndex(index);
        if (_useLandingMask)
            _occupiedLandingMask &= ~(1UL << storageIndex);
        else
            _occupiedLandings![storageIndex] = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetStorageIndex(int index)
    {
        if ((uint)index < (uint)_period)
            return index;

        index %= _period;
        return index < 0 ? index + _period : index;
    }
}
