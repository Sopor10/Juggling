using FluentAssertions;
using Siteswaps.Generator.Components;
using Siteswaps.Generator.Components.Feeding;
using Siteswaps.Generator.Components.SiteswapLab;

namespace Siteswaps.Generator.Test.Components.SiteswapLab;

[TestFixture]
public class PassingEditorStateTests
{
    [Test]
    public void SelectCell_Can_Reset_Selection_After_Period_Change()
    {
        var state = new PassingEditorState();

        state.AddBeat();
        state.SelectedBeat.Should().Be(state.Period - 1);

        state.SelectCell(0, 0);

        state.SelectedPerson.Should().Be(0);
        state.SelectedBeat.Should().Be(0);
    }

    [Test]
    public void Passing_Period_Extension_Preserves_Throws_And_Appends_Self_Defaults()
    {
        var state = new PassingEditorState();
        state.InitializeThrowsForFirstEntry();
        var original = state
            .People.Select(person =>
                person.Cells.Select(cell => (cell.Height, cell.TargetPerson)).ToArray()
            )
            .ToArray();
        var originalPeriod = state.Period;

        state.AddBeat();

        state.Period.Should().Be(originalPeriod + 1);
        for (var person = 0; person < state.People.Count; person++)
        {
            state
                .People[person]
                .Cells.Take(originalPeriod)
                .Select(cell => (cell.Height, cell.TargetPerson))
                .Should()
                .Equal(original[person]);
            state.People[person].Cells[originalPeriod].Height.Should().Be(3);
            state.People[person].Cells[originalPeriod].TargetPerson.Should().Be(person);
        }

        state.RemoveBeat();
        state.Period.Should().Be(originalPeriod);
        state.People.Should().OnlyContain(person => person.Cells.Count == originalPeriod);
    }

    [Test]
    public void People_Map_To_TimeZones_By_Their_Index()
    {
        var state = new PassingEditorState();

        state.SetPersonCount(PassingEditorState.MaxPeople);

        state
            .People.Select(person => person.Name)
            .Should()
            .Equal("A", "B", "C", "D", "E", "F", "G", "H");
        state.People.Select(person => person.TimeZone).Should().Equal(0, 1, 2, 3, 4, 5, 6, 7);
        state.PhaseCount.Should().Be(PassingEditorState.MaxPeople);
    }

    [Test]
    public void Adding_And_Removing_People_Preserves_Index_TimeZone_Mapping()
    {
        var state = new PassingEditorState();

        state.SetPersonCount(8);
        state.SetPersonCount(3);
        state.SetPersonCount(6);

        state.People.Select(person => person.Name).Should().Equal("A", "B", "C", "D", "E", "F");
        state.People.Select(person => person.TimeZone).Should().Equal(0, 1, 2, 3, 4, 5);
        state.PhaseCount.Should().Be(6);
    }

    [Test]
    public void Landing_Slots_Follow_Height_And_Shared_TimeZones()
    {
        var state = new PassingEditorState();
        state.SetPersonCount(3);
        state.SetHeight(0, 0, 1);

        state.AvailableTargetsFor(0, 0, 1).Should().Equal(1);
        state.People[0].Cells[0].TargetPerson.Should().Be(1);

        state.CycleTimeZone(2);
        state.CycleTimeZone(2);
        state.People.Select(person => person.TimeZone).Should().Equal(0, 1, 1);
        state.AvailableTargetsFor(0, 0, 1).Should().Equal(1, 2);
        state.SetTarget(0, 0, 1).Should().BeTrue();
        state.SetTarget(0, 0, 2).Should().BeTrue();
        state.People[0].Cells[0].Height.Should().Be(1);
        state.LandingFor(0, 0).TargetPerson.Should().Be(2);
        state.LandingFor(0, 0).TargetBeat.Should().Be(0);
        state.LandingFor(0, 0).TargetTimeZone.Should().Be(1);
        state.AvailableTargetsFor(0, 0, 1).Should().NotContain(0);
    }

    [Test]
    public void Initialized_Throws_Are_Valid_And_Report_Collisions_Immediately()
    {
        var state = new PassingEditorState();
        state.SetPersonCount(3);
        state.InitializeThrowsForFirstEntry();

        state.IsValid.Should().BeTrue();
        state.CollisionTargets.Should().BeEmpty();
        state.EmptyTargets.Should().BeEmpty();
        state.Average.Should().Be(9);
        state.Notation.Should().Contain("A: 9 9");

        state.CycleTimeZone(2);
        state.CycleTimeZone(2);
        state.SetHeight(0, 0, 1);
        state.SetTarget(0, 0, 2).Should().BeTrue();
        state.SetHeight(1, 0, 0);
        state.SetTarget(1, 0, 2).Should().BeTrue();

        state.IsValid.Should().BeFalse();
        state.CollisionTargets.Should().Contain(new PassingLandingSlot(2, 0));
        state.EmptyTargets.Should().NotBeEmpty();
    }

    [Test]
    public void Same_Landing_Beat_Uses_Explicit_Person_Slots()
    {
        var state = new PassingEditorState();
        state.SetPersonCount(3);
        state.InitializeThrowsForFirstEntry();

        var simultaneous = new[]
        {
            state.LandingFor(0, 0),
            state.LandingFor(1, 0),
            state.LandingFor(2, 0),
        };

        simultaneous.Select(landing => landing.TargetBeat).Should().OnlyContain(beat => beat == 0);
        simultaneous
            .Select(landing => landing.TargetPerson)
            .Should()
            .BeEquivalentTo(new int?[] { 0, 1, 2 });
        state.CollisionTargets.Should().BeEmpty();
    }

    [Test]
    public void TimeZones_Cycle_Independently_And_May_Be_Shared()
    {
        var state = new PassingEditorState();
        state.SetPersonCount(3);

        state.CycleTimeZone(1);
        state.CycleTimeZone(1);

        state.People.Select(person => person.TimeZone).Should().Equal(0, 0, 2);

        state.CycleTimeZone(1);
        state.People[1].TimeZone.Should().Be(1);
    }

    [Test]
    public void First_Throw_Step_Initializes_Every_Cell_As_Local_Three_Once()
    {
        var state = new PassingEditorState();
        state.SetPersonCount(3);
        state.CycleTimeZone(2);
        state.CycleTimeZone(2);

        state.InitializeThrowsForFirstEntry();

        state.ThrowsInitialized.Should().BeTrue();
        state
            .People.SelectMany(person => person.Cells)
            .Should()
            .OnlyContain(cell => cell.Height == 6);
        state
            .People.SelectMany(person => person.Cells)
            .Select(cell =>
                FeedingThrowDisplay.Format(
                    cell.Height,
                    state.ActiveTimeZoneCount,
                    FeedingThrowDisplay.Mode.Local
                )
            )
            .Should()
            .OnlyContain(display => display == "3");
        state.People[1].Cells.Should().OnlyContain(cell => cell.TargetPerson == 1);
        state.People[2].Cells.Should().OnlyContain(cell => cell.TargetPerson == 2);

        state.SetHeight(1, 0, 6);
        state.InitializeThrowsForFirstEntry();

        state.People[1].Cells[0].Height.Should().Be(6);
    }

    [Test]
    public void New_People_And_Beats_Use_Local_Three_Without_Rewriting_Existing_Cells()
    {
        var state = new PassingEditorState();
        state.SetPersonCount(3);
        state.InitializeThrowsForFirstEntry();
        state.SetHeight(0, 0, 6);

        state.SetPersonCount(4);

        state.People[0].Cells[0].Height.Should().Be(6);
        state.People[1].Cells[0].Height.Should().Be(9);
        state.People[3].Cells.Should().OnlyContain(cell => cell.Height == 12);

        state.AddBeat();

        foreach (var person in state.People)
        {
            person.Cells[person.Cells.Count - 1].Height.Should().Be(12);
            person.Cells[person.Cells.Count - 1].TargetPerson.Should().NotBeNull();
        }
    }

    [Test]
    public void Timeline_Phase_Uses_TimeZone_Not_Person_Index_And_Updates_When_Cycled()
    {
        var state = new PassingEditorState();
        state.SetPersonCount(3);
        state.CycleTimeZone(2);
        state.CycleTimeZone(2);

        state.TimelinePhaseFor(1).Should().Be(1);
        state.TimelinePhaseFor(2).Should().Be(1);

        state.CycleTimeZone(2);

        state.TimelinePhaseFor(2).Should().Be(2);
        state.TimelinePhaseFor(1).Should().Be(1);
    }

    [Test]
    public void Person_Count_Changes_Preserve_Valid_Shared_TimeZones()
    {
        var state = new PassingEditorState();
        state.SetPersonCount(3);
        state.CycleTimeZone(1);
        state.CycleTimeZone(1);

        state.SetPersonCount(4);
        state.People.Select(person => person.TimeZone).Should().Equal(0, 0, 2, 3);

        state.SetPersonCount(2);
        state.People.Select(person => person.TimeZone).Should().Equal(0, 0);
    }

    [Test]
    public void Landing_Beat_Is_Height_Distance_With_Period_Wraparound()
    {
        var state = new PassingEditorState("531");

        state.LandingFor(0, 0).TargetBeat.Should().Be(2);
        state.LandingFor(0, 1).TargetBeat.Should().Be(1);
    }

    [Test]
    public void Height_Six_Target_Slot_Changes_Do_Not_Change_Height()
    {
        var state = new PassingEditorState();
        state.SetPersonCount(3);
        state.CycleTimeZone(1);
        state.CycleTimeZone(1);
        state.CycleTimeZone(2);
        state.SetHeight(0, 0, 6);

        state.AvailableTargetsFor(0, 0, 6).Should().Equal(0, 1, 2);
        state.SetTarget(0, 0, 1).Should().BeTrue();
        state.SetTarget(0, 0, 2).Should().BeTrue();
        state.SetTarget(0, 0, 0).Should().BeTrue();

        state.People[0].Cells[0].Height.Should().Be(6);
    }

    [Test]
    public void Height_And_TimeZone_Changes_Update_Available_Slots()
    {
        var state = new PassingEditorState();
        state.SetPersonCount(3);
        state.CycleTimeZone(2);
        state.CycleTimeZone(2);
        state.SetHeight(0, 0, 1);
        state.SetTarget(0, 0, 2).Should().BeTrue();

        state.AvailableTargetsFor(0, 0, 1).Should().Equal(1, 2);

        state.SetHeight(0, 0, 2);

        state.People[0].Cells[0].Height.Should().Be(2);
        state.AvailableTargetsFor(0, 0, 2).Should().Equal(0);
        state.People[0].Cells[0].TargetPerson.Should().Be(0);
        state.LastTargetAdjustment.Should().NotBeNull();
    }

    [Test]
    public void Local_Height_Scales_With_Active_TimeZone_Count_Not_People()
    {
        var state = new PassingEditorState();
        state.SetPersonCount(3);
        state.CycleTimeZone(2);
        state.CycleTimeZone(2);

        state.ActiveTimeZoneCount.Should().Be(2);
        state.ToGlobalHeight(3).Should().Be(6);
        state.InitializeThrowsForFirstEntry();

        state
            .People.SelectMany(person => person.Cells)
            .Should()
            .OnlyContain(cell => cell.Height == 6);
    }

    [Test]
    public void Max_Throw_Height_Uses_Default_Custom_And_Nondestructive_Lowering()
    {
        var defaultState = new PassingEditorState();
        defaultState.MaxThrowHeight.Should().Be(new SettingsDto().MaxHeight);

        var customState = new PassingEditorState(maxThrowHeight: 7);
        customState.MaxThrowHeight.Should().Be(7);
        customState.SetHeight(0, 0, 7);
        customState.People[0].Cells[0].Height.Should().Be(7);

        customState.ApplyMaxThrowHeight(3);
        customState.People[0].Cells[0].Height.Should().Be(7);
        customState.HeightLimitViolationCount.Should().Be(1);
        customState.IsValid.Should().BeFalse();
    }

    [TestCase(FeedingThrowDisplay.Mode.Local, 4, "1.33")]
    [TestCase(FeedingThrowDisplay.Mode.Global, 4, "4")]
    [TestCase(FeedingThrowDisplay.Mode.Name, 4, "1.33")]
    public void Throw_Display_Modes_Do_Not_Mutate_Editing_Or_Landing(
        FeedingThrowDisplay.Mode mode,
        int height,
        string expected
    )
    {
        var state = new PassingEditorState();
        state.SetPersonCount(3);
        state.SetHeight(0, 0, height);
        state.SetTarget(0, 0, 1).Should().BeTrue();
        var landing = state.LandingFor(0, 0);

        FeedingThrowDisplay.Format(height, state.ActiveTimeZoneCount, mode).Should().Be(expected);
        state.People[0].Cells[0].Height.Should().Be(height);
        state.People[0].Cells[0].TargetPerson.Should().Be(1);
        state.LandingFor(0, 0).Should().Be(landing);
    }
}
