using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Domain.Filter;
using Siteswaps.Generator.Domain;

var summary = BenchmarkRunner.Run(typeof(Program).Assembly);



namespace Benchmark
{
    [HtmlExporter]
    public class GeneratorBenchmarks
    {

        [Benchmark]
        public async Task<IEnumerable<ISiteswap>> Generate_Old()
        {
            var generator = new Siteswaps.Generator.Domain.OldGenerator.SiteswapGeneratorFactory(new FilterBuilderFactory())
                .WithInput(Input())
                .Create();
            return await generator.GenerateAsync();
        }
        
        
        [Benchmark]
        public async Task<IEnumerable<ISiteswap>> Generate_New()
        {
            var generator = new Siteswaps.Generator.Domain.NewGenerator.SiteswapGeneratorFactory(new FilterBuilderFactory())
                .WithInput(Input())
                .Create();
            return await generator.GenerateAsync();
        }

        private static SiteswapGeneratorInput Input()
        {
            var input = new SiteswapGeneratorInput()
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