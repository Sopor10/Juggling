using Siteswaps.Generator.Generator;
using Siteswaps.Generator.Generator.Filter.NumberFilter;

namespace Siteswaps.Generator.Test.Filter;

public partial class FilterTestSuite
{
    [Test]
    [TestCase(
        new[] { 8, 4, 8, 4 },
        0,
        PersonalizedNumberFilter.Type.Exact,
        Juggler.A,
        ExpectedResult = true
    )]
    [TestCase(
        new[] { 8, 4, 8, 4 },
        0,
        PersonalizedNumberFilter.Type.Exact,
        Juggler.B,
        ExpectedResult = false
    )]
    [TestCase(
        new[] { 8, 4, 8, 4 },
        2,
        PersonalizedNumberFilter.Type.AtMost,
        Juggler.B,
        ExpectedResult = true
    )]
    [TestCase(
        new[] { 8, 4, 8, 4 },
        2,
        PersonalizedNumberFilter.Type.AtLeast,
        Juggler.B,
        ExpectedResult = true
    )]
    [TestCase(
        new[] { 8, 4, 8, -1 },
        2,
        PersonalizedNumberFilter.Type.AtLeast,
        Juggler.B,
        ExpectedResult = true
    )]
    [TestCase(
        new[] { 8, 4, -1, -1 },
        0,
        PersonalizedNumberFilter.Type.Exact,
        Juggler.A,
        ExpectedResult = true
    )]
    [TestCase(
        new[] { 8, 4, -1, -1 },
        0,
        PersonalizedNumberFilter.Type.AtMost,
        Juggler.A,
        ExpectedResult = true
    )]
    [TestCase(
        new[] { 8, 4, -1, -1 },
        0,
        PersonalizedNumberFilter.Type.AtLeast,
        Juggler.A,
        ExpectedResult = true
    )]
    [TestCase(
        new[] { 7, 5, 3, 1 },
        0,
        PersonalizedNumberFilter.Type.Exact,
        Juggler.A,
        ExpectedResult = true
    )]
    [TestCase(
        new[] { 7, 5, 3, 1 },
        0,
        PersonalizedNumberFilter.Type.Exact,
        Juggler.B,
        ExpectedResult = true
    )]
    public bool No_4_For_A(
        int[] input,
        int amount,
        PersonalizedNumberFilter.Type type,
        Juggler juggler
    )
    {
        return new PersonalizedNumberFilter(2, 2, 10, [4], amount, type, (int)juggler).CanFulfill(
            new PartialSiteswap(input)
        );
    }

    public enum Juggler
    {
        A = 0,
        B = 1,
        C = 2,
    }
}
