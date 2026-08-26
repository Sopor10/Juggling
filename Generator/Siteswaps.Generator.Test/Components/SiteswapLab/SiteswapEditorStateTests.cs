using FluentAssertions;
using Siteswaps.Generator.Components;
using Siteswaps.Generator.Components.Feeding;
using Siteswaps.Generator.Components.SiteswapLab;

namespace Siteswaps.Generator.Test.Components.SiteswapLab;

[TestFixture]
public class PassingEditorStateTests
{
    private static PassingEditorState CreateThreePersonIndexTimeZoneState()
    {
        var state = new PassingEditorState("531");
        state.SetPersonCount(3);
        return state;
    }

    [Test]
    public void Default_State_Has_Three_People_Period_Five_And_Feeding_TimeZones()
    {
        var state = new PassingEditorState();

        state.People.Count.Should().Be(3);
        state.Period.Should().Be(5);
        state.People.Select(person => person.Name).Should().Equal("A", "B", "C");
        state.People.Select(person => person.TimeZone).Should().Equal(0, 1, 1);
    }

    public void SelectCell_Can_Reset_Selection_After_Period_Change()
    {
        var state = new PassingEditorState();

        state.AddBeat();
        state.SelectedBeat.Should().Be(state.Period - 1);

        state.SelectCell(0, 0);

        state.HasSelection.Should().BeTrue();
        state.SelectedPerson.Should().Be(0);
        state.SelectedBeat.Should().Be(0);
    }

    [Test]
    public void CycleTarget_Advances_To_Next_Available_Person_At_Same_Height()
    {
        var state = CreateThreePersonIndexTimeZoneState();
        state.CycleTimeZone(1);
        state.CycleTimeZone(1);
        state.CycleTimeZone(2);
        state.SetHeight(0, 0, 6);
        state.SelectCell(0, 0);

        state.AvailableTargetsFor(0, 0, 6).Should().Equal(0, 1, 2);
        state.People[0].Cells[0].TargetPerson.Should().Be(0);

        state.CycleTarget(0, 0).Should().BeTrue();
        state.People[0].Cells[0].TargetPerson.Should().Be(1);
        state.People[0].Cells[0].Height.Should().Be(6);

        state.CycleTarget(0, 0).Should().BeTrue();
        state.People[0].Cells[0].TargetPerson.Should().Be(2);

        state.CycleTarget(0, 0).Should().BeTrue();
        state.People[0].Cells[0].TargetPerson.Should().Be(0);
    }

    [Test]
    public void CycleTarget_Does_Nothing_When_Only_One_Target_Is_Available()
    {
        var state = new PassingEditorState();
        state.InitializeThrowsForFirstEntry();
        state.SelectCell(0, 0);

        state.CycleTarget(0, 0).Should().BeFalse();
        state.HasSelection.Should().BeTrue();
        state.People[0].Cells[0].TargetPerson.Should().Be(0);
    }

    [Test]
    public void ToggleCellSelection_Deselects_When_Clicking_Selected_Cell_Again()
    {
        var state = new PassingEditorState();
        state.InitializeThrowsForFirstEntry();
        state.SelectCell(0, 0);

        state.ToggleCellSelection(0, 0);

        state.HasSelection.Should().BeFalse();
    }

    [Test]
    public void ToggleCellSelection_Deselects_Self_Throw_Whose_Landing_Is_On_Same_Beat()
    {
        var state = new PassingEditorState();
        state.SetHeight(0, 0, 0);
        state.SelectCell(0, 0);

        state.LandingFor(0, 0).TargetPerson.Should().Be(0);
        state.LandingFor(0, 0).TargetBeat.Should().Be(0);

        state.ToggleCellSelection(0, 0);

        state.HasSelection.Should().BeFalse();
    }

    [Test]
    public void SetLandingTarget_Computes_Height_For_Clicked_Landing_Slot()
    {
        var state = new PassingEditorState();
        state.SetPersonCount(2);
        state.InitializeThrowsForFirstEntry();
        state.SelectCell(0, 0);
        var landingBeat = state.LandingFor(0, 0).TargetBeat;

        state.SetLandingTarget(0, 0, 1, landingBeat).Should().BeTrue();

        state.People[0].Cells[0].TargetPerson.Should().Be(1);
        state.LandingFor(0, 0).TargetBeat.Should().Be(landingBeat);
        state.LandingFor(0, 0).TargetPerson.Should().Be(1);
    }

    [Test]
    public void SetLandingTarget_Can_Redirect_To_Another_Person_At_Landing_Beat()
    {
        var state = CreateThreePersonIndexTimeZoneState();
        state.InitializeThrowsForFirstEntry();
        state.SelectCell(0, 0);
        var landingBeat = state.LandingFor(0, 0).TargetBeat;
        state.People[0].Cells[0].TargetPerson.Should().Be(0);

        state.SetLandingTarget(0, 0, 2, landingBeat).Should().BeTrue();

        state.People[0].Cells[0].TargetPerson.Should().Be(2);
        state.LandingFor(0, 0).TargetBeat.Should().Be(landingBeat);
    }

    [Test]
    public void AdjustHeightByPeriod_Keeps_Landing_Beat()
    {
        var state = new PassingEditorState("531");
        state.SelectCell(0, 0);
        var landing = state.LandingFor(0, 0);

        state.AdjustHeightByPeriod(0, 0, 1);

        state.People[0].Cells[0].Height.Should().Be(5 + state.HeightPeriodStep);
        state.LandingFor(0, 0).TargetBeat.Should().Be(landing.TargetBeat);
        state.LandingFor(0, 0).TargetPerson.Should().Be(landing.TargetPerson);
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
            state.People[person].Cells[originalPeriod].Height.Should().Be(state.ToGlobalHeight(3));
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
        state.People.Select(person => person.TimeZone).Should().Equal(0, 1, 1, 3, 4, 5, 6, 7);
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
        state.People.Select(person => person.TimeZone).Should().Equal(0, 1, 1, 3, 4, 5);
        state.PhaseCount.Should().Be(6);
    }

    [Test]
    public void Landing_Slots_Follow_Height_And_Shared_TimeZones()
    {
        var state = CreateThreePersonIndexTimeZoneState();
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
        state.InitializeThrowsForFirstEntry();

        state.IsValid.Should().BeTrue();
        state.CollisionTargets.Should().BeEmpty();
        state.EmptyTargets.Should().BeEmpty();
        state.Average.Should().Be(6);
        state.Notation.Should().Contain("A: 6 6");

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
        var state = CreateThreePersonIndexTimeZoneState();
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
        var state = CreateThreePersonIndexTimeZoneState();

        state.CycleTimeZone(1);
        state.CycleTimeZone(1);

        state.People.Select(person => person.TimeZone).Should().Equal(0, 0, 2);

        state.CycleTimeZone(1);
        state.People[1].TimeZone.Should().Be(1);
    }

    [Test]
    public void First_Throw_Step_Initializes_Every_Cell_As_Local_Three_Once()
    {
        var state = CreateThreePersonIndexTimeZoneState();
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
        var state = CreateThreePersonIndexTimeZoneState();
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
        var state = CreateThreePersonIndexTimeZoneState();
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
        state.People.Select(person => person.TimeZone).Should().Equal(0, 0, 1, 3);

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
        var state = CreateThreePersonIndexTimeZoneState();
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
        var state = CreateThreePersonIndexTimeZoneState();
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

        var customState = new PassingEditorState("531", maxThrowHeight: 7);
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
        var state = CreateThreePersonIndexTimeZoneState();
        state.SetHeight(0, 0, height);
        state.SetTarget(0, 0, 1).Should().BeTrue();
        var landing = state.LandingFor(0, 0);

        FeedingThrowDisplay.Format(height, state.ActiveTimeZoneCount, mode).Should().Be(expected);
        state.People[0].Cells[0].Height.Should().Be(height);
        state.People[0].Cells[0].TargetPerson.Should().Be(1);
        state.LandingFor(0, 0).Should().Be(landing);
    }

    [Test]
    public void StartingClubs_Match_StartingClubDistribution()
    {
        var state = new PassingEditorState("78627");

        state.StartingClubsFor(0).Should().Be(StartingClubDistribution.ForPerson(state, 0));
    }

    [Test]
    public void Rotate_Shifts_Every_Person_Throws_And_Updates_Selected_Beat()
    {
        var state = new PassingEditorState("531");
        state.SelectCell(0, 1);

        state.Rotate(1);

        state.People[0].Cells.Select(cell => cell.Height).Should().Equal(3, 1, 5);
        state.SelectedBeat.Should().Be(0);
    }

    [Test]
    public void ApplyChipDrop_Keeps_Source_Selected_When_Dropping_On_Source_Cell()
    {
        var state = new PassingEditorState();
        state.InitializeThrowsForFirstEntry();
        state.SelectCell(0, 0);

        state.ApplyChipDrop(0, 0, 0, 0);

        state.HasSelection.Should().BeTrue();
        state.SelectedPerson.Should().Be(0);
        state.SelectedBeat.Should().Be(0);
    }

    [Test]
    public void ApplyChipDrop_Redirects_Landing_Target_Like_SetLandingTarget()
    {
        var state = CreateThreePersonIndexTimeZoneState();
        state.InitializeThrowsForFirstEntry();
        state.SelectCell(0, 0);
        var landingBeat = state.LandingFor(0, 0).TargetBeat;
        state.People[0].Cells[0].TargetPerson.Should().Be(0);

        state.ApplyChipDrop(0, 0, 2, landingBeat);

        state.People[0].Cells[0].TargetPerson.Should().Be(2);
        state.LandingFor(0, 0).TargetBeat.Should().Be(landingBeat);
        state.HasSelection.Should().BeTrue();
        state.SelectedPerson.Should().Be(0);
        state.SelectedBeat.Should().Be(0);
    }

    [Test]
    public void ApplyChipDrop_Sets_Landing_On_Any_Valid_Cell()
    {
        var state = new PassingEditorState();
        state.InitializeThrowsForFirstEntry();
        state.SelectCell(0, 0);
        state.People[0].Cells[0].TargetPerson.Should().Be(0);
        var landingBeat = state.LandingFor(0, 0).TargetBeat;
        landingBeat.Should().NotBe(1);

        state.ApplyChipDrop(0, 0, 0, 1);

        state.People[0].Cells[0].TargetPerson.Should().Be(0);
        state.LandingFor(0, 0).TargetBeat.Should().Be(1);
        state.HasSelection.Should().BeTrue();
        state.SelectedPerson.Should().Be(0);
        state.SelectedBeat.Should().Be(0);
    }

    [Test]
    public void CanSetLandingTarget_Matches_SetLandingTarget_Without_Mutating_State()
    {
        var template = new PassingEditorState();
        template.InitializeThrowsForFirstEntry();

        for (var sourcePerson = 0; sourcePerson < template.People.Count; sourcePerson++)
        {
            for (var sourceBeat = 0; sourceBeat < template.Period; sourceBeat++)
            {
                for (var targetPerson = 0; targetPerson < template.People.Count; targetPerson++)
                {
                    for (var targetBeat = 0; targetBeat < template.Period; targetBeat++)
                    {
                        var probe = new PassingEditorState();
                        probe.InitializeThrowsForFirstEntry();
                        var canSet = probe.CanSetLandingTarget(
                            sourcePerson,
                            sourceBeat,
                            targetPerson,
                            targetBeat
                        );

                        var trial = new PassingEditorState();
                        trial.InitializeThrowsForFirstEntry();
                        trial
                            .SetLandingTarget(sourcePerson, sourceBeat, targetPerson, targetBeat)
                            .Should()
                            .Be(canSet);
                    }
                }
            }
        }
    }

    [Test]
    public void CanSetLandingTarget_Returns_False_When_MaxThrowHeight_Blocks_Landing()
    {
        var state = new PassingEditorState(maxThrowHeight: 1);
        state.InitializeThrowsForFirstEntry();

        state.MaxThrowHeight.Should().Be(1);
        state.CanSetLandingTarget(0, 0, 0, 1).Should().BeFalse();
        state.SetLandingTarget(0, 0, 0, 1).Should().BeFalse();
    }

    [Test]
    public void CanSetLandingTarget_Allows_Changing_Height_To_Reach_Different_Target_Person()
    {
        var state = CreateThreePersonIndexTimeZoneState();
        state.InitializeThrowsForFirstEntry();
        state.SetHeight(0, 0, 1);

        state.AvailableTargetsFor(0, 0, 1).Should().Equal(1);
        state.CanSetLandingTarget(0, 0, 2, 0).Should().BeTrue();
    }

    [Test]
    public void CanSetLandingTarget_Marks_Unreachable_Beats_Invalid_When_MaxThrowHeight_Is_Low()
    {
        var state = new PassingEditorState(maxThrowHeight: 8);
        state.InitializeThrowsForFirstEntry();

        state.CanSetLandingTarget(1, 0, 0, 0).Should().BeFalse();
    }

    [Test]
    public void CanSetLandingTarget_Allows_Every_Other_Cell_In_Default_Three_Person_Pattern()
    {
        var state = new PassingEditorState();
        state.InitializeThrowsForFirstEntry();

        for (var person = 0; person < state.People.Count; person++)
        {
            for (var beat = 0; beat < state.Period; beat++)
            {
                if (person == 0 && beat == 0)
                {
                    continue;
                }

                state.CanSetLandingTarget(0, 0, person, beat).Should().BeTrue();
            }
        }
    }

    [Test]
    public void ApplyChipDrop_Keeps_Source_Selected_When_Landing_Cannot_Be_Set()
    {
        var state = new PassingEditorState(maxThrowHeight: 0);
        state.InitializeThrowsForFirstEntry();
        state.SelectCell(0, 0);

        state.ApplyChipDrop(0, 0, 0, 1);

        state.HasSelection.Should().BeTrue();
        state.SelectedPerson.Should().Be(0);
        state.SelectedBeat.Should().Be(0);
    }

    [Test]
    public void ApplyChipDrop_Keeps_Dragged_Source_Selected_When_Landing_Cannot_Be_Set()
    {
        var state = new PassingEditorState(maxThrowHeight: 0);
        state.InitializeThrowsForFirstEntry();
        state.SelectCell(0, 0);

        state.ApplyChipDrop(0, 1, 0, 0);

        state.HasSelection.Should().BeTrue();
        state.SelectedPerson.Should().Be(0);
        state.SelectedBeat.Should().Be(1);
    }

    [Test]
    public void Rotate_Supports_Negative_Steps()
    {
        var state = new PassingEditorState("531");

        state.Rotate(-1);

        state.People[0].Cells.Select(cell => cell.Height).Should().Equal(1, 5, 3);
    }
}
