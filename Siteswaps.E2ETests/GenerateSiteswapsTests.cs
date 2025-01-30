using CliWrap;
using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests;

public class GenerateSiteswapsTests : IClassFixture<BlazorWebassemblyFixture<Program>>
{
    public BlazorWebassemblyFixture<Program> Fixture { get; }

    public GenerateSiteswapsTests(BlazorWebassemblyFixture<Program> fixture)
    {
        Fixture = fixture;
    }

    [Fact]
    public async Task Generate_Siteswaps_In_Default_Conditions()
    {
        var page = await Fixture.Context!.NewPageAsync();

        var generator = await page.OpenGenerator(Fixture.RootUri.AbsoluteUri);
        var result = await generator.GenerateSiteswaps();
        await result.WaitForSiteswap("aaa00");
    }
}

public static class PageExtensions
{
    public static async Task<GeneratorPageObject> OpenGenerator(this IPage page, string uri)
    {
        await page.GotoAsync(uri);
        return new GeneratorPageObject(page);
    }
}
    

public class GeneratorPageObject(IPage page)
{
    public async Task<GeneratedSiteswapsResultPageObject> GenerateSiteswaps()
    {
        await page.GetByTestId("generate-button").ClickAsync();
        return new GeneratedSiteswapsResultPageObject(page);
    }
}

public class GeneratedSiteswapsResultPageObject(IPage page)
{
    public async Task WaitForSiteswap(string siteswap)
    {
        await page.GetByTestId($"generated-siteswap-{siteswap}").WaitForAsync();
    }
}