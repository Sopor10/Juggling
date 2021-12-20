using NUnit.Framework;

namespace Siteswaps.Generator.Test;

public class PartialSiteswapTest
{
    [Test]
    [TestCase(new[]{5,-1,-1}, ExpectedResult = 5)]
    [TestCase(new[]{5,5,-1}, ExpectedResult = 4)]
    [TestCase(new[]{5,4,-1}, ExpectedResult = 4)]
    [TestCase(new[]{5,2,-1}, ExpectedResult = 4)]
    [TestCase(new[]{5,3,-1}, ExpectedResult = 4)]
    [TestCase(new[]{4,3,-1}, ExpectedResult = 3)]
    [TestCase(new[]{4,2,4,2,-1}, ExpectedResult = 3)]
    [TestCase(new[]{4,2,-1,-1}, ExpectedResult = 4)]
    [TestCase(new[]{5,1,4,0,-1}, ExpectedResult = 4)]
    [TestCase(new[]{5,0,-1,-1,-1}, ExpectedResult = 5)]
    [TestCase(new[]{5,0,2,-1,-1, - 1}, ExpectedResult = 5)]
    [TestCase(new[]{5,0,2,5,-1, - 1}, ExpectedResult = 0)]
    public int Calculation_Of_Next_Max_Value_Is_Correct(int[] values) => new PartialSiteswap(values).MaxForNextFree();
    
    [Ignore("future optimization")]
    [TestCase(new[]{5,0,2,-1,-1}, ExpectedResult = 4)]
    public int Calculation_Of_Next_Max_Value_Is_Correct_Ignored(int[] values) => new PartialSiteswap(values).MaxForNextFree();

}