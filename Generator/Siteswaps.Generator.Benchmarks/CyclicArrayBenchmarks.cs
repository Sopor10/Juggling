using BenchmarkDotNet.Attributes;

namespace Siteswaps.Generator.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class CyclicArrayBenchmarks
{
    private int[] _data = null!;
    private int[] _indexCache = null!;
    private int[] _duplicatedData = null!;
    private int _length;
    private int _rotationIndex;

    [Params(5, 10, 14)]
    public int Period { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _length = Period;
        _rotationIndex = Period / 2;
        _data = Enumerable.Range(0, _length).ToArray();

        var cacheSize = _length * 3;
        _indexCache = new int[cacheSize];
        for (int i = 0; i < cacheSize; i++)
            _indexCache[i] = i % _length;

        _duplicatedData = new int[_length * 3];
        for (int i = 0; i < _duplicatedData.Length; i++)
            _duplicatedData[i] = _data[i % _length];
    }

    [Benchmark(Baseline = true)]
    public int Modulo()
    {
        int sum = 0;
        for (int rot = 0; rot < _length; rot++)
        {
            for (int i = 0; i < _length; i++)
            {
                sum += _data[(i + rot) % _length];
            }
        }
        return sum;
    }

    [Benchmark]
    public int IndexCache()
    {
        int sum = 0;
        for (int rot = 0; rot < _length; rot++)
        {
            for (int i = 0; i < _length; i++)
            {
                sum += _data[_indexCache[i + rot]];
            }
        }
        return sum;
    }

    [Benchmark]
    public int DuplicatedArray()
    {
        int sum = 0;
        for (int rot = 0; rot < _length; rot++)
        {
            for (int i = 0; i < _length; i++)
            {
                sum += _duplicatedData[i + rot];
            }
        }
        return sum;
    }

    [Benchmark]
    public int BitMaskPowerOf2()
    {
        int mask = 16 - 1;
        int sum = 0;
        for (int rot = 0; rot < _length; rot++)
        {
            for (int i = 0; i < _length; i++)
            {
                sum += _data[(i + rot) & mask];
            }
        }
        return sum;
    }

    [Benchmark]
    public int ConditionalSubtract()
    {
        int sum = 0;
        for (int rot = 0; rot < _length; rot++)
        {
            for (int i = 0; i < _length; i++)
            {
                int idx = i + rot;
                if (idx >= _length)
                    idx -= _length;
                sum += _data[idx];
            }
        }
        return sum;
    }
}
