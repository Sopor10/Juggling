using System.Text.RegularExpressions;
using FluentAssertions;

namespace Siteswaps.Generator.Test.Feeding;

/// <summary>
/// Round-2 retest repros for Feeding (desired Soll; no production fixes here).
/// </summary>
[TestFixture]
public class FeedingRound2RetestReproTests
{
    /// <summary>
    /// Finding (High): ConfiguredGenerationWorkflow on /feeding sits outside .wizard-page,
    /// so wizard-chip / wizard-btn-generate keep native ~30px sizing.
    /// </summary>
    [Test]
    public void Feeding_Or_Host_Provides_Wizard_Chip_And_Generate_Touch_Styles_Without_Wizard_Page()
    {
        var feedingCss = ReadGeneratorSource(
            Path.Combine("Components", "Feeding", "FeedingPage.razor.css")
        );
        var hostCssPath = GeneratorProjectPath(
            Path.Combine(
                "Components",
                "GenerationWorkflow",
                "ConfiguredGenerationWorkflow.razor.css"
            )
        );
        var hostCss = File.Exists(hostCssPath) ? File.ReadAllText(hostCssPath) : string.Empty;

        var providesChip =
            StylesWizardControlOutsideWizardPage(feedingCss, "wizard-chip", 44)
            || StylesWizardControlOutsideWizardPage(hostCss, "wizard-chip", 44);

        var providesGenerate =
            StylesWizardControlOutsideWizardPage(feedingCss, "wizard-btn-generate", 44)
            || StylesWizardControlOutsideWizardPage(feedingCss, "wizard-btn-generate", 48)
            || StylesWizardControlOutsideWizardPage(hostCss, "wizard-btn-generate", 44)
            || StylesWizardControlOutsideWizardPage(hostCss, "wizard-btn-generate", 48);

        (providesChip && providesGenerate)
            .Should()
            .BeTrue(
                "Feeding embeds ConfiguredGenerationWorkflow outside .wizard-page — FeedingPage or ConfiguredGenerationWorkflow CSS must style .wizard-chip (min-height 44px) and .wizard-btn-generate (min-height ≥ 44px) without a .wizard-page ancestor"
            );
    }

    /// <summary>
    /// Finding (Medium): B1/B2 local result buttons omit selected visual + aria-pressed
    /// (unlike pass-assignment chips).
    /// </summary>
    [Test]
    public void Feeding_Local_Result_Buttons_Expose_Active_And_AriaPressed()
    {
        var razor = ReadGeneratorSource(Path.Combine("Components", "Feeding", "FeedingPage.razor"));

        foreach (var labelKey in new[] { "B1 local results", "B2 local results" })
        {
            var listMarkup = ExtractResultsListAfterLabel(razor, labelKey);
            listMarkup
                .Should()
                .NotBeNullOrWhiteSpace($"FeedingPage must render a results list for {labelKey}");

            listMarkup!
                .Should()
                .Contain("feeding-result", $"{labelKey} list must use feeding-result buttons");
            listMarkup
                .Should()
                .Contain(
                    "aria-pressed",
                    $"{labelKey} selection must expose aria-pressed like pass chips"
                );
            Regex
                .IsMatch(listMarkup, @"\bactive\b")
                .Should()
                .BeTrue($"{labelKey} selection must toggle an active class for visual selection");
        }
    }

    private static string? ExtractResultsListAfterLabel(string razor, string labelKey)
    {
        var match = Regex.Match(
            razor,
            $"""@L\["{Regex.Escape(labelKey)}"\][\s\S]*?<ul class="feeding-results">([\s\S]*?)</ul>""",
            RegexOptions.IgnoreCase
        );
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Drop .wizard-page-scoped rules — they do not apply when the host is on /feeding.
    /// </summary>
    private static bool StylesWizardControlOutsideWizardPage(
        string css,
        string className,
        int minHeightPx
    )
    {
        if (string.IsNullOrWhiteSpace(css))
        {
            return false;
        }

        var withoutWizardPageRules = Regex.Replace(
            css,
            @"[^\{\}]*\.wizard-page[^\{\}]*\{[^\{\}]*\}",
            string.Empty,
            RegexOptions.IgnoreCase
        );

        return withoutWizardPageRules.Contains(className, StringComparison.Ordinal)
            && withoutWizardPageRules.Contains(
                $"min-height: {minHeightPx}px",
                StringComparison.Ordinal
            );
    }

    private static string ReadGeneratorSource(string relativePathUnderGeneratorProject) =>
        File.ReadAllText(GeneratorProjectPath(relativePathUnderGeneratorProject));

    private static string GeneratorProjectPath(string relativePathUnderGeneratorProject) =>
        Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "..",
            "..",
            "..",
            "..",
            "Siteswaps.Generator",
            relativePathUnderGeneratorProject
        );
}
