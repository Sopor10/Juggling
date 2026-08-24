using System.Globalization;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace Siteswaps.Generator.Test.Feeding;

/// <summary>
/// Round-5 retest repros for Feeding (desired Soll; no production fixes here).
/// </summary>
[TestFixture]
[Category("Round5Retest")]
public class FeedingRound5RetestReproTests
{
    /// <summary>
    /// Finding (High): After Generate completes, OnWorkflowResults pops history via
    /// NavigateBackOrSetSetupAsync → history.back(), consuming the Generate entry.
    /// Browser back from post-generate Setup then leaves /feeding.
    /// Soll: Generate→Setup completion replaces the current history entry (replacePhaseState /
    /// ReplaceHistoryPhaseAsync / SetPhaseAsync push:false), and does not pop.
    /// </summary>
    [Test]
    public void Feeding_Generate_Completion_Replaces_History_Instead_Of_Popping()
    {
        var codeBehind = ReadGeneratorSource(
            Path.Combine("Components", "Feeding", "FeedingPage.razor.cs")
        );

        var onWorkflowResults = ExtractMethodBody(codeBehind, "OnWorkflowResults");
        onWorkflowResults
            .Should()
            .NotBeNullOrWhiteSpace("FeedingPage must declare OnWorkflowResults");

        var completionUsesReplace = UsesHistoryReplaceToSetup(onWorkflowResults!);
        var completionPopsViaNavigateBack =
            onWorkflowResults!.Contains("NavigateBackOrSetSetupAsync", StringComparison.Ordinal)
            && MethodInvokesHistoryBack(codeBehind, "NavigateBackOrSetSetupAsync");

        (completionUsesReplace && !completionPopsViaNavigateBack)
            .Should()
            .BeTrue(
                "OnWorkflowResults must return to Setup by replacing history "
                    + "(replacePhaseState / ReplaceHistoryPhaseAsync / SetPhaseAsync push:false), "
                    + "not by NavigateBackOrSetSetupAsync → history.back() which consumes the "
                    + "Generate entry so the next browser back leaves /feeding"
            );
    }

    /// <summary>
    /// Finding (Medium): Generate→Setup (BackToSetup / Cancel / popstate) leaves focus on body.
    /// Soll: Returning to Setup restores focus (heading, lead, or primary CTA).
    /// </summary>
    [Test]
    public void Feeding_Generate_To_Setup_Restores_Focus()
    {
        var codeBehind = ReadGeneratorSource(
            Path.Combine("Components", "Feeding", "FeedingPage.razor.cs")
        );

        var backToSetup = ExtractMethodBody(codeBehind, "BackToSetup");
        var onBrowserPopState = ExtractMethodBody(codeBehind, "OnBrowserPopState");
        var navigateBackOrSetSetup = ExtractMethodBody(codeBehind, "NavigateBackOrSetSetupAsync");
        var onWorkflowResults = ExtractMethodBody(codeBehind, "OnWorkflowResults");

        backToSetup.Should().NotBeNullOrWhiteSpace("FeedingPage must declare BackToSetup");
        onBrowserPopState
            .Should()
            .NotBeNullOrWhiteSpace("FeedingPage must declare OnBrowserPopState");

        var restoresFocus =
            InvokesFocusRestore(backToSetup!)
            || InvokesFocusRestore(onBrowserPopState!)
            || InvokesFocusRestore(navigateBackOrSetSetup ?? string.Empty)
            || InvokesFocusRestore(onWorkflowResults ?? string.Empty);

        restoresFocus
            .Should()
            .BeTrue(
                "BackToSetup, OnBrowserPopState, and/or the Setup-return path must restore focus "
                    + "(Focus*Async / FocusAsync / focusElement) so Generate→Setup does not leave "
                    + "focus on body"
            );
    }

    /// <summary>
    /// Finding (Medium): feeding-btn-primary is 44px / inherit font vs wizard-btn-generate
    /// 48px / display font — measurable in CSS.
    /// Soll: Primary CTA min-height and font-family align with Wizard generate.
    /// </summary>
    [Test]
    public void Feeding_Primary_Cta_Height_And_Font_Align_With_Wizard()
    {
        var feedingCss = ReadGeneratorSource(
            Path.Combine("Components", "Feeding", "FeedingPage.razor.css")
        );
        var wizardCss = ReadGeneratorSource(
            Path.Combine("Components", "Layout", "WizardShell.razor.css")
        );

        var feedingPrimary = ExtractRuleBodiesContaining(feedingCss, ".feeding-btn-primary");
        var wizardGenerate = ExtractRuleBodiesContaining(wizardCss, ".wizard-btn-generate");

        feedingPrimary.Should().NotBeNullOrWhiteSpace("expected .feeding-btn-primary rule");
        wizardGenerate.Should().NotBeNullOrWhiteSpace("expected .wizard-btn-generate rules");

        var feedingMinHeight = ParsePx(CssDecl(feedingPrimary!, "min-height"));
        var wizardMinHeight = ParsePx(CssDecl(wizardGenerate!, "min-height"));

        var heightAligned =
            feedingMinHeight is >= 48
            && wizardMinHeight is not null
            && feedingMinHeight == wizardMinHeight;

        var feedingFont = NormalizeCssValue(CssDecl(feedingPrimary!, "font-family"));
        var wizardFont = NormalizeCssValue(CssDecl(wizardGenerate!, "font-family"));
        var fontAligned =
            !string.IsNullOrEmpty(feedingFont)
            && feedingFont == wizardFont
            && !feedingFont.Contains("inherit", StringComparison.Ordinal);

        (heightAligned && fontAligned)
            .Should()
            .BeTrue(
                "feeding-btn-primary must match wizard-btn-generate min-height (≥48) and "
                    + "font-family (display stack, not inherit). "
                    + $"feeding min-height={feedingMinHeight?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "missing"}px, "
                    + $"wizard min-height={wizardMinHeight?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "missing"}px, "
                    + $"feeding font='{feedingFont}', wizard font='{wizardFont}'"
            );
    }

    private static bool UsesHistoryReplaceToSetup(string methodBody) =>
        Regex.IsMatch(
            methodBody,
            @"ReplaceHistoryPhaseAsync\s*\(\s*FeedingPhase\.Setup|replacePhaseState|"
                + @"SetPhaseAsync\s*\(\s*FeedingPhase\.Setup\s*,\s*(?:false|push\s*:\s*false)",
            RegexOptions.IgnoreCase
        );

    private static bool MethodInvokesHistoryBack(string codeBehind, string methodName)
    {
        var body = ExtractMethodBody(codeBehind, methodName);
        return body is not null
            && Regex.IsMatch(
                body,
                @"InvokeVoidAsync\s*\(\s*""back""\s*\)",
                RegexOptions.IgnoreCase
            );
    }

    private static bool InvokesFocusRestore(string methodBody) =>
        Regex.IsMatch(
            methodBody,
            @"Focus\w*Async|FocusAsync|focusElement",
            RegexOptions.IgnoreCase
        );

    private static string? ExtractMethodBody(string source, string methodName)
    {
        var match = Regex.Match(
            source,
            $@"(?:private|public|protected)\s+(?:async\s+)?(?:Task|ValueTask|void)\s+{Regex.Escape(methodName)}\s*\([^)]*\)\s*\{{",
            RegexOptions.IgnoreCase
        );
        if (!match.Success)
        {
            return null;
        }

        var start = match.Index + match.Length - 1;
        var depth = 0;
        for (var i = start; i < source.Length; i++)
        {
            if (source[i] == '{')
            {
                depth++;
            }
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return source[(start + 1)..i];
                }
            }
        }

        return null;
    }

    private static string? ExtractRuleBodiesContaining(string css, string classFragment)
    {
        var bodies = new List<string>();
        foreach (Match match in Regex.Matches(css, @"([^{}]+)\{([^{}]*)\}"))
        {
            var selector = match.Groups[1].Value;
            if (
                selector.Contains(classFragment, StringComparison.OrdinalIgnoreCase)
                && !selector.TrimStart().StartsWith('@')
            )
            {
                bodies.Add(match.Groups[2].Value);
            }
        }

        return bodies.Count == 0 ? null : string.Join('\n', bodies);
    }

    private static string? CssDecl(string ruleBody, string property)
    {
        var match = Regex.Match(
            ruleBody,
            $@"\b{Regex.Escape(property)}\s*:\s*([^;]+);",
            RegexOptions.IgnoreCase
        );
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static int? ParsePx(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var match = Regex.Match(value, @"(\d+(?:\.\d+)?)\s*px", RegexOptions.IgnoreCase);
        return
            match.Success
            && double.TryParse(
                match.Groups[1].Value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var px
            )
            ? (int)Math.Round(px)
            : null;
    }

    private static string NormalizeCssValue(string? value) =>
        Regex.Replace(value ?? string.Empty, @"\s+", " ").Trim().ToLowerInvariant();

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
