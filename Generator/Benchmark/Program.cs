using Benchmark;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Siteswaps.Generator.Generator;

var summary = BenchmarkRunner.Run<GeneratorBenchmarks>();

namespace Benchmark
{
    using Siteswap = Siteswaps.Generator.Generator.Siteswap;

    [HtmlExporter]
    [MemoryDiagnoser]
    public class GeneratorBenchmarks
    {
        [Benchmark]
        public static async Task<List<Siteswap>> Generate()
        {
            var generator = new SiteswapGeneratorFactory().Create(Input());
            return await generator.GenerateAsync().ToListAsync();
        }

        private static SiteswapGeneratorInput Input() =>
            new()
            {
                Period = 3,
                MaxHeight = 10,
                MinHeight = 2,
                NumberOfObjects = 5,
                StopCriteria = new StopCriteria(TimeSpan.FromDays(5), Int32.MaxValue)
            };
    }
}
