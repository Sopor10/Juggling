using FluentAssertions;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test;

public class EnumerableIntExtensionTests
{
    [TestCase(new[] { 1, 2 }, new[] { 1, 3 }, -1)]
    [TestCase(new[] { 1, 3 }, new[] { 1, 2 }, 1)]
    [TestCase(new[] { 1, 2 }, new[] { 1, 2 }, 1)]
    public void CompareSequences_Compares_The_First_Different_Value(
        int[] first,
        int[] second,
        int expected
    )
    {
        first.CompareSequences(second).Should().Be(expected);
    }

    [TestCase(new[] { 5 }, new[] { 5, 3 }, 1)]
    [TestCase(new[] { 5 }, new[] { 5, 7 }, -1)]
    [TestCase(new[] { 1, 0 }, new[] { 1 }, -1)]
    [TestCase(new[] { 5 }, new[] { 5, 5 }, -1)]
    [TestCase(new[] { 5, 5 }, new[] { 5 }, 1)]
    public void CompareSequences_Compares_Sequence_Length(int[] first, int[] second, int expected)
    {
        first.CompareSequences(second).Should().Be(expected);
    }

    [Test]
    public void CompareSequences_Uses_The_Maximum_When_The_First_Sequence_Is_Shorter()
    {
        new[] { 5, 1 }.CompareSequences(new[] { 5, 1, 3 }).Should().Be(1);
    }

    [Test]
    public void CompareSequences_Uses_The_Maximum_When_The_Second_Sequence_Is_Shorter()
    {
        new[] { 5, 1, 3 }.CompareSequences(new[] { 5, 1 }).Should().Be(-1);
    }

    [Test]
    public void CompareSequences_Rejects_Two_Empty_Sequences_With_A_Useful_Message()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            Array.Empty<int>().CompareSequences(Array.Empty<int>())
        );

        exception!.Message.Should().Contain("sequence must be non empty");
    }

    [Test]
    public void AbsteigendeSeq_Splits_When_The_Sequence_Rises()
    {
        var result = new[] { 1, 2, 1, 3, 3, 2 }.AbsteigendeSeq().Select(x => x.ToArray());

        result
            .Should()
            .BeEquivalentTo(
                new[] { new[] { 1 }, new[] { 2, 1 }, new[] { 3, 3, 2 } },
                options => options.WithStrictOrdering()
            );
    }

    [Test]
    public void AbsteigendeSeq_Returns_No_Groups_For_Empty_Input()
    {
        Array.Empty<int>().AbsteigendeSeq().Should().BeEmpty();
    }
}
