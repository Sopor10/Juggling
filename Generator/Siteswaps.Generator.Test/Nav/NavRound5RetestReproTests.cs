using System.Globalization;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace Siteswaps.Generator.Test.Nav;

/// <summary>
/// Round-5 retest repros for shared nav touch targets (desired Soll; no production fixes here).
/// </summary>
[TestFixture]
[Category("Round5Retest")]
public class NavRound5RetestReproTests
{
    /// <summary>
    /// Finding (Medium): Nav search submit and Settings link touch targets are under 44px
    /// (search bar height 2.4rem; submit has no min-height; link dips to 2.4rem on small screens).
    /// Soll: .pznav-search-submit and every .pznav-link rule declare min-height ≥ 44px.
    /// </summary>
    [Test]
    public void Nav_Search_And_Settings_Meet_44px_Touch_Target()
    {
        var css = ReadWebassemblySource(Path.Combine("Shared", "NavMenu.razor.css"));

        var searchSubmitBodies = ExtractAllRuleBodiesContaining(css, ".pznav-search-submit");
        var linkBodies = ExtractAllRuleBodiesContaining(css, ".pznav-link");

        searchSubmitBodies.Should().NotBeEmpty("expected .pznav-search-submit rule");
        linkBodies.Should().NotBeEmpty("expected .pznav-link rule (Settings)");

        var submitHeights = searchSubmitBodies
            .Select(EffectiveMinHeightPx)
            .Where(h => h is not null)
            .Select(h => h!.Value)
            .ToList();
        var linkHeights = linkBodies
            .Select(EffectiveMinHeightPx)
            .Where(h => h is not null)
            .Select(h => h!.Value)
            .ToList();

        var worstSubmit = submitHeights.Count == 0 ? (int?)null : submitHeights.Min();
        var worstLink = linkHeights.Count == 0 ? (int?)null : linkHeights.Min();

        (worstSubmit >= 44 && worstLink >= 44)
            .Should()
            .BeTrue(
                "Nav search submit and Settings (.pznav-link) must expose ≥44px touch targets "
                    + $"(search-submit worst min-height={FormatPx(worstSubmit)}, "
                    + $"pznav-link worst min-height={FormatPx(worstLink)})"
            );
    }

    private static int? EffectiveMinHeightPx(string ruleBody)
    {
        var minHeight = ParseLengthPx(CssDecl(ruleBody, "min-height"));
        if (minHeight is not null)
        {
            return minHeight;
        }

        return ParseLengthPx(CssDecl(ruleBody, "height"));
    }

    private static string FormatPx(int? px) => px is null ? "missing" : $"{px}px";

    private static List<string> ExtractAllRuleBodiesContaining(string css, string classFragment)
    {
        var bodies = new List<string>();
        var token = Regex.Escape(classFragment) + @"\b";
        foreach (Match match in Regex.Matches(css, @"([^{}]+)\{([^{}]*)\}"))
        {
            var selector = match.Groups[1].Value;
            if (
                Regex.IsMatch(selector, token, RegexOptions.IgnoreCase)
                && !selector.TrimStart().StartsWith('@')
            )
            {
                bodies.Add(match.Groups[2].Value);
            }
        }

        return bodies;
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

    private static int? ParseLengthPx(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var px = Regex.Match(value, @"(\d+(?:\.\d+)?)\s*px", RegexOptions.IgnoreCase);
        if (
            px.Success
            && double.TryParse(
                px.Groups[1].Value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var pxVal
            )
        )
        {
            return (int)Math.Round(pxVal);
        }

        var rem = Regex.Match(value, @"(\d+(?:\.\d+)?)\s*rem", RegexOptions.IgnoreCase);
        if (
            rem.Success
            && double.TryParse(
                rem.Groups[1].Value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var remVal
            )
        )
        {
            return (int)Math.Round(remVal * 16);
        }

        return null;
    }

    private static string ReadWebassemblySource(string relativePathUnderWebassembly) =>
        File.ReadAllText(
            Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "..",
                "..",
                "..",
                "..",
                "..",
                "Webassembly",
                relativePathUnderWebassembly
            )
        );
}
