using FluentAssertions;
using Siteswaps.Generator.Components.WizardPage.Controls;

namespace Siteswaps.Generator.Test.Wizard;

[TestFixture]
public class DualRangeInclusiveTests
{
    /// <summary>
    /// Summary: Fill for 5–7 on a 2–30 track must cover three inclusive slots, not [5, 7).
    /// </summary>
    [Test]
    public void Fill_Includes_Max_Slot()
    {
        var (left, width) = DualRangeInclusive.Fill(5, 7, 2, 30);

        const double slots = 29;
        left.Should().BeApproximately(3 / slots * 100, 0.01);
        width.Should().BeApproximately(3 / slots * 100, 0.01);
        width.Should().BeGreaterThan((7 - 5) * 100.0 / (30 - 2));
    }

    /// <summary>Summary: A single selected value still occupies its full inclusive slot.</summary>
    [Test]
    public void Fill_Single_Value_Has_Slot_Width()
    {
        var (left, width) = DualRangeInclusive.Fill(7, 7, 2, 30);

        const double slots = 29;
        left.Should().BeApproximately(5 / slots * 100, 0.01);
        width.Should().BeApproximately(100.0 / slots, 0.01);
    }

    /// <summary>Summary: Full bounds must paint the entire track.</summary>
    [Test]
    public void Fill_Full_Range_Is_Entire_Track()
    {
        var (left, width) = DualRangeInclusive.Fill(2, 30, 2, 30);

        left.Should().Be(0);
        width.Should().Be(100);
    }
}
