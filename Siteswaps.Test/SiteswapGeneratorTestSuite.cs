using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator;
using Siteswaps.Generator.Filter;

namespace Siteswaps.Test
{
    public abstract class SiteswapGeneratorTestSuite
    {
        protected abstract ISiteswapGenerator CreateTestObject();
        
        [Test]
        [TestCase(new[]{3,3,3})]
        [TestCase(new[]{5,3,1})]
        [TestCase(new[]{4,2,3})]
        public void Generator_Generates_Siteswap(int[] expected)
        {
            var generator = CreateTestObject();

            var input = new SiteswapGeneratorInput(3, 3, 0, 5, new NoFilter());

            var result = generator.Generate(input).ToList();
            result.Should().Contain(expected);
        }
        
        [Test]
        [TestCase(new[]{4,2,4,2,3})]
        public void Generator_Generates_Longer_Siteswap(int[] expected)
        {
            var generator = CreateTestObject();

            var input = new SiteswapGeneratorInput(3, 5, 0, 5, new NoFilter());

            var result = generator.Generate(input).ToList();
            result.Should().Contain(expected);
        }
    }
}