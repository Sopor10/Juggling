using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator;

namespace Siteswaps.Test
{
    public class EnumerableIntExtensionTests
    {
        [Test]
        public void AbsteigendeSequenzen_Are_Identified_Correctly()
        {
            var sut = new List<int>() { 9, 7, 9, 7, 8 };
            var result = new List<List<int>>()
            {
                new() { 9, 7 },
                new() { 9, 7 },
                new() { 8 }
            };
            sut.AbsteigendeSeq().Should().BeEquivalentTo(result);
        }
        
        [Test]
        public void AbsteigendeSequenzen_Respect_Multiple_Ocurences_Of_The_Same_Number()
        {
            var sut = new List<int>() { 9, 9, 8, 9, 8 };
            var result = new List<List<int>>()
            {
                new() { 9, 9,8 },
                new() { 9, 8 },
            };
            sut.AbsteigendeSeq().Should().BeEquivalentTo(result);
        }
        
        [Test]
        [TestCase(new[]{4}, new[]{4,2},ExpectedResult = 1)]
        [TestCase(new[]{4,2}, new[]{4},ExpectedResult = -1)]
        [TestCase(new[]{5,3}, new[]{4,2},ExpectedResult = 1)]
        [TestCase(new[]{4,2}, new[]{4,2,4},ExpectedResult = -1)]
        [TestCase(new[]{4,2}, new[]{4,2,3},ExpectedResult = 1)]
        [TestCase(new[]{4,2,3}, new[]{4,2},ExpectedResult = -1)]
        [TestCase(new[]{4,2,4}, new[]{4,2},ExpectedResult = 1)]
        [TestCase(new[]{4,2}, new[]{4,2, 4},ExpectedResult = -1)]
        public int Compare_2_Sequences_Works_As_Expected(int[] arr1, int[] arr2) => arr1.CompareSequences(arr2);

        [Test]
        public void Compare_2_Sequences_Is_Symmetrical()
        {
            Assert.Fail();//Todo
        }
    }
}