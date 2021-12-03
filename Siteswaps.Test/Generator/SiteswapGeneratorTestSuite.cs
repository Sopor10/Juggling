using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator;
using Siteswaps.Generator.Filter;

namespace Siteswaps.Test.Generator
{
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
            return new SiteswapGeneratorInput()
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

                var input = Input(3,5,0,3);

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

            var input = Input(5,5,0,3);

            var result = generator.Generate(input).ToList();
            result.Should().Contain(expected);
        }

        [Test]
        public void Standard_Filter_Should_Filter_Out_No_Valid_Siteswap()
        {
            var generator = CreateTestObject();
            var input = Input(5,5,0,3);

            var siteswaps = generator.Generate(input).Where(x => x.NumberOfObjects() == input.NumberOfObjects).ToList();
            siteswaps.Should().HaveCount(26);
        }
    }
}