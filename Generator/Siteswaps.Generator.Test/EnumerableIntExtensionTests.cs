using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using FluentAssertions;
using NUnit.Framework;

namespace Siteswaps.Generator.Test;

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
    [TestCase(new[]{4}, new[]{4,2})]
    [TestCase(new[]{5,3}, new[]{4,2})]
    [TestCase(new[]{4,2}, new[]{4,2,3})]
    [TestCase(new[]{4,2,4}, new[]{4,2})]
    public void Compare_2_Sequences_First_Sequence_Is_Bigger(int[] arr1, int[] arr2)
    {
        arr1.CompareSequences(arr2).Should().Be(1);
    }

    [Test]
    [TestCase(new[]{5,0}, new[]{5,5})]
    [TestCase(new[]{4,2}, new[]{4})]
    [TestCase(new[]{4,2}, new[]{4,2,4})]
    [TestCase(new[]{4,2,3}, new[]{4,2})]
    [TestCase(new[]{4,2}, new[]{4,2, 4})]
    public void Compare_2_Sequences_Second_Sequence_Is_Bigger(int[] arr1, int[] arr2)
    {
        arr1.CompareSequences(arr2).Should().Be(-1);
    }

    [Test]
    [Repeat(100)]
    public void Compare_2_Sequences_Is_Symmetrical()
    {
        var value = new Random().Next();
        Randomizer.Seed = new Random(value);
        var faker = new Faker();
        var len = faker.Random.Int(1, 30);
        var array1 = Enumerable.Range(0, len).Select(x => faker.Random.Int()).ToArray();
        var array2 = Enumerable.Range(0, len).Select(x => faker.Random.Int()).ToArray();

        array1.CompareSequences(array2).Should().NotBe(array2.CompareSequences(array1), $"The seed for this failed run is {value}");
    }
    [Test]
    [Repeat(100)]
    public void Compare_2_Sequences_Throws_If_Both_Sequeces_Are_Empty()
    {
        var array1 = new int[]{};
        var array2 = new int[]{};

        Action sut = () =>array1.CompareSequences(array2);
        sut.Should().Throw<InvalidOperationException>();
    }
}