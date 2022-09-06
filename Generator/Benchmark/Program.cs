using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Siteswaps.Generator.Generator;

var summary = BenchmarkRunner.Run(typeof(Program).Assembly);

namespace Benchmark
{
    [HtmlExporter]
    public class GeneratorBenchmarks
    {
        [Benchmark]
        public async Task<IEnumerable<Siteswap>> Generate_New()
        {
            var generator = new SiteswapGeneratorFactory().Create(Input());
            return await generator.GenerateAsync().ToListAsync();
        }

        private static SiteswapGeneratorInput Input()
        {
            var input = new SiteswapGeneratorInput
            {
                Period = 3,
                MaxHeight = 10,
                MinHeight = 2,
                NumberOfObjects = 5,
                StopCriteria = new StopCriteria(TimeSpan.FromDays(5), Int32.MaxValue)
            };
            return input;
        }
    }
}