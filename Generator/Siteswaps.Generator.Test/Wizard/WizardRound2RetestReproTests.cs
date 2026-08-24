using System.Text.RegularExpressions;
using FluentAssertions;

namespace Siteswaps.Generator.Test.Wizard;

/// <summary>
/// Round-2 retest repros for Wizard/shared controls (desired Soll; no production fixes here).
/// </summary>
[TestFixture]
public class WizardRound2RetestReproTests
{
    /// <summary>
    /// Finding (Medium): JugglerPicker must delegate to PeriodStepper with localized ARIA labels.
    /// </summary>
    [Test]
    public void JugglerPicker_Uses_PeriodStepper_With_Localized_Labels()
    {
        var razor = ReadGeneratorSource(
            Path.Combine("Components", "WizardPage", "Controls", "JugglerPicker.razor")
        );

        razor
            .Should()
            .Contain(
                "PeriodStepper",
                "juggler selection must use the same stepper control as period"
            );

        var usesLocalizer =
            razor.Contains("IStringLocalizer", StringComparison.Ordinal)
            || Regex.IsMatch(razor, @"\bL\[");

        var hasGermanHardcodes =
            razor.Contains("Jongleure", StringComparison.Ordinal)
            || razor.Contains("oder genaue Anzahl", StringComparison.Ordinal)
            || razor.Contains("Wert auf", StringComparison.Ordinal)
            || razor.Contains("begrenzt", StringComparison.Ordinal);

        var usesChipPicker =
            razor.Contains("wizard-chip-row", StringComparison.Ordinal)
            || razor.Contains("or exact count", StringComparison.Ordinal);

        (usesLocalizer && !hasGermanHardcodes && !usesChipPicker)
            .Should()
            .BeTrue(
                "JugglerPicker must wrap PeriodStepper with localized stepper labels (no chip picker or DE hardcodes under EN)"
            );
    }

    /// <summary>
    /// Finding (Medium): Wizard editing steps use plain &lt;p class="wizard-section-label"&gt;
    /// while CSS/docs expect step headings (h2) for structure.
    /// </summary>
    [Test]
    public void Wizard_Editing_Steps_Expose_Headings()
    {
        var razor = ReadGeneratorSource(
            Path.Combine("Components", "WizardPage", "WizardPage.razor")
        );

        var editingMarkup = ExtractEditingPhaseMarkup(razor);
        editingMarkup
            .Should()
            .NotBeNullOrWhiteSpace("WizardPage must declare an Editing phase surface");

        var hasStepHeading = Regex.IsMatch(
            editingMarkup!,
            """<(h[1-3])\b|role\s*=\s*["']heading["']""",
            RegexOptions.IgnoreCase
        );

        hasStepHeading
            .Should()
            .BeTrue(
                "each wizard editing step must expose a heading (h2/h3 or role=heading), not only wizard-section-label paragraphs"
            );
    }

    private static string? ExtractEditingPhaseMarkup(string razor)
    {
        var match = Regex.Match(
            razor,
            """@if\s*\(\s*State\.Phase\s*==\s*WizardPhase\.Editing\s*\)\s*\{([\s\S]*?)\n\s*\}""",
            RegexOptions.IgnoreCase
        );
        return match.Success ? match.Groups[1].Value : null;
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
