using FluentAssertions;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;
using Siteswaps.Generator.Core.Generator.Filter.Combinatorics;
using Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

namespace Siteswaps.Generator.Test;

public class EnumerableIntExtensionTests
{
    [Test]
    public void CompareSequences_Rejects_Two_Empty_Sequences()
    {
        var act = () => Array.Empty<int>().CompareSequences(Array.Empty<int>());

        act.Should().Throw<InvalidOperationException>();
    }

    [TestCase(new[] { 1, 2 }, new[] { 1, 3 }, -1)]
    [TestCase(new[] { 1, 3 }, new[] { 1, 2 }, 1)]
    [TestCase(new[] { 1, 2 }, new[] { 1, 2 }, 1)]
    [TestCase(new[] { 5 }, new[] { 5, 3 }, 1)]
    [TestCase(new[] { 5 }, new[] { 5, 7 }, -1)]
    [TestCase(new[] { 1, 0 }, new[] { 1 }, -1)]
    public void CompareSequences_Uses_First_Different_Value_And_Length(
        int[] first,
        int[] second,
        int expected
    )
    {
        first.CompareSequences(second).Should().Be(expected);
    }

    [Test]
    public void CompareSequences_Handles_Equal_Extensions_And_Exact_Error_Message()
    {
        new[] { 5 }.CompareSequences(new[] { 5, 5 }).Should().Be(-1);
        new[] { 5, 5 }.CompareSequences(new[] { 5 }).Should().Be(1);
        new[] { 5, 1 }.CompareSequences(new[] { 5, 1, 3 }).Should().Be(1);
        new[] { 5, 1, 3 }.CompareSequences(new[] { 5, 1 }).Should().Be(-1);

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

public class CyclicArrayTests
{
    [Test]
    public void Indexing_Enumeration_And_Rotation_Use_Cyclic_Order()
    {
        var sut = new CyclicArray<int>(new[] { 1, 2, 3 });

        sut[0].Should().Be(1);
        sut[3].Should().Be(1);
        sut.EnumerateValues(2).Should().Equal(1, 2, 3, 1, 2, 3);

        sut.Rotate(1).Should().BeSameAs(sut);
        sut[0].Should().Be(2);
        sut.Should().Equal(2, 3, 1);

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
    public void AsSpan_Returns_Original_Or_Rotated_Storage()
    {
        var original = new CyclicArray<int>(new[] { 1, 2, 3 });
        original.AsSpan().ToArray().Should().Equal(1, 2, 3);

        var rotated = new CyclicArray<int>(new[] { 1, 2, 3 }, rotationIndex: 1);
        rotated.AsSpan().ToArray().Should().Equal(2, 3, 1);

        new CyclicArray<int>(new[] { 1, 2, 3 }, rotationIndex: 2)
            .AsSpan()
            .ToArray()
            .Should()
            .Equal(3, 1, 2);
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

public class SiteswapCoreTests
{
    [Test]
    public void String_Notation_Parses_And_Renders_Extended_Heights()
    {
        var sut = Siteswap.CreateFromCorrect("a72");

        sut.Items.Should().Equal(10, 7, 2);
        sut.ToString().Should().Be("a72");
        sut.Period.Value.Should().Be(3);
    }

    [Test]
    public void Siteswaps_Compare_By_Notation()
    {
        var first = Siteswap.CreateFromCorrect(10, 7, 2);
        var same = Siteswap.CreateFromCorrect("a72");
        var different = Siteswap.CreateFromCorrect(10, 7, 3);

        first.Equals(same).Should().BeTrue();
        first.Equals(different).Should().BeFalse();
        first.Equals((Siteswap?)null).Should().BeFalse();
    }

    [Test]
    public void Siteswap_Exposes_Average_And_Validity()
    {
        Siteswap.CreateFromCorrect(5, 3, 1).Average.Should().BeApproximately(3, 0.001);
        Siteswap.CreateFromCorrect(5, 3, 1).IsValid().Should().BeTrue();
        Siteswap.CreateFromCorrect(3, 2).IsValid().Should().BeFalse();
        Siteswap.CreateFromCorrect(1, 3, 0).IsValid().Should().BeFalse();
    }

    [Test]
    public void LocalSiteswap_Exposes_Notation_Average_And_Validity()
    {
        var local = Siteswap.CreateFromCorrect(5, 3, 1).GetLocalSiteswap(0, 2);

        local.GlobalNotation.Should().Be("513");
        local.LocalNotation.Should().Be("2.5 0.5 1.5");
        local.Average().Should().BeApproximately(1.5, 0.001);
        local.IsValidAsGlobalSiteswap().Should().BeFalse();

        Siteswap
            .CreateFromCorrect(5, 3, 1)
            .GetLocalSiteswap(0, 1)
            .IsValidAsGlobalSiteswap()
            .Should()
            .BeTrue();
    }
}

public class FilterCombinatoricsTests
{
    [Test]
    public void AndFilter_Requires_All_Filters_And_Propagates_Rotation_Awareness()
    {
        var value = new PartialSiteswap(new[] { 1 });
        var sut = new AndFilter(
            new RecordingFilter(true, isRotationAware: false),
            new RecordingFilter(false, isRotationAware: true)
        );

        sut.CanFulfill(value).Should().BeFalse();
        sut.IsRotationAware.Should().BeTrue();
        new AndFilter().CanFulfill(value).Should().BeTrue();
    }

    [Test]
    public void OrFilter_Accepts_One_Matching_Filter()
    {
        var value = new PartialSiteswap(new[] { 1 });

        new OrFilter(new RecordingFilter(false), new RecordingFilter(true))
            .CanFulfill(value)
            .Should()
            .BeTrue();
        new OrFilter(new RecordingFilter(false), new RecordingFilter(false))
            .CanFulfill(value)
            .Should()
            .BeFalse();
        new OrFilter(new RecordingFilter(false, isRotationAware: true), new RecordingFilter(false))
            .IsRotationAware.Should()
            .BeTrue();
    }

    [Test]
    public void NotFilter_Is_Inactive_For_Partial_Values_And_Inverts_Full_Values()
    {
        var filter = new RecordingFilter(false, isRotationAware: true);
        var sut = new NotFilter(filter);

        sut.CanFulfill(new PartialSiteswap(new[] { -1 })).Should().BeTrue();
        sut.CanFulfill(new PartialSiteswap(new[] { 1 })).Should().BeTrue();
        sut.IsRotationAware.Should().BeTrue();

        new NotFilter(new RecordingFilter(true))
            .CanFulfill(new PartialSiteswap(new[] { 1 }))
            .Should()
            .BeFalse();
    }

    private sealed class RecordingFilter(bool result, bool isRotationAware = false)
        : ISiteswapFilter
    {
        public bool CanFulfill(PartialSiteswap value) => result;

        public bool IsRotationAware => isRotationAware;
    }
}

public class StateTests
{
    [Test]
    public void State_Uses_Bit_Positions_In_Its_String_Representation()
    {
        new State(0, 1, 0, 1).ToString().Should().Be("0101");
        State.GroundState(3).ToString().Should().Be("111");
    }

    [TestCase(new[] { 3, 3, 3 }, 3, "111")]
    [TestCase(new[] { 4, 4, 4 }, 4, "1111")]
    public void CalculateState_Reaches_A_Stable_State(
        int[] siteswap,
        int maxHeight,
        string expected
    )
    {
        State
            .CalculateState(new PartialSiteswap(siteswap), maxHeight)
            .ToString()
            .Should()
            .Be(expected);
    }

    [Test]
    public void State_And_Number_Filters_Advertise_Rotation_Awareness()
    {
        var input = new SiteswapGeneratorInput(3, 3, 0, 10);

        new FilterBuilder(input)
            .WithState(State.GroundState(3))
            .Build()
            .IsRotationAware.Should()
            .BeTrue();
        new FilterBuilder(input)
            .MinimumOccurence(new[] { 1 }, 1)
            .Build()
            .IsRotationAware.Should()
            .BeTrue();
    }
}

public class GeneratorFilterBoundaryTests
{
    [Test]
    public void Exact_Number_Of_Passes_Handles_Partial_And_Full_Siteswaps()
    {
        var input = new SiteswapGeneratorInput(3, 3, 0, 10);
        var sut = new FilterBuilder(input).ExactNumberOfPasses(1, 2).Build();

        sut.CanFulfill(new PartialSiteswap(new[] { 1, -1, -1 })).Should().BeTrue();
        sut.CanFulfill(new PartialSiteswap(new[] { 1, 3, -1 })).Should().BeFalse();
        sut.CanFulfill(new PartialSiteswap(new[] { 1, 2, 2 })).Should().BeTrue();
        sut.CanFulfill(new PartialSiteswap(new[] { 1, 3, 3 })).Should().BeFalse();
    }

    [Test]
    public void Flexible_Pattern_Distinguishes_Pass_And_Self_Throws()
    {
        var input = new SiteswapGeneratorInput(2, 3, 1, 4);
        var sut = new FilterBuilder(input)
            .FlexiblePattern(
                new List<List<int>>
                {
                    new() { -2 },
                    new() { -3 },
                },
                2,
                true
            )
            .Build();

        sut.CanFulfill(new PartialSiteswap(new[] { 1, 2 })).Should().BeTrue();
        sut.CanFulfill(new PartialSiteswap(new[] { 2, 2 })).Should().BeFalse();
    }

    [Test]
    public void Rotation_Aware_Flexible_Pattern_Uses_Juggler_Position()
    {
        var input = new SiteswapGeneratorInput(4, 3, 1, 4);
        var sut = new RotationAwareFlexiblePatternFilter(
            new List<List<int>>
            {
                new() { -2 },
                new() { -3 },
            },
            2,
            input,
            0
        );

        sut.CanFulfill(new PartialSiteswap(new[] { 1, 0, 2, 0 })).Should().BeTrue();
        sut.CanFulfill(new PartialSiteswap(new[] { 2, 0, 2, 0 })).Should().BeFalse();
    }

    [Test]
    public void Personalized_Number_Filter_Counts_Empty_Throws_For_The_Selected_Juggler()
    {
        var sut = new PersonalizedNumberFilter(
            2,
            0,
            6,
            new[] { 2 },
            1,
            PersonalizedNumberFilter.Type.AtLeast,
            0
        );

        sut.CanFulfill(new PartialSiteswap(new[] { -1, 0, 2, 0 })).Should().BeTrue();
        sut.CanFulfill(new PartialSiteswap(new[] { 0, 0, 0, 0 })).Should().BeFalse();
    }

    [Test]
    public async Task Generator_Stops_At_The_Configured_Result_Limit()
    {
        var input = new SiteswapGeneratorInput(3, 3, 0, 5)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(10), 1),
        };

        var results = await new SiteswapGenerator(input)
            .GenerateAsync(CancellationToken.None)
            .ToListAsync();

        results.Should().HaveCount(1);
    }
}
