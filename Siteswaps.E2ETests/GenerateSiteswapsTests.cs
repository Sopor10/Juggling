using CliWrap;
using Microsoft.Playwright;
using Xunit;

namespace Siteswaps.E2ETests;

public class GenerateSiteswapsTests : IClassFixture<BlazorTest>
{
    public BlazorTest Fixture { get; }

    public GenerateSiteswapsTests(BlazorTest fixture)
    {
        Fixture = fixture;
    }

    [Fact]
    public async Task Generate_Siteswaps_In_Default_Conditions()
    {
        var page = await Fixture.Context!.NewPageAsync();

        await page.GotoAsync(Fixture.RootUri.AbsoluteUri);

        await page.GetByRole(AriaRole.Button, new() { Name = "Generate" }).ClickAsync();

        await page.Locator("//h3[normalize-space()='Siteswaps']").WaitForAsync();
    }
}
