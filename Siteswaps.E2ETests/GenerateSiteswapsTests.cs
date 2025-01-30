using CliWrap;
using FluentAssertions;
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

        var generator = await page.OpenGeneratorAsync(Fixture.RootUri.AbsoluteUri);
        var result = await generator.GenerateSiteswapsAsync();
        await result.WaitForSiteswapAsync("aaa00");
    }

    [Fact]
    public async Task Can_Add_Pattern_Filter()
    {
        var page = await Fixture.Context!.NewPageAsync();

        var generator = await page.OpenGeneratorAsync(Fixture.RootUri.AbsoluteUri);
        var dialog = await generator.AddPatternFilterAsync();
        await dialog.AddThrowAsync("Self");
        await dialog.AddFilterAsync();
        var result = await generator.GenerateSiteswapsAsync();
        await result.WaitForSiteswapAsync("a7562");
        await result.ShoulNotGenerateAsync("aaa00");
    }
}

public static class PageExtensions
{
    public static async Task<GeneratorPageObject> OpenGeneratorAsync(this IPage page, string uri)
    {
        await page.GotoAsync(uri);
        return new(page);
    }
}

public class GeneratorPageObject(IPage page)
{
    public async Task<GeneratedSiteswapsResultPageObject> GenerateSiteswapsAsync()
    {
        await page.GetByTestId("generate-button").ClickAsync();
        return new(page);
    }

    public async Task<PatternFilterDialogObject> AddPatternFilterAsync()
    {
        await page.GetByTestId("filter-node-AND").ClickAsync();
        await page.GetByText("Add Filter").ClickAsync();
        return new(page);
    }
}

public class GeneratedSiteswapsResultPageObject(IPage page)
{
    public async Task WaitForSiteswapAsync(string siteswap)
    {
        await page.GetByTestId($"generated-siteswap-{siteswap}").WaitForAsync();
    }

    public async Task ShoulNotGenerateAsync(string siteswap)
    {
        var count = await page.GetByTestId($"generated-siteswap-{siteswap}").CountAsync();
        count.Should().Be(0);
    }
}

public class PatternFilterDialogObject(IPage page)
{
    public async Task<PatternFilterDialogObject> AddThrowAsync(string name)
    {
        await page.GetByTestId($"add-throw-{name}").ClickAsync();
        return this;
    }

    public async Task<GeneratorPageObject> AddFilterAsync()
    {
        await page.GetByTestId("add-pattern-filter").ClickAsync();
        return new(page);
    }
}
