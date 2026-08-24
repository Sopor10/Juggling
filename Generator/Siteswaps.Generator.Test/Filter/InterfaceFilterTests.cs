using FluentAssertions;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;

namespace Siteswaps.Generator.Test.Filter;

/// <summary>
/// The interface filter constrains where throws land; the pattern filters constrain which throw
/// is made on a beat. For 744 / 474 the two views disagree, which pins the distinction.
/// </summary>
[TestFixture]
public class InterfaceFilterTests
{
    private const int Pass = -2;
    private const int Self = -3;

    private static readonly SiteswapGeneratorInput Period3 = new()
    {
        Period = 3,
        MinHeight = 2,
        MaxHeight = 9,
        NumberOfObjects = 5,
    };

    /// <summary>Landing mask S,P,S: the pass must arrive on beat 1.</summary>
    private static List<List<int>> PassLandsOnBeat1 =>
        [
            [Self],
            [Pass],
            [Self],
        ];

    [Test]
    public void Accepts_Siteswap_Whose_Pass_Lands_On_The_Masked_Beat()
    {
        // 744: the 7 is thrown on beat 0 and lands on beat 1.
        var filter = new InterfaceFilter(PassLandsOnBeat1, 2, Period3, allowRotation: false);

        filter.CanFulfill(Filled(7, 4, 4)).Should().BeTrue();
    }

    [Test]
    public void Rejects_Siteswap_Whose_Pass_Is_Thrown_On_The_Masked_Beat_But_Lands_Elsewhere()
    {
        // 474: the 7 is thrown on beat 1 (what a pattern filter looks at) but lands on beat 2.
        var filter = new InterfaceFilter(PassLandsOnBeat1, 2, Period3, allowRotation: false);

        filter.CanFulfill(Filled(4, 7, 4)).Should().BeFalse();
    }

    [Test]
    public void Pattern_Filter_On_The_Same_Mask_Accepts_The_Opposite_Siteswap()
    {
        // Guards the abstraction choice: swapping the filter type changes which siteswaps survive.
        var patternFilter = new AbsoluteFlexiblePatternFilter(PassLandsOnBeat1, 2, Period3);

        patternFilter.CanFulfill(Filled(4, 7, 4)).Should().BeTrue();
        patternFilter.CanFulfill(Filled(7, 4, 4)).Should().BeFalse();
    }

    [Test]
    public void AllowRotation_Accepts_Every_Cyclic_Rotation_Of_The_Landing_Mask()
    {
        var filter = new InterfaceFilter(PassLandsOnBeat1, 2, Period3, allowRotation: true);

        filter.CanFulfill(Filled(7, 4, 4)).Should().BeTrue();
        filter.CanFulfill(Filled(4, 7, 4)).Should().BeTrue();
        filter.CanFulfill(Filled(4, 4, 7)).Should().BeTrue();
    }

    [Test]
    public void Rejects_A_Partial_Siteswap_As_Soon_As_A_Landing_Slot_Contradicts_The_Mask()
    {
        // A 4 thrown on beat 0 already occupies landing slot 1, which the mask reserves for a pass.
        var filter = new InterfaceFilter(PassLandsOnBeat1, 2, Period3, allowRotation: false);
        var partial = new PartialSiteswap([-1, -1, -1]);
        partial.FillCurrentPosition(4);

        partial.IsFilled().Should().BeFalse();
        filter.CanFulfill(partial).Should().BeFalse();
    }

    [Test]
    public void Generated_Siteswaps_All_Land_Their_Passes_On_The_Masked_Beats()
    {
        var input = new SiteswapGeneratorInput
        {
            Period = 5,
            MinHeight = 2,
            MaxHeight = 9,
            NumberOfObjects = 5,
        };
        List<List<int>> mask =
        [
            [Self],
            [Self],
            [Pass],
            [Self],
            [Self],
        ];
        var generator = new SiteswapGenerator(
            new InterfaceFilter(mask, 2, input, allowRotation: false),
            input
        );

        var results = generator.Generate().ToList();

        results.Should().NotBeEmpty();
        results.Should().OnlyContain(s => LandingKinds(s.Items) == "SSPSS");
    }

    private static PartialSiteswap Filled(params int[] heights) => new(heights);

    private static string LandingKinds(int[] heights)
    {
        var landing = new char[heights.Length];
        for (var i = 0; i < heights.Length; i++)
        {
            landing[(i + heights[i]) % heights.Length] = heights[i] % 2 == 0 ? 'S' : 'P';
        }

        return new string(landing);
    }
}
