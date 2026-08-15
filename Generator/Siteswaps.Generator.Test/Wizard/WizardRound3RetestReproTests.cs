using System.Text.RegularExpressions;
using FluentAssertions;
using Siteswaps.Generator.Components.WizardPage;

namespace Siteswaps.Generator.Test.Wizard;

/// <summary>
/// Round-3 retest repros for Wizard (desired Soll; no production fixes here).
/// </summary>
[TestFixture]
public class WizardRound3RetestReproTests
{
    /// <summary>
    /// Finding (High): ProgressDots Total=ProgressStepCount (4) while editing StepAnnouncement
    /// denominator uses TotalSteps (3) — visible "step X of Y" disagrees with the dots.
    /// Soll: ProgressDots Total and StepAnnouncement denominator use the same step-count constant.
    /// </summary>
    [Test]
    public void Wizard_StepAnnouncement_Denominator_Matches_ProgressDots_Total()
    {
        var razor = ReadGeneratorSource(
            Path.Combine("Components", "WizardPage", "WizardPage.razor")
        );
        var codeBehind = ReadGeneratorSource(
            Path.Combine("Components", "WizardPage", "WizardPage.razor.cs")
        );

        var dotsTotal = ExtractProgressDotsTotalBinding(razor);
        dotsTotal
            .Should()
            .NotBeNullOrWhiteSpace("WizardPage must bind ProgressDots Total");

        var announcementDenominator = ExtractStepAnnouncementDenominator(codeBehind);
        announcementDenominator
            .Should()
            .NotBeNullOrWhiteSpace("WizardPage must declare StepAnnouncement with a /Y denominator");

        var sameConstant =
            string.Equals(dotsTotal, announcementDenominator, StringComparison.Ordinal)
            || ResolvedStepCount(dotsTotal!) == ResolvedStepCount(announcementDenominator!);

        sameConstant
            .Should()
            .BeTrue(
                "ProgressDots Total and StepAnnouncement denominator must agree "
                    + $"(dots='{dotsTotal}' → {ResolvedStepCount(dotsTotal!)}, "
                    + $"announcement='{announcementDenominator}' → {ResolvedStepCount(announcementDenominator!)})"
            );
    }

    /// <summary>
    /// Finding (High): Wizard Results phase / ResultsView has Adjust filters + New search,
    /// but no Back control matching editing-step navigation (L["Back"] / wizard-back-btn).
    /// </summary>
    [Test]
    public void Wizard_Results_Phase_Exposes_Back_Button()
    {
        var resultsView = ReadGeneratorSource(
            Path.Combine("Components", "WizardPage", "Results", "ResultsView.razor")
        );
        var wizardPage = ReadGeneratorSource(
            Path.Combine("Components", "WizardPage", "WizardPage.razor")
        );

        var resultsPhaseMarkup = ExtractResultsPhaseMarkup(wizardPage) ?? string.Empty;

        var hasBack =
            HasBackControl(resultsView)
            || HasBackControl(resultsPhaseMarkup);

        hasBack
            .Should()
            .BeTrue(
                "ResultsView or Wizard Results phase must expose a Back button "
                    + "(L[\"Back\"] or wizard-back-btn), not only Adjust filters / New search"
            );
    }

    private static string? ExtractProgressDotsTotalBinding(string razor)
    {
        var match = Regex.Match(
            razor,
            @"<ProgressDots\b[\s\S]*?\bTotal\s*=\s*""([^""]+)""",
            RegexOptions.IgnoreCase
        );
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string? ExtractStepAnnouncementDenominator(string codeBehind)
    {
        var property = Regex.Match(
            codeBehind,
            @"StepAnnouncement\s*=>\s*([\s\S]*?);",
            RegexOptions.IgnoreCase
        );
        if (!property.Success)
        {
            return null;
        }

        var body = property.Groups[1].Value;

        // L["Step {0} / {1}: {2}", current, DENOMINATOR, title]
        var args = Regex.Match(
            body,
            @"\[\s*""Step \{0\} / \{1\}:[^""]*""\s*,\s*[^,]+,\s*([^,]+)\s*,",
            RegexOptions.IgnoreCase
        );
        return args.Success ? args.Groups[1].Value.Trim() : null;
    }

    private static int ResolvedStepCount(string expression)
    {
        if (expression.Contains(nameof(WizardState.ProgressStepCount), StringComparison.Ordinal))
        {
            return WizardState.ProgressStepCount;
        }

        if (expression.Contains(nameof(WizardState.TotalSteps), StringComparison.Ordinal))
        {
            return WizardState.TotalSteps;
        }

        if (int.TryParse(expression, out var literal))
        {
            return literal;
        }

        return int.MinValue;
    }

    private static string? ExtractResultsPhaseMarkup(string razor)
    {
        // Results live in the else of Phase == Editing
        var match = Regex.Match(
            razor,
            """State\.Phase\s*==\s*WizardPhase\.Editing[\s\S]*?\n\s*\}\s*\n\s*else\s*\n\s*\{([\s\S]*?)\n\s*\}""",
            RegexOptions.IgnoreCase
        );
        return match.Success ? match.Groups[1].Value : null;
    }

    private static bool HasBackControl(string markup) =>
        markup.Contains("""L["Back"]""", StringComparison.Ordinal)
        || markup.Contains("wizard-back-btn", StringComparison.OrdinalIgnoreCase)
        || Regex.IsMatch(
            markup,
            """>\s*@L\["Back"\]\s*<""",
            RegexOptions.IgnoreCase
        );

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
