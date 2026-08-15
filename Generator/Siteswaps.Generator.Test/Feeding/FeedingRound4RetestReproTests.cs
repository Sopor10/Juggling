using System.Text.RegularExpressions;
using System.Xml.Linq;
using FluentAssertions;

namespace Siteswaps.Generator.Test.Feeding;

/// <summary>
/// Round-4 retest repros for Feeding (desired Soll; no production fixes here).
/// </summary>
[TestFixture]
public class FeedingRound4RetestReproTests
{
    private const string B1ReadyLeadKey = "B1 is ready. Pick a local pattern, then generate B2.";

    /// <summary>
    /// Finding (Medium): After B1, OnWorkflowResults auto-selects locals[0], but lead copy
    /// still urges Pick/Choose/wähle as if nothing were selected.
    /// Soll: Lead acknowledges selected state (no pick/choose verb while auto-select),
    /// OR auto-select is removed, OR SetupLeadText branches on SelectedSiteswap.
    /// </summary>
    [Test]
    public void Feeding_After_B1_Lead_Acknowledges_Selected_State_When_Auto_Selecting()
    {
        var codeBehind = ReadGeneratorSource(
            Path.Combine("Components", "Feeding", "FeedingPage.razor.cs")
        );
        var enResx = ReadGeneratorSource(Path.Combine("Components", "Feeding", "FeedingPage.resx"));
        var deResx = ReadGeneratorSource(
            Path.Combine("Components", "Feeding", "FeedingPage.de.resx")
        );

        var autoSelectsFirstB1 = Regex.IsMatch(
            codeBehind,
            @"OnWorkflowResults[\s\S]*?SelectSiteswap\s*\(\s*""B1""\s*,\s*locals\s*\[\s*0\s*\]",
            RegexOptions.IgnoreCase
        );

        autoSelectsFirstB1
            .Should()
            .BeTrue(
                "precondition: OnWorkflowResults still auto-selects the first B1 local — "
                    + "otherwise this consistency repro does not apply"
            );

        var setupLeadBody = ExtractSetupLeadTextBody(codeBehind);
        setupLeadBody.Should().NotBeNullOrWhiteSpace("FeedingPage must declare SetupLeadText");

        var leadBranchesOnSelection = setupLeadBody!.Contains(
            "SelectedSiteswap",
            StringComparison.Ordinal
        );

        var enLead = ResxValue(enResx, B1ReadyLeadKey) ?? B1ReadyLeadKey;
        var deLead = ResxValue(deResx, B1ReadyLeadKey) ?? string.Empty;
        var leadUrgesManualPick =
            ContainsPickVerb(enLead)
            || ContainsPickVerb(deLead)
            || ContainsPickVerb(B1ReadyLeadKey);

        (leadBranchesOnSelection || !leadUrgesManualPick)
            .Should()
            .BeTrue(
                "With B1 auto-select, SetupLeadText must acknowledge selection "
                    + "(branch on SelectedSiteswap and/or drop Pick/Choose/wähle from the B1-ready lead). "
                    + $"EN='{enLead}', DE='{deLead}'"
            );
    }

    /// <summary>
    /// Finding (High/Medium): feeding-btn-primary / feeding-chip look ≠ wizard-btn-generate /
    /// wizard-chip (radius + primary/active color) — measurable in CSS.
    /// Soll: Feeding primary CTA and chip active tokens align with Wizard generate/chip.
    /// </summary>
    [Test]
    public void Feeding_Primary_And_Chip_Tokens_Align_With_Wizard()
    {
        var feedingCss = ReadGeneratorSource(
            Path.Combine("Components", "Feeding", "FeedingPage.razor.css")
        );
        var wizardCss = ReadGeneratorSource(
            Path.Combine("Components", "WizardPage", "WizardPage.razor.css")
        );

        var feedingPrimary = ExtractRuleBodyContaining(feedingCss, ".feeding-btn-primary");
        var wizardGenerate = ExtractRuleBodiesContaining(wizardCss, ".wizard-btn-generate");
        var feedingChipActive = ExtractRuleBodyContaining(feedingCss, ".feeding-chip.active");
        var wizardChipActive = ExtractRuleBodyContaining(wizardCss, ".wizard-chip.active");

        feedingPrimary.Should().NotBeNullOrWhiteSpace("expected .feeding-btn-primary rule");
        wizardGenerate.Should().NotBeNullOrWhiteSpace("expected .wizard-btn-generate rules");
        feedingChipActive.Should().NotBeNullOrWhiteSpace("expected .feeding-chip.active rule");
        wizardChipActive.Should().NotBeNullOrWhiteSpace("expected .wizard-chip.active rule");

        var feedingRadius = CssDecl(feedingPrimary!, "border-radius");
        var wizardRadius = CssDecl(wizardGenerate!, "border-radius");
        var primaryRadiusAligned =
            NormalizeCssValue(feedingRadius) == NormalizeCssValue(wizardRadius);

        var primaryUsesWizardOrangeFamily =
            UsesOrangeAccent(feedingPrimary!) && UsesOrangeAccent(wizardGenerate!);

        var feedingChipBg = CssDecl(feedingChipActive!, "background");
        var wizardChipBg = CssDecl(wizardChipActive!, "background");
        var chipActiveAligned =
            NormalizeCssValue(feedingChipBg) == NormalizeCssValue(wizardChipBg)
            || (UsesPurpleActive(feedingChipActive!) && UsesPurpleActive(wizardChipActive!));

        (primaryRadiusAligned && primaryUsesWizardOrangeFamily && chipActiveAligned)
            .Should()
            .BeTrue(
                "feeding-btn-primary must match wizard-btn-generate (border-radius + orange CTA), "
                    + "and feeding-chip.active must match wizard-chip.active color family. "
                    + $"feeding-primary radius='{feedingRadius}', "
                    + $"wizard-generate radius='{wizardRadius}', "
                    + $"feeding-chip.active bg='{feedingChipBg}', "
                    + $"wizard-chip.active bg='{wizardChipBg}'"
            );
    }

    /// <summary>
    /// Finding (Medium): Browser back from Feeding Generate/Results leaves /feeding entirely —
    /// phases are not on the history stack (unlike Wizard).
    /// Soll: Feeding phase changes push/replace history so browser back returns to prior phase.
    /// </summary>
    [Test]
    public void Feeding_Phase_Changes_Participate_In_Browser_History()
    {
        var codeBehind = ReadGeneratorSource(
            Path.Combine("Components", "Feeding", "FeedingPage.razor.cs")
        );
        var razor = ReadGeneratorSource(Path.Combine("Components", "Feeding", "FeedingPage.razor"));
        var jsPath = GeneratorProjectPath(
            Path.Combine("Components", "Feeding", "FeedingPage.razor.js")
        );
        var js = File.Exists(jsPath) ? File.ReadAllText(jsPath) : string.Empty;

        var surface = string.Join('\n', codeBehind, razor, js);
        var wiresHistory =
            Regex.IsMatch(
                surface,
                @"push(Editor|Results)?State|initHistory|replace(Editor)?State|history\.pushState|Navigation\.NavigateTo",
                RegexOptions.IgnoreCase
            ) || surface.Contains("LocationChanged", StringComparison.Ordinal);

        wiresHistory
            .Should()
            .BeTrue(
                "FeedingPage must push/replace browser history on GenerateB1/GenerateB2/Results "
                    + "phase changes (or handle popstate), so browser back stays inside the feeding flow"
            );
    }

    private static string? ExtractSetupLeadTextBody(string codeBehind)
    {
        var match = Regex.Match(
            codeBehind,
            @"SetupLeadText\s*=>\s*([\s\S]*?);",
            RegexOptions.IgnoreCase
        );
        return match.Success ? match.Groups[1].Value : null;
    }

    private static bool ContainsPickVerb(string text) =>
        Regex.IsMatch(
            text,
            @"\b(pick|choose|wähle|waehle)\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
        );

    private static string? ResxValue(string resxXml, string name)
    {
        var doc = XDocument.Parse(resxXml);
        return doc
            .Root?.Elements("data")
            .FirstOrDefault(e =>
                string.Equals((string?)e.Attribute("name"), name, StringComparison.Ordinal)
            )
            ?.Element("value")
            ?.Value;
    }

    private static string? ExtractRuleBodyContaining(string css, string classFragment) =>
        ExtractRuleBodiesContaining(css, classFragment);

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

    private static bool UsesOrangeAccent(string ruleBody) =>
        Regex.IsMatch(
            ruleBody,
            @"#f9a500|#ffb838|wizard-orange|feeding-orange|linear-gradient",
            RegexOptions.IgnoreCase
        );

    private static bool UsesPurpleActive(string ruleBody) =>
        Regex.IsMatch(
            ruleBody,
            @"#3c286d|wizard-purple-700|feeding-purple-700|feeding-purple-600",
            RegexOptions.IgnoreCase
        );

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
