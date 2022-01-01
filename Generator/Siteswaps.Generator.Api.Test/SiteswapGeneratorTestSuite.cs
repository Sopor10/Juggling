using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Siteswaps.Generator.Api.Test;

public abstract class SiteswapGeneratorTestSuite
{
    protected abstract ISiteswapGenerator CreateTestObject(SiteswapGeneratorInput input);

    [Test]
    [TestCase(new[] { 5, 3, 1 })]
    [TestCase(new[] { 4, 2, 3 })]
    public async Task Generator_Generates_Siteswap(int[] expected)
    {
        var input = Input(3, 5, 0, 3);

        var generator = CreateTestObject(input);


        var result = (await generator.GenerateAsync()).ToList();
        result.Should().Contain(expected);
    }

    private SiteswapGeneratorInput Input(int period, int maxHeight, int minHeight, int numberOfObjects)
    {
        return new SiteswapGeneratorInput
        {
            Period = period,
            MaxHeight = maxHeight,
            MinHeight = minHeight,
            NumberOfObjects = numberOfObjects,
            StopCriteria = new (TimeSpan.FromSeconds(15),10000 )
        };
    }

    [Test]
    [TestCase(5, 10, 2, 7, ExpectedResult = 110)]
    [TestCase(5, 10, 2, 6, ExpectedResult = 152)]
    [TestCase(7, 13, 2, 8, ExpectedResult = 8946)]
    [TestCase(3, 13, 0, 8, ExpectedResult = 28)]
    [TestCase(3, 10, 0, 5, ExpectedResult = 20)]
    [TestCase(5, 5, 0, 3, ExpectedResult = 26)]
    public async Task<int> Number_Of_Siteswaps_Should_Be_Correct(int period, int maxHeight, int minHeight, int numberOfObjects)
    {
        var input = Input(period, maxHeight, minHeight, numberOfObjects);
        var generator = CreateTestObject(input);

        var siteswaps = (await generator.GenerateAsync()).ToList();
        return siteswaps.Count;
    }

    [TestCase(7, 13, 2, 8, "result.txt")]
    public async Task Siteswaps_Should_Be_In_Result_List(int period, int maxHeight, int minHeight, int numberOfObjects, string resultFilename)
    {
        var input = Input(period, maxHeight, minHeight, numberOfObjects);
        var generator = CreateTestObject(input);

        var siteswaps = (await generator.GenerateAsync()).ToList();

        var siteswapsAsStrings = siteswaps.Select(x => x.ToString()).ToList();
        
        var deserialize = JsonSerializer.Deserialize<Result>(await File.ReadAllTextAsync(resultFilename)) ?? throw new FileNotFoundException(nameof(resultFilename));

        siteswapsAsStrings.Should().OnlyContain(x => deserialize.List.Contains(x));
        
        deserialize.List.Should().OnlyContain(x => siteswapsAsStrings.Contains(x));
    }
}

public class Result
{
    public List<string> List { get; set; } = new();
}
