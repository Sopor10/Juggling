using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Siteswaps.Test
{
    public class CyclicArrayTest
    {
        [Test]
        [TestCase(0,0)]
        [TestCase(6,1)]
        [TestCase(5,0)]
        public void Indexer_Access_Test(int place, int expected)
        {
            var sut = new CyclicArray<int>(Enumerable.Range(0,5));

            sut[place].Should().Be(expected);
        }
        
        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Rotate_By(int i)
        {
            var sut = new CyclicArray<int>(Enumerable.Range(0,5));

            sut.Rotate(i)[0].Should().Be(i);
        }
    }
}