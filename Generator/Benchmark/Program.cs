using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Domain;
using Siteswaps.Generator.Domain.Filter;

var summary = BenchmarkRunner.Run(typeof(Program).Assembly);



namespace Benchmark
{
    [MarkdownExporter, AsciiDocExporter, HtmlExporter, CsvExporter, RPlotExporter]
    public class GeneratorBenchmarks
    {

        [Benchmark]
        public void Generate()
        {
            var input = new SiteswapGeneratorInput()
            {
                Period = 5,
                MaxHeight = 10,
                MinHeight = 2,
                NumberOfObjects = 7,
                StopCriteria = new StopCriteria(TimeSpan.FromDays(5), Int32.MaxValue)
            };
            var generator = new SiteswapGeneratorFactory(new FilterBuilderFactory())
                .WithInput(input)
                .Create();
            generator.GenerateAsync();
        }
    }
}