using System.Text.RegularExpressions;
using FluentAssertions;

namespace Siteswaps.Generator.Test.GenerationWorkflow;

/// <summary>
/// Round-3 review-finding repros for reusable-generation (desired Soll; no production fixes here).
/// </summary>
[TestFixture]
public class GenerationWorkflowRound3ReproTests
{
    [Test]
    public void Cancel_Button_Is_Enabled_While_IsGenerating()
    {
        // Finding #1 (High): Cancel shows only when IsGenerating but disabled="@IsGenerating" — must stay enabled/clickable.
        var hostRazor = GenerationWorkflowInvariantReproTests.ReadGeneratorSource(
            Path.Combine("Components", "GenerationWorkflow", "ConfiguredGenerationWorkflow.razor")
        );

        var cancelBlock = ExtractCancelButtonMarkup(hostRazor);
        cancelBlock
            .Should()
            .NotBeNullOrWhiteSpace("Host must render a Cancel button in the IsGenerating branch");

        var disabledByIsGenerating =
            cancelBlock!.Contains("disabled=\"@IsGenerating\"", StringComparison.Ordinal)
            || cancelBlock.Contains("disabled='@IsGenerating'", StringComparison.Ordinal)
            || Regex.IsMatch(
                cancelBlock,
                """disabled\s*=\s*["']@IsGenerating["']""",
                RegexOptions.IgnoreCase
            );

        var explicitlyEnabled =
            cancelBlock.Contains("disabled=\"false\"", StringComparison.OrdinalIgnoreCase)
            || cancelBlock.Contains("disabled='false'", StringComparison.OrdinalIgnoreCase)
            || cancelBlock.Contains("disabled=\"@false\"", StringComparison.Ordinal)
            || cancelBlock.Contains("disabled=\"@(false)\"", StringComparison.Ordinal);

        var hasDisabledAttribute = Regex.IsMatch(
            cancelBlock,
            """\bdisabled\b""",
            RegexOptions.IgnoreCase
        );

        disabledByIsGenerating
            .Should()
            .BeFalse(
                "Cancel must not use disabled=\"@IsGenerating\" — that disables it whenever it is shown"
            );

        (explicitlyEnabled || !hasDisabledAttribute)
            .Should()
            .BeTrue(
                "Cancel must stay enabled while generating (no disabled attr, or disabled=false)"
            );
    }

    [Test]
    public void Busy_State_Exposes_Spinner_Or_Aria_Status()
    {
        // Finding #2 (Medium): busy state needs spinner/aria-busy/aria-live/Generating… (unlike ResultsView).
        var hostRazor = GenerationWorkflowInvariantReproTests.ReadGeneratorSource(
            Path.Combine("Components", "GenerationWorkflow", "ConfiguredGenerationWorkflow.razor")
        );

        var hasBusyFeedback =
            hostRazor.Contains("aria-busy", StringComparison.OrdinalIgnoreCase)
            || hostRazor.Contains("aria-live", StringComparison.OrdinalIgnoreCase)
            || hostRazor.Contains("role=\"status\"", StringComparison.OrdinalIgnoreCase)
            || hostRazor.Contains("role='status'", StringComparison.OrdinalIgnoreCase)
            || hostRazor.Contains("spinner", StringComparison.OrdinalIgnoreCase)
            || hostRazor.Contains("Generating…", StringComparison.Ordinal)
            || hostRazor.Contains("Generating...", StringComparison.Ordinal)
            || Regex.IsMatch(hostRazor, """>\s*Generating""", RegexOptions.IgnoreCase);

        hasBusyFeedback
            .Should()
            .BeTrue(
                "while IsGenerating, Host must show busy feedback (spinner / aria-busy|aria-live|role=status / Generating…)"
            );
    }

    // Finding #3 (Medium residual / Strategic Concession): Wizard ChildContent shell; own generate path — no red repro this round.

    private static string? ExtractCancelButtonMarkup(string hostRazor)
    {
        // Prefer the button that calls CancelGeneration / shows Cancel label.
        var matches = Regex.Matches(
            hostRazor,
            """<button\b[\s\S]*?</button>""",
            RegexOptions.IgnoreCase
        );

        foreach (Match match in matches)
        {
            var block = match.Value;
            var looksLikeCancel =
                block.Contains("CancelGeneration", StringComparison.Ordinal)
                || (
                    block.Contains("Cancel", StringComparison.OrdinalIgnoreCase)
                    && (
                        block.Contains("onclick", StringComparison.OrdinalIgnoreCase)
                        || block.Contains("@onclick", StringComparison.OrdinalIgnoreCase)
                    )
                );

            if (looksLikeCancel)
            {
                return block;
            }
        }

        return null;
    }
}
