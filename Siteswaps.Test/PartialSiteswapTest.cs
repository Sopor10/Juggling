using NUnit.Framework;
using Siteswaps.Generator;

namespace Siteswaps.Test
{
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
        public int Calculation_Of_Next_Max_Value_Is_Correct(int[] values)
        {
            var sut = new PartialSiteswap(values);
            return sut.MaxForNextFree();
        }
    }
}