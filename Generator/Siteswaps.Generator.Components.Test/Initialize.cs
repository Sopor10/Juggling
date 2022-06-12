using VerifyTests.AngleSharp;

namespace Siteswaps.Generator.Components.Test;

[SetUpFixture]
public class Initialize
{
    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        VerifyBunit.Initialize();
        // remove some noise from the html snapshot
        VerifierSettings.ScrubEmptyLines();
        VerifierSettings.ScrubLinesWithReplace(s => s.Replace("<!--!-->", ""));
        HtmlPrettyPrint.All();
        VerifierSettings.ScrubLinesContaining("<script src=\"_framework/dotnet.");
    }
}