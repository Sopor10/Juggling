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

        page.Should().Contain("<button type=\"button\"");
        page.Should().Contain("class=\"lab-timezone-card\"");
        page.Should().NotContain("@L[\"TimeZone {0}\", timeZoneIndex]");
        page.Should().Contain("People in the same timezone throw synchronously");
        page.Should().Contain("@onclick=\"() => _direct.CycleTimeZone(personIndex)\"");
        page.Should().Contain("class=\"lab-timezones\"");
        page.Should().NotContain("lab-timezone-arrows");
        page.Should().NotContain("LabTimeZoneArrowPath");
        page.Should().Contain("lab-timezone-person");
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

        page.Should().Contain("data-timezone=\"@_direct.TimelinePhaseFor(person)\"");
        page.Should().Contain("--lab-phase:{_direct.TimelinePhaseFor(person)}");
        page.Should().Contain("--lab-phase-count:{_direct.PhaseCount}");
        page.Should().NotContain("--lab-stagger:{person}");
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
        page.Should().Contain("class=\"lab-selected-landing\"");
    }

    [Test]
    public void Throws_Step_Shows_Compact_Club_Count_Without_Statusbar()
    {
        var page = ReadGeneratorSource(
            Path.Combine("Components", "SiteswapLab", "SiteswapLabPage.razor")
        );

        page.Should().Contain("class=\"lab-overview-clubs\"");
        page.Should().Contain("FeedingThrowDisplay.FormatAverage(_direct.Average)");
        page.Should().Contain("{0} clubs");
        page.Should().NotContain("lab-statusbar");
        page.Should().NotContain("Clubs / average");
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
        page.Should().Contain("collision");
        page.Should().Contain("unfilled");
        page.Should().Contain("No incoming throw");
        page.Should().Contain("StateClass=\"chip => LandingSlotStateClass");
        page.Should().NotContain("<button class=\"feeding-beat self\"");
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
