using System.Text.RegularExpressions;
using FluentAssertions;

namespace Siteswaps.Generator.Test.Wizard;

/// <summary>
/// Round-4 retest repros for Wizard (desired Soll; no production fixes here).
/// </summary>
[TestFixture]
public class WizardRound4RetestReproTests
{
    /// <summary>
    /// Finding (Medium): Results → Back leaves focus on body — BackToEditing / popstate
    /// do not call FocusActiveStepHeadingAsync (unlike Advance/GoBack/Jump).
    /// Soll: Leaving Results restores focus to the active editing step heading.
    /// </summary>
    [Test]
    public void Wizard_Results_Back_Focuses_Active_Step_Heading()
    {
        var codeBehind = ReadGeneratorSource(
            Path.Combine("Components", "WizardPage", "WizardPage.razor.cs")
        );

        var backToEditing = ExtractMethodBody(codeBehind, "BackToEditing");
        var onBrowserPopState = ExtractMethodBody(codeBehind, "OnBrowserPopState");

        backToEditing.Should().NotBeNullOrWhiteSpace("WizardPage must declare BackToEditing");
        onBrowserPopState
            .Should()
            .NotBeNullOrWhiteSpace("WizardPage must declare OnBrowserPopState");

        var restoresFocus =
            InvokesFocusActiveStepHeading(backToEditing!)
            || InvokesFocusActiveStepHeading(onBrowserPopState!);

        restoresFocus
            .Should()
            .BeTrue(
                "Results Back (BackToEditing and/or OnBrowserPopState when returning to Editing) "
                    + "must call FocusActiveStepHeadingAsync so focus does not remain on body"
            );
    }

    /// <summary>
    /// Finding (Medium): ProgressDots hit target is 40×40 CSS px (&lt; 44px touch guideline).
    /// Soll: .wizard-dot min-height and min-width are at least 44px.
    /// </summary>
    [Test]
    public void Wizard_ProgressDots_Meet_44px_Touch_Target()
    {
        var css = ReadGeneratorSource(
            Path.Combine("Components", "WizardPage", "WizardPage.razor.css")
        );

        var dotRule = ExtractRuleBody(css, @"\.wizard-page\s+::deep\s+\.wizard-dot\b(?![-\w.])");
        dotRule.Should().NotBeNullOrWhiteSpace("expected .wizard-dot rule");

        var minHeight = ParsePx(CssDecl(dotRule!, "min-height"));
        var minWidth = ParsePx(CssDecl(dotRule!, "min-width"));

        (minHeight >= 44 && minWidth >= 44)
            .Should()
            .BeTrue(
                ".wizard-dot must expose ≥44px touch target "
                    + $"(min-height={minHeight?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "missing"}px, "
                    + $"min-width={minWidth?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "missing"}px)"
            );
    }

    /// <summary>
    /// Finding (Medium): SiteswapCard title link measures ~27px tall (no min-height).
    /// Soll: .pz-siteswap-card-title declares min-height ≥ 44px for a usable touch target.
    /// </summary>
    [Test]
    public void Wizard_SiteswapCard_Title_Meets_44px_Touch_Target()
    {
        var css = ReadGeneratorSource(
            Path.Combine("Components", "WizardPage", "WizardPage.razor.css")
        );

        var titleRule = ExtractRuleBody(
            css,
            @"\.wizard-page\s+::deep\s+\.pz-siteswap-card-title\b(?![-\w])"
        );
        titleRule
            .Should()
            .NotBeNullOrWhiteSpace("expected .pz-siteswap-card-title rule under .wizard-page");

        var minHeight = ParsePx(CssDecl(titleRule!, "min-height"));

        (minHeight >= 44)
            .Should()
            .BeTrue(
                ".pz-siteswap-card-title must declare min-height ≥ 44px "
                    + $"(found {minHeight?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "none"})"
            );
    }

    private static bool InvokesFocusActiveStepHeading(string methodBody) =>
        methodBody.Contains("FocusActiveStepHeadingAsync", StringComparison.Ordinal);

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

    private static string? ExtractRuleBody(string css, string selectorPattern)
    {
        var match = Regex.Match(css, selectorPattern + @"\s*\{([^}]*)\}", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
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
        return match.Success && double.TryParse(match.Groups[1].Value, out var px)
            ? (int)Math.Round(px)
            : null;
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
