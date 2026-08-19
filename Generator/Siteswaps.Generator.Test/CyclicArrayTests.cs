using FluentAssertions;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test;

public class CyclicArrayTests
{
    [Test]
    public void Indexing_Uses_Cyclic_Order()
    {
        var sut = new CyclicArray<int>(new[] { 1, 2, 3 });

        sut[0].Should().Be(1);
        sut[3].Should().Be(1);
    }

    [Test]
    public void EnumerateValues_Repeats_Cyclic_Order()
    {
        var sut = new CyclicArray<int>(new[] { 1, 2, 3 });

        sut.EnumerateValues(2).Should().Equal(1, 2, 3, 1, 2, 3);
    }

    [Test]
    public void Rotate_Returns_The_Same_Array_With_Updated_Order()
    {
        var sut = new CyclicArray<int>(new[] { 1, 2, 3 });

        sut.Rotate(1).Should().BeSameAs(sut);
        sut.Should().Equal(2, 3, 1);
    }

    [Test]
    public void Indexer_Set_Updates_The_Rotated_Position()
    {
        var sut = new CyclicArray<int>(new[] { 1, 2, 3 }, rotationIndex: 1);

        sut[0] = 9;

        sut.Should().Equal(9, 3, 1);
    }

    [Test]
    public void Enumerate_Reports_Position_And_Value()
    {
        var sut = new CyclicArray<int>(new[] { 4, 5, 6 }, rotationIndex: 1);

        sut.Enumerate(2).Should().Equal((0, 5), (1, 6), (2, 4), (3, 5), (4, 6), (5, 4));
    }

    [Test]
    public void AsSpan_Returns_Original_Storage_Without_Rotation()
    {
        var sut = new CyclicArray<int>(new[] { 1, 2, 3 });

        sut.AsSpan().ToArray().Should().Equal(1, 2, 3);
    }

    [Test]
    public void AsSpan_Returns_Rotated_Storage()
    {
        var sut = new CyclicArray<int>(new[] { 1, 2, 3 }, rotationIndex: 2);

        sut.AsSpan().ToArray().Should().Equal(3, 1, 2);
    }

    [Test]
    public void Enumerator_Can_Be_Reset()
    {
        var enumerator = new CyclicArray<int>(new[] { 7, 8 }).GetEnumerator();

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Should().Be(7);
        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Should().Be(8);
        enumerator.MoveNext().Should().BeFalse();

        enumerator.Reset();
        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Should().Be(7);
    }
}
