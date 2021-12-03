using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator;
using Siteswaps.Generator.Filter;

namespace Siteswaps.Test.Generator;

public abstract class SiteswapGeneratorTestSuite
{
    protected abstract ISiteswapGenerator CreateTestObject();

    [Test]
    [TestCase(new[] { 5, 3, 1 })]
    [TestCase(new[] { 4, 2, 3 })]
    public void Generator_Generates_Siteswap(int[] expected)
    {
        var generator = CreateTestObject();

        var input = Input(3, 5, 0, 3);

        var result = generator.Generate(input).ToList();
        result.Should().Contain(expected);
    }

    private SiteswapGeneratorInput Input(int period, int maxHeight, int minHeight, int numberOfObjects)
    {
        return new SiteswapGeneratorInput
        {
            Period = period,
            Filter = new NoFilter(),
            MaxHeight = maxHeight,
            MinHeight = minHeight,
            NumberOfObjects = numberOfObjects
        };
    }

    [Test]
    [TestCase(new[] { 3, 3, 3 })]
    public void Generator_Should_Not_Generate_Siteswap(int[] expected)
    {
        if (Siteswap.TryCreate(expected, out var expectedSiteswap))
        {
            var generator = CreateTestObject();

            var input = Input(3, 5, 0, 3);

            var result = generator.Generate(input).ToList();
            result.Should().NotContain(expectedSiteswap);
        }
        else
        {
            Assert.Fail("Given Array is no valid Siteswap");
        }
    }

    [Test]
    [TestCase(new[] { 4, 2, 4, 2, 3 })]
    public void Generator_Generates_Longer_Siteswap(int[] expected)
    {
        var generator = CreateTestObject();

        var input = Input(5, 5, 0, 3);

        var result = generator.Generate(input).ToList();
        result.Should().Contain(expected);
    }

    [Test]
    public void Standard_Filter_Should_Filter_Out_No_Valid_Siteswap()
    {
        var generator = CreateTestObject();
        var input = Input(5, 5, 0, 3);

        var siteswaps = generator.Generate(input).Where(x => x.NumberOfObjects() == input.NumberOfObjects).ToList();
        siteswaps.Should().HaveCount(26);
    }

    [Test]
    [TestCase(5, 10, 2, 7, ExpectedResult = 110)]
    [TestCase(5, 10, 2, 6, ExpectedResult = 152)]
    [TestCase(7, 13, 2, 8, ExpectedResult = 8946)]
    [TestCase(3, 13, 0, 8, ExpectedResult = 28)]
    [TestCase(3, 10, 0, 5, ExpectedResult = 20)]
    public int Number_Of_Siteswaps_Should_Be_Correct(int period, int maxHeight, int minHeight, int numberOfObjects)
    {
        var generator = CreateTestObject();
        var input = Input(period, maxHeight, minHeight, numberOfObjects);

        var siteswaps = generator.Generate(input).Where(x => x.NumberOfObjects() == input.NumberOfObjects).ToList();
        return siteswaps.Count();
    }

    [TestCase(7, 13, 2, 8)]
    public void Siteswaps_Should_Be_In_Result_List(int period, int maxHeight, int minHeight, int numberOfObjects)
    {
        var generator = CreateTestObject();
        var input = Input(period, maxHeight, minHeight, numberOfObjects);

        var siteswaps = generator.Generate(input).Where(x => x.NumberOfObjects() == input.NumberOfObjects).ToList();

        var siteswapsAsStrings = siteswaps.Select(x => x.ToString()).ToList();
        
        var deserialize = JsonSerializer.Deserialize<Result>(File.ReadAllText("result.json"));

        // all generated siteswaps are correct
        foreach (var value in siteswapsAsStrings)
        {
            deserialize.List.Should().Contain(value, "I generated this siteswap");
        }

        // all correct siteswaps are generated
        foreach (var value in deserialize.List)
        {
            siteswapsAsStrings.Should().Contain(value, "this siteswap should be generated");
        }
    }
}

public class Result
{
    public List<string> List { get; set; }
}