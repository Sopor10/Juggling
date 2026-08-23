using FluentAssertions;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;
using Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

namespace Siteswaps.Generator.Test;

public class GeneratorFilterBoundaryTests
{
    [Test]
    public void Exact_Number_Of_Passes_Accepts_A_Partial_Siteswap_Within_The_Limit()
    {
        var sut = new FilterBuilder(new SiteswapGeneratorInput(3, 3, 0, 10))
            .ExactNumberOfPasses(1, 2)
            .Build();

        sut.CanFulfill(new PartialSiteswap(new[] { 1, -1, -1 })).Should().BeTrue();
    }

    [Test]
    public void Exact_Number_Of_Passes_Rejects_A_Partial_Siteswap_Over_The_Limit()
    {
        var sut = new FilterBuilder(new SiteswapGeneratorInput(3, 3, 0, 10))
            .ExactNumberOfPasses(1, 2)
            .Build();

        sut.CanFulfill(new PartialSiteswap(new[] { 1, 3, -1 })).Should().BeFalse();
    }

    [Test]
    public void Exact_Number_Of_Passes_Accepts_A_Full_Siteswap_At_The_Limit()
    {
        var sut = new FilterBuilder(new SiteswapGeneratorInput(3, 3, 0, 10))
            .ExactNumberOfPasses(1, 2)
            .Build();

        sut.CanFulfill(new PartialSiteswap(new[] { 1, 2, 2 })).Should().BeTrue();
    }

    [Test]
    public void Exact_Number_Of_Passes_Rejects_A_Full_Siteswap_Over_The_Limit()
    {
        var sut = new FilterBuilder(new SiteswapGeneratorInput(3, 3, 0, 10))
            .ExactNumberOfPasses(1, 2)
            .Build();

        sut.CanFulfill(new PartialSiteswap(new[] { 1, 3, 3 })).Should().BeFalse();
    }

    [Test]
    public void Flexible_Pattern_Accepts_The_Expected_Pass_And_Self_Throws()
    {
        var sut = new FilterBuilder(new SiteswapGeneratorInput(2, 3, 1, 4))
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
    }

    [Test]
    public void Flexible_Pattern_Handles_Throws_Above_The_NumberMask_Range()
    {
        var sut = new FilterBuilder(new SiteswapGeneratorInput(2, 3, 1, 70))
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

        sut.CanFulfill(new PartialSiteswap(new[] { 65, 66 })).Should().BeTrue();
    }

    [Test]
    public void Flexible_Pattern_Rejects_An_Unexpected_Self_Throw()
    {
        var sut = new FilterBuilder(new SiteswapGeneratorInput(2, 3, 1, 4))
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

        sut.CanFulfill(new PartialSiteswap(new[] { 2, 2 })).Should().BeFalse();
    }

    [Test]
    public void Rotation_Aware_Flexible_Pattern_Accepts_The_Juggler_Position()
    {
        var sut = new RotationAwareFlexiblePatternFilter(
            new List<List<int>>
            {
                new() { -2 },
                new() { -3 },
            },
            2,
            new SiteswapGeneratorInput(4, 3, 1, 4),
            0
        );

        sut.CanFulfill(new PartialSiteswap(new[] { 1, 0, 2, 0 })).Should().BeTrue();
    }

    [Test]
    public void Rotation_Aware_Flexible_Pattern_Rejects_The_Wrong_Juggler_Position()
    {
        var sut = new RotationAwareFlexiblePatternFilter(
            new List<List<int>>
            {
                new() { -2 },
                new() { -3 },
            },
            2,
            new SiteswapGeneratorInput(4, 3, 1, 4),
            0
        );

        sut.CanFulfill(new PartialSiteswap(new[] { 2, 0, 2, 0 })).Should().BeFalse();
    }

    [Test]
    public void Personalized_Number_Filter_Counts_An_Empty_Throw_As_Possible()
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
    }

    [Test]
    public void Personalized_Number_Filter_Rejects_Insufficient_Throws()
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
