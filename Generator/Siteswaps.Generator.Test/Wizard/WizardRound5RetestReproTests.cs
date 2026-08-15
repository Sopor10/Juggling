using System.Text.RegularExpressions;
using FluentAssertions;

namespace Siteswaps.Generator.Test.Wizard;

/// <summary>
/// Round-5 retest repros for Wizard (desired Soll; no production fixes here).
/// </summary>
[TestFixture]
[Category("Round5Retest")]
public class WizardRound5RetestReproTests
{
    /// <summary>
    /// Finding (Medium): Fast browser-back during Re-Generate (cache / MinSpinner delay)
    /// desyncs history vs UI — StartGenerationAsync still commits WizardPhase.Results after
    /// cancel/popstate cleared Generating.
    /// Soll: Every transition to Results aborts when generation was cancelled (or phase left
    /// Generating), including the cache-hit + Delay path.
    /// </summary>
    [Test]
    public void Wizard_StartGeneration_Does_Not_Commit_Results_After_Cancel_Or_Pop()
    {
        var codeBehind = ReadGeneratorSource(
            Path.Combine("Components", "WizardPage", "WizardPage.razor.cs")
        );

        var startGeneration = ExtractMethodBody(codeBehind, "StartGenerationAsync");
        startGeneration
            .Should()
            .NotBeNullOrWhiteSpace("WizardPage must declare StartGenerationAsync");

        var cacheHitCommitsUnguarded = CacheHitCommitsResultsWithoutCancelGuard(startGeneration!);
        var streamPathCommitsAfterCancelFlag = StreamPathCommitsResultsAfterWasCancelled(
            startGeneration!
        );

        (cacheHitCommitsUnguarded || streamPathCommitsAfterCancelFlag)
            .Should()
            .BeFalse(
                "StartGenerationAsync must not set WizardPhase.Results after cancel/popstate: "
                    + "cache-hit path needs a cancel/phase guard after MinSpinner Delay, and the "
                    + "stream path must not assign Results after WasCancelled=true. "
                    + $"cacheHitUnguarded={cacheHitCommitsUnguarded}, "
                    + $"streamCommitsAfterCancel={streamPathCommitsAfterCancelFlag}"
            );
    }

    private static bool CacheHitCommitsResultsWithoutCancelGuard(string methodBody)
    {
        // Cache-hit block: from TryLoadCachedResultsAsync success through its return.
        var cacheHit = Regex.Match(
            methodBody,
            @"if\s*\(\s*await\s+TryLoadCachedResultsAsync[\s\S]*?return;",
            RegexOptions.IgnoreCase
        );
        if (!cacheHit.Success)
        {
            return false;
        }

        var block = cacheHit.Value;
        if (!block.Contains("WizardPhase.Results", StringComparison.Ordinal))
        {
            return false;
        }

        // Text between the last awaitable Delay (MinSpinner) and Phase=Results must guard cancel.
        var delayToResults = Regex.Match(
            block,
            @"Task\.Delay\([^)]+\)([\s\S]*?)State\.Phase\s*=\s*WizardPhase\.Results",
            RegexOptions.IgnoreCase
        );
        var window = delayToResults.Success ? delayToResults.Groups[1].Value : block;

        var guarded = Regex.IsMatch(
            window,
            @"IsCancellationRequested|WasCancelled|_isStartingGeneration|Phase\s*==\s*WizardPhase\.Generating|Phase\s*!=\s*WizardPhase\.Generating",
            RegexOptions.IgnoreCase
        );

        return !guarded;
    }

    private static bool StreamPathCommitsResultsAfterWasCancelled(string methodBody) =>
        Regex.IsMatch(
            methodBody,
            @"WasCancelled\s*=\s*true[\s\S]{0,200}?State\.Phase\s*=\s*WizardPhase\.Results",
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
