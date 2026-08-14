using FluentAssertions;
using Siteswaps.Generator.Components.Internal.EasyFilter;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.WizardPage;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test.Wizard;

[TestFixture]
public class WizardPatternFilterUiTests
{
    /// <summary>Summary: Pattern palette must always expose don't-care ("frei") even when missing from allowed throws.</summary>
    [Test]
    public void WithDontCarePalette_Prepends_Empty_When_Missing()
    {
        var palette = WizardPatternFilterUi.WithDontCarePalette([Throw.Zip, Throw.Heff]).ToList();

        palette[0].Should().Be(Throw.Empty);
        WizardPatternFilterUi.Label(palette[0]).Should().Be(WizardPatternFilterUi.DontCareLabel);
        palette.Should().Contain(Throw.Zip);
    }

    /// <summary>Summary: Don't-care must not be duplicated when already present in allowed throws.</summary>
    [Test]
    public void WithDontCarePalette_Does_Not_Duplicate_Empty()
    {
        var palette = WizardPatternFilterUi
            .WithDontCarePalette([Throw.Empty, Throw.Zip])
            .ToList();

        palette.Count(t => t.Height == Throw.Empty.Height).Should().Be(1);
    }

    /// <summary>Summary: New pattern drafts must start as all don't-care slots ("frei").</summary>
    [Test]
    public void DefaultSlots_Are_All_Empty()
    {
        var slots = WizardPatternFilterUi.DefaultSlots(5);

        slots.Should().HaveCount(5);
        slots.Should().OnlyContain(t => t == Throw.Empty);
        slots.Select(WizardPatternFilterUi.Label).Should().OnlyContain(l => l == "frei");
    }

    /// <summary>Summary: DefaultSlots must never return an empty sequence.</summary>
    [Test]
    public void DefaultSlots_Clamps_Zero_Length_To_One()
    {
        WizardPatternFilterUi.DefaultSlots(0).Should().ContainSingle().Which.Should().Be(Throw.Empty);
    }
}

[TestFixture]
public class WizardStateFilterNotationTests
{
    /// <summary>Summary: State filters must render classic occupied/free notation (x / _).</summary>
    [Test]
    public void Notation_Uses_X_And_Underscore()
    {
        var filter = new EasyStateFilter.StateFilter([true, true, false, false, false]);

        filter.Notation().Should().Be("x x _ _ _");
    }
}

[TestFixture]
public class WizardMaxThrowHeightTests
{
    /// <summary>Summary: Settings MaxHeight must clamp and drop selected throws above the limit.</summary>
    [Test]
    public void ApplyMaxThrowHeight_Clamps_And_Prunes_AllowedThrows()
    {
        var state = new WizardState();
        state.AllowedThrows.Add(Throw.Quad);
        state.ApplyMaxThrowHeight(9);

        state.MaxThrowHeight.Should().Be(9);
        state.AllowedThrows.Should().OnlyContain(t => t.Height <= 9);
        state.AllowedThrows.Should().NotContain(t => t.Height == Throw.Quad.Height);
    }

    /// <summary>Summary: Throw.All offered in the chip grid must stop at MaxHeight inclusive.</summary>
    [Test]
    public void Throw_All_Respects_MaxHeight_Inclusive()
    {
        var throws = Throw.All(8).ToList();

        throws.Max(t => t.Height).Should().Be(8);
        throws.Should().OnlyContain(t => t.Height <= 8);
        throws.Should().NotContain(t => t.Height == Throw.DoublePass.Height);
    }

    /// <summary>Summary: ApplyMaxThrowHeight must clamp to AbsoluteMaxThrowHeight.</summary>
    [Test]
    public void ApplyMaxThrowHeight_Clamps_To_Absolute_Ceiling()
    {
        var state = new WizardState();
        state.ApplyMaxThrowHeight(999);

        state.MaxThrowHeight.Should().Be(WizardState.AbsoluteMaxThrowHeight);
    }
}
