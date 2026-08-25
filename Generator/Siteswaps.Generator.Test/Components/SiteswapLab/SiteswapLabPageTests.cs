using FluentAssertions;

namespace Siteswaps.Generator.Test.Components.SiteswapLab;

[TestFixture]
public class SiteswapLabPageTests
{
    [Test]
    public void Page_Exposes_Route_And_Renders_Cells_Without_Prototype_Tabs()
    {
        var page = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor")
        );

        page.Should().Contain("@page \"/siteswap-lab\"");
        page.Should().NotContain("lab-card-heading");
        page.Should().NotContain("Direct manipulation");
        page.Should().NotContain("Precise");
        page.Should().NotContain("lab-concept");
        page.Should().NotContain("Landing first");
        page.Should().NotContain("Sequence first");
        page.Should().NotContain("role=\"tablist\"");
        page.Should().NotContain("role=\"tabpanel\"");
        page.Should().NotContain("lab-tab-");
        page.Should().NotContain("lab-panel-");
    }

    [Test]
    public void Page_Uses_Accessible_Steppers_Without_Text_Error_List()
    {
        var page = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor")
        );
        var stepper = ReadGeneratorSource(
            Path.Combine("Components", "WizardPage", "Controls", "PeriodStepper.razor")
        );

        stepper.Should().Contain("type=\"number\"");
        stepper
            .Should()
            .Contain(
                "<label class=\"wizard-sr-only\" for=\"@_inputId\">@ValueAccessibleName</label>"
            );
        stepper.Should().NotContain("aria-label=\"@ValueAccessibleName\"");
        stepper.Should().Contain("disabled=\"@(Value <= Min)\"");
        stepper.Should().Contain("disabled=\"@(Value >= Max)\"");
        page.Should().Contain("<PeriodStepper Value=\"_direct.People.Count\"");
        page.Should().Contain("<PeriodStepper Value=\"_direct.Period\"");
        page.Should().Contain("Remove person, People");
        page.Should().Contain("Add person, People");
        page.Should().Contain("Remove last beat, Period");
        page.Should().Contain("Append beat, Period");
        page.Should().NotContain("role=\"alert\"");
        page.Should().NotContain("lab-diagnostics");
        page.Should().NotContain("Pattern needs attention");
        page.Should().NotContain("lab-statusbar");
        page.Should().NotContain("Clubs / average");
    }

    [Test]
    public void Reusable_Stepper_Owns_Its_Screenreader_Only_Style()
    {
        var css = ReadGeneratorSource(
            Path.Combine("Components", "WizardPage", "Controls", "PeriodStepper.razor.css")
        );
        var wizard = ReadGeneratorSource(
            Path.Combine("Components", "WizardPage", "WizardPage.razor")
        );
        var feedingWorkflow = ReadGeneratorSource(
            Path.Combine("Components", "GenerationWorkflow", "ConfiguredGenerationWorkflow.razor")
        );

        css.Should().Contain(".wizard-sr-only");
        css.Should().Contain("position: absolute");
        css.Should().Contain("width: 1px");
        css.Should().Contain("height: 1px");
        css.Should().Contain("overflow: hidden");
        css.Should().Contain("clip: rect(0, 0, 0, 0)");
        css.Should().Contain("white-space: nowrap");
        wizard.Should().Contain("<PeriodStepper Value=\"State.Period.Value\"");
        feedingWorkflow.Should().Contain("<PeriodStepper Value=\"State.Period.Value\"");
    }

    [Test]
    public void Cells_Uses_Interactive_TimeZones_And_Shared_Display_Settings()
    {
        var page = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor")
        );

        page.Should().Contain("class=\"lab-timezone-table\"");
        page.Should().Contain("class=\"lab-timezone-row\"");
        page.Should().Contain("class=\"lab-timezone-marker\"");
        page.Should().Contain("lab-timezone-cell-active");
        page.Should().Contain("class=\"lab-timezone-row-label\"");
        page.Should().NotContain("class=\"lab-timezone-card\"");
        page.Should().NotContain("@L[\"TimeZone {0}\", timeZoneIndex]");
        page.Should().Contain("People in the same timezone throw synchronously");
        page.Should().Contain("@onclick=\"() => _direct.CycleTimeZone(personIndex)\"");
        page.Should().Contain("OnTimeZoneRowKeyDown");
        page.Should().Contain("{0}, TimeZone {1}. Tap for next phase.");
        page.Should().Contain("class=\"lab-timezones\"");
        page.Should().NotContain("lab-timezone-arrows");
        page.Should().NotContain("LabTimeZoneArrowPath");
        page.Should().NotContain("lab-timezone-token");
        page.Should().NotContain("lab-timezone-person");
        page.Should().Contain("lab-timezone-cell lab-timezone-cell-active");
        page.Should().Contain("class=\"lab-timezone-phase-marker\"");
        page.Should().NotContain("CyclePersonForTimeZone");
        page.Should().NotContain("DisplayedPersonForTimeZone");
        page.Should().NotContain("PeopleInTimeZone");
        page.Should().NotContain("lab-timezone-shared");
        page.Should().Contain("<FeedingThrowChipRow");
        page.Should().Contain("<FeedingThrowDisplayModeToggle @bind-Mode=\"_throwDisplayMode\" />");
        page.Should().Contain("FeedingThrowDisplay.Format(");
        page.Should().Contain("GetItemAsync<SettingsDto>(\"settings\")");
        page.Should().Contain("class=\"lab-height-editor\"");
        page.Should().Contain("class=\"lab-selected-cell\"");
        page.Should().NotContain("class=\"lab-palette\"");
        page.Split("class=\"lab-target-editor\"").Should().HaveCount(2);
        page.Should().NotContain("<select");
        page.Should().Contain("class=\"feeding-throw-mode\"");
        page.Should().Contain("role=\"radiogroup\"");
        page.Should().Contain("OnChipClicked");
        page.Should().Contain("ApplyChipDrop");
        page.Should().Contain("HasSelection");
        page.Should().Contain("AdjustPassingHeightByPeriod");
        page.Should().Contain("class=\"lab-height-editor-controls\"");
        page.Should().Contain("class=\"lab-height-step-period\"");
        page.Should().Contain("±{0} = one pattern period");
        page.Should().NotContain("class=\"lab-period-height-editor\"");
        page.Should().NotContain("Period step size");
        page.Should().Contain("Select a throw above to see where it lands and edit it.");
        page.Should().Contain("lab-selected-empty");
        page.Should().Contain("Tap a throw to select it. Drag to change where it lands.");
        page.Should().Contain("class=\"lab-throws-hint\"");
        page.Should().NotContain("ChipClicked=\"beat => _direct.SelectCell");
        page.Should()
            .Contain("SetPassingTarget(_direct.SelectedPerson, _direct.SelectedBeat, target)");
        page.Should().Contain("ApplyChipDrop");
    }

    [Test]
    public void Cells_Renders_Two_Step_Wizard_And_Initializes_Throws_On_Entry()
    {
        var page = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor")
        );

        page.Should().Contain("<ProgressDots Total=\"2\"");
        page.Should().Contain("wizard-swipe-hint");
        page.Should().Contain("wizard-sr-only");
        page.Should().Contain("Step {0} / {1}: {2}");
        page.Should().Contain("People + TimeZones");
        page.Should().Contain("Next: {0}");
        page.Should().Contain("wizard-next-preview");
        page.Should().Contain("WizardShellSections.Footer");
        page.Should().Contain("<WizardStepPanel StepIndex=\"0\" IsActive=\"@IsStepActive(0)\">");
        page.Should().Contain("<WizardStepPanel StepIndex=\"1\" IsActive=\"@IsStepActive(1)\">");
        page.Should().Contain("class=\"wizard-steps\"");
        page.Should().NotContain("class=\"siteswap-lab\"");
        page.Should().NotContain("lab-steps");
        page.Should().Contain("IsStepActive");
        page.Should()
            .NotContain("@if (_cellsStep == 0)\n            {\n                <WizardStepPanel");
        page.Should().Contain("EnterThrowsStep");
        page.Should().Contain("_direct.InitializeThrowsForFirstEntry()");
        page.Should().Contain("_direct.SelectCell(0, 0)");
        page.Should().Contain("ReturnToPeopleStep");
        page.Should().NotContain("_cellsStep == 1 ? \"wizard-invisible\"");
        page.Should().NotContain("lab-step-hint");
        page.Should().NotContain("lab-wizard-nav");
        page.Should().NotContain("Reset to 3-person local-3 feed");
        page.Should().NotContain("ApplyNormalFeedPreset");
    }

    [Test]
    public void Cells_Timeline_Offset_Is_Driven_Only_By_TimeZone_Phase()
    {
        var page = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor")
        );
        var css = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor.css")
        );

        page.Should().Contain("data-timezone=\"@phase\"");
        page.Should().NotContain("data-timezone=\"@personTimeZone\"");
        page.Should().Contain("--lab-phase:{phase}");
        page.Should().Contain("--lab-phase-count:{_direct.PhaseCount}");
        page.Should().NotContain("--lab-stagger:{person}");
        css.Should().Contain(".lab-timezone-table");
        css.Should().Contain(".lab-timezone-phase-marker");
        css.Should().Contain(".lab-timezone-row");
        css.Should().Contain(".lab-timezone-marker");
        css.Should().Contain(".lab-timezone-cell-active");
        css.Should().NotContain(".lab-timezone-token");
        css.Should().Contain("var(--lab-phase, 0)");
        css.Should().Contain("var(--lab-phase-count, 1)");
        css.Should().NotContain("var(--lab-stagger");
    }

    [Test]
    public void Cells_Does_Not_Render_Standalone_Landings_Or_Notation_Blocks()
    {
        var page = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor")
        );
        page.Should().NotContain("lab-status-notation");
        page.Should().NotContain("Passing notation");
        page.Should().NotContain("<h3>Landings</h3>");
        page.Should().NotContain("<h3>Notation</h3>");
        page.Should().NotContain("class=\"lab-selected-landing\"");
    }

    [Test]
    public void Throws_Step_Toolbar_Shows_Rotation_Only()
    {
        var page = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor")
        );
        var toolbar = page.Split("class=\"lab-overview-toolbar\"", 2)[1]
            .Split("class=\"lab-passing-overview\"", 2)[0];

        page.Should().Contain("class=\"lab-overview-toolbar\"");
        page.Should().Contain("class=\"lab-overview-toolbar-group\"");
        page.Should().Contain("class=\"lab-overview-toolbar-label\"");
        page.Should().NotContain("class=\"lab-overview-clubs\"");
        page.Should().NotContain("ClubsLabel");
        page.Should().NotContain("FeedingThrowDisplay.FormatAverage(_direct.Average)");
        toolbar.Should().Contain("class=\"lab-overview-rotation\"");
        toolbar.Should().Contain("Rotate starting position");
        toolbar.Should().NotContain("FeedingThrowDisplayModeToggle");
        toolbar.Should().NotContain("@L[\"Throw display\"]");
        page.Should().NotContain("lab-statusbar");
        page.Should().NotContain("Clubs / average");
    }

    [Test]
    public void Throws_Step_Shows_Starting_Clubs_And_Rotation_Controls()
    {
        var page = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor")
        );
        var css = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor.css")
        );

        page.Should().Contain("class=\"lab-overview-rotation\"");
        page.Should().Contain("@onclick=\"() => _direct.Rotate(1)\"");
        page.Should().Contain("@onclick=\"() => _direct.Rotate(-1)\"");
        page.Should().Contain("Rotate starting position");
        page.Should().Contain("class=\"lab-start-props\"");
        page.Should().Contain("_direct.StartingClubsFor(personIndex)");
        page.Should().Contain("Start clubs");
        css.Should().Contain(".lab-start-props");
        css.Should().Contain(".lab-rotate");
    }

    [Test]
    public void LandingBeatFor_Marks_Only_Target_Person_Not_Entire_Beat_Column()
    {
        var page = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor")
        );

        page.Should().Contain("SelectedPassingLanding.TargetPerson == person");
        page.Should().Contain("SelectedPassingLanding.TargetBeat");
        page.Should().NotContain("LandingBeatFor(int _)");
    }

    [Test]
    public void OnChipClicked_Only_Selects_Or_Deselects()
    {
        var page = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor")
        );
        var chipClicked = page.Split("private void OnChipClicked", 2)[1]
            .Split("private void OnChipDragStarted", 2)[0];

        chipClicked.Should().Contain("ToggleCellSelection");
        chipClicked.Should().Contain("SelectCell");
        chipClicked.Should().Contain("_direct.HasSelection");
        chipClicked.Should().NotContain("ApplyChipDrop");
        chipClicked.Should().NotContain("SetLandingTarget");
        chipClicked.Should().NotContain("CycleTarget");
    }

    [Test]
    public void PassingChipAccessibleName_Describes_Drag_And_Drop_Targets()
    {
        var page = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor")
        );
        var accessibleName = page.Split("private string PassingChipAccessibleName", 2)[1]
            .Split("private string LandingSlotStateClass", 2)[0];

        accessibleName.Should().Contain("_dragSourcePerson");
        accessibleName.Should().Contain("CanSetLandingTarget");
        accessibleName.Should().Contain("Drop on another cell to set landing target.");
        accessibleName
            .Should()
            .Contain("Drop to land dragged throw from {0} beat {1} on {2} beat {3}.");
    }

    [Test]
    public void PassingChipAccessibleName_Does_Not_Suggest_Landing_On_Click()
    {
        var page = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor")
        );
        var accessibleName = page.Split("private string PassingChipAccessibleName", 2)[1]
            .Split("private string LandingSlotStateClass", 2)[0];

        accessibleName.Should().Contain("Landing of selected throw from");
        accessibleName.Should().NotContain("Press to land");
    }

    [Test]
    public void Throws_Step_Enables_Drag_And_Drop_On_Chip_Rows()
    {
        var page = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor")
        );
        var chipRow = ReadGeneratorSource(
            Path.Combine("Components", "Feeding", "FeedingThrowChipRow.razor")
        );
        var css = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor.css")
        );

        page.Should().Contain("EnableDragDrop=\"true\"");
        page.Should().Contain("RowPersonIndex=\"@personIndex\"");
        page.Should().Contain("ActiveDragSourcePerson=\"@_dragSourcePerson\"");
        page.Should().Contain("ActiveDragSourceBeat=\"@_dragSourceBeat\"");
        page.Should().Contain("IsValidDropTarget=");
        page.Should().Contain("IsValidDropTargetForDrag");
        page.Should().Contain("ChipDragStarted=");
        page.Should().Contain("ChipDragEntered=");
        page.Should().Contain("ChipDropped=");
        page.Should().Contain("ChipDragEnded=");
        page.Should().Contain("OnChipDragStarted");
        page.Should().Contain("ApplyChipDrop");
        chipRow.Should().Contain("draggable=\"@(EnableDragDrop");
        chipRow.Should().Contain("@ondragover:preventDefault");
        chipRow.Should().Contain("@ondrop:preventDefault");
        chipRow.Should().Contain("is-dragging");
        chipRow.Should().Contain("is-drop-target");
        chipRow.Should().Contain("is-drop-invalid");
        chipRow.Should().Contain("IsValidDropTarget");
        chipRow.Should().Contain("ActiveDragSourcePerson");
        chipRow.Should().Contain("RowPersonIndex");
        chipRow.Should().Contain("aria-grabbed");
        chipRow.Should().Contain("@onpointerdown");
        chipRow.Should().Contain("@onpointermove");
        chipRow.Should().Contain("@onpointerup");
        chipRow.Should().Contain("FeedingThrowChipPointerDrag");
        css.Should().Contain("touch-action: none");
        css.Should().Contain(".feeding-beat.is-dragging");
        css.Should().Contain(".feeding-beat.is-drop-target");
        css.Should().Contain(".feeding-beat.is-drop-invalid");
    }

    [Test]
    public void Cells_Legend_Matches_All_Visible_Noninteractive_Chip_States()
    {
        var page = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor")
        );

        page.Should().Contain("class=\"lab-chip-legend\"");
        page.Should().Contain("feeding-beat self");
        page.Should().Contain("feeding-beat pass");
        page.Should().Contain("is-selected");
        page.Should().Contain("is-landing");
        page.Should().Contain("is-incoming");
        page.Should().Contain("collision");
        page.Should().Contain("unfilled");
        page.Should().Contain("No incoming throw");
        page.Should().Contain("Incoming throw");
        page.Should().Contain("StateClass=\"chip => PassingChipStateClass");
        page.Should().NotContain("<button class=\"feeding-beat self\"");

        var legend = page.Split("class=\"lab-chip-legend\"", 2)[1].Split("</ul>", 2)[0];
        legend.Should().Contain("class=\"lab-chip-legend-throw-display\"");
        legend.Should().NotContain("@L[\"Throw display\"]");
        legend
            .Should()
            .Contain("<FeedingThrowDisplayModeToggle @bind-Mode=\"_throwDisplayMode\" />");
    }

    [Test]
    public void Selected_Throw_Shows_Incoming_Sources_In_Panel_And_Grid()
    {
        var page = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor")
        );
        var css = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor.css")
        );

        page.Should().Contain("SourcesLandingAt(_direct.SelectedPerson, _direct.SelectedBeat)");
        page.Should().Contain("SelectedIncomingLandings");
        page.Should().Contain("IsIncomingSourceForSelection");
        page.Should().Contain("class=\"lab-incoming-throws\"");
        page.Should().Contain("class=\"lab-incoming-list\"");
        page.Should().Contain("Lands from");
        page.Should().Contain("No throws land here.");
        page.Should().Contain("Lands on selected cell at {4} beat {5}.");
        page.Should().Contain("PassingChipStateClass");

        css.Should().Contain(".feeding-beat.is-incoming");
        css.Should().Contain(".lab-incoming-throws");
        css.Should().Contain(".lab-incoming-list");
    }

    [Test]
    public void Page_Is_Localized_And_Has_A_Mobile_Layout()
    {
        var page = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor")
        );
        var css = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor.css")
        );

        page.Should().Contain("IStringLocalizer<SiteswapLabPage>");
        css.Should().Contain("@media (max-width: 480px)");
        css.Should().Contain("min-height: 44px");
        css.Should().Contain(":focus-visible");
    }

    private static string ReadGeneratorSource(string relativePathUnderGeneratorProject) =>
        File.ReadAllText(
            Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "..",
                "..",
                "..",
                "..",
                "Siteswaps.Generator",
                relativePathUnderGeneratorProject
            )
        );
}
