using System.Runtime.CompilerServices;
using VerifyTests.Bunit;
using Bunit;
using Siteswaps.Generator.Components.Internal;
using VerifyTests.AngleSharp;

namespace Siteswaps.Components.Tests;

public abstract class BunitTestContext : TestContextWrapper
{
    [SetUp]
    public void Setup()
    {
        TestContext = new Bunit.TestContext();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [TearDown]
    public void TearDown() => TestContext?.Dispose();
}

public class NormalTests : BunitTestContext
{

    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyBunit.Initialize();
        // remove some noise from the html snapshot
        VerifierSettings.ScrubEmptyLines();
        BlazorScrubber.ScrubCommentLines();
        HtmlPrettyPrint.All(nodes => nodes.ScrubAttributes("id"));
        HtmlPrettyPrint.All();
        VerifierSettings.ScrubLinesWithReplace(s =>
        {
            var scrubbed = s.Replace("<!--!-->", "");
            if (string.IsNullOrWhiteSpace(scrubbed))
            {
                return null;
            }

            return scrubbed;
        });
        VerifierSettings.ScrubLinesContaining("<script src=\"_framework/dotnet.");
        VerifierSettings.ScrubInlineGuids();
    }

    [Test]
    public async Task Test()
    {
        var cut = RenderComponent<RangeInput>();

        await Verify(cut);
    }
}