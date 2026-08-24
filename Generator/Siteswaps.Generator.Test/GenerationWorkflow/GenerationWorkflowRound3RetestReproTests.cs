using System.Text.RegularExpressions;
using FluentAssertions;

namespace Siteswaps.Generator.Test.GenerationWorkflow;

/// <summary>
/// Round-3 retest repros for ConfiguredGenerationWorkflow host CSS (desired Soll; no production fixes here).
/// </summary>
[TestFixture]
public class GenerationWorkflowRound3RetestReproTests
{
    /// <summary>
    /// Finding (Medium): Host chip-grid uses repeat(auto-fill, minmax(44px, 1fr)) plus chip ellipsis,
    /// while Wizard uses repeat(4, minmax(0, 1fr)) — Feeding Generate labels truncate.
    /// Soll: Host columns match Wizard, OR chips do not ellipsis labels.
    /// </summary>
    [Test]
    public void Host_ChipGrid_Columns_Match_Wizard_Or_Chips_Do_Not_Ellipsis()
    {
        var hostCss = ReadGeneratorSource(
            Path.Combine(
                "Components",
                "GenerationWorkflow",
                "ConfiguredGenerationWorkflow.razor.css"
            )
        );
        var wizardCss = ReadGeneratorSource(
            Path.Combine("Components", "Layout", "WizardShell.razor.css")
        );

        var hostColumns = ExtractChipGridColumns(hostCss, preferHostScoped: true);
        var wizardColumns = ExtractChipGridColumns(wizardCss, preferHostScoped: false);

        wizardColumns
            .Should()
            .NotBeNullOrWhiteSpace("WizardPage must declare .wizard-chip-grid columns");

        var columnsMatch =
            !string.IsNullOrWhiteSpace(hostColumns)
            && NormalizeCssValue(hostColumns!) == NormalizeCssValue(wizardColumns!);

        // Host is the Feeding Generate surface outside .wizard-page — only host ellipsis bites there.
        var hostForcesEllipsis = HostChipRuleForcesEllipsis(hostCss);

        (columnsMatch || !hostForcesEllipsis)
            .Should()
            .BeTrue(
                "ConfiguredGenerationWorkflow .wizard-chip-grid columns must match Wizard "
                    + $"(host='{hostColumns}', wizard='{wizardColumns}') "
                    + "OR host .wizard-chip must not use text-overflow:ellipsis / white-space:nowrap"
            );
    }

    private static string? ExtractChipGridColumns(string css, bool preferHostScoped)
    {
        // Prefer the host-scoped rule when present; else any .wizard-chip-grid block.
        string[] patterns = preferHostScoped
            ?
            [
                @"\.configured-generation-workflow\s+::deep\s+\.wizard-chip-grid\s*\{([^}]*)\}",
                @"\.wizard-chip-grid\s*\{([^}]*)\}",
            ]
            :
            [
                @"\.wizard-page\s+::deep\s+\.wizard-chip-grid\s*\{([^}]*)\}",
                @"\.wizard-chip-grid\s*\{([^}]*)\}",
            ];

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(css, pattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                continue;
            }

            var columns = Regex.Match(
                match.Groups[1].Value,
                @"grid-template-columns\s*:\s*([^;]+);",
                RegexOptions.IgnoreCase
            );
            if (columns.Success)
            {
                return columns.Groups[1].Value.Trim();
            }
        }

        return null;
    }

    private static bool HostChipRuleForcesEllipsis(string hostCss)
    {
        var match = Regex.Match(
            hostCss,
            @"\.configured-generation-workflow\s+::deep\s+\.wizard-chip\s*\{([^}]*)\}",
            RegexOptions.IgnoreCase
        );
        if (!match.Success)
        {
            return false;
        }

        var body = match.Groups[1].Value;
        return body.Contains("text-overflow", StringComparison.OrdinalIgnoreCase)
            && body.Contains("ellipsis", StringComparison.OrdinalIgnoreCase)
            && body.Contains("white-space", StringComparison.OrdinalIgnoreCase)
            && body.Contains("nowrap", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeCssValue(string value) =>
        Regex.Replace(value, @"\s+", " ").Trim().ToLowerInvariant();

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
