using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Siteswaps.E2ETests;

public class GenerateSiteswapsTests: E2ETestsBase
{
    [Test]
    public async Task Standard_Values_Should_Generate_Siteswap_aa753()
    {
        await Page.GotoAsync(ExpertUi);

        await Page.ClickAsync("#generate");

        var result = await Page.Locator("#passist-link-aa753").CountAsync();
        result.Should().Be(1);
    }
    
    [Test]
    public async Task Clicking_On_A_Siteswap_Link_Opens_Passist_In_A_New_Tab()
    {
        await Page.GotoAsync(ExpertUi);

        await Page.ClickAsync("#generate");

        
        var popup = await Context.RunAndWaitForPageAsync(async () =>
        {
            await Page.Locator("#passist-link-aa753").ClickAsync();
        });

        var title = await popup.TitleAsync();
        title.Should().Contain("passist");
    }
        
    [Test]
    public async Task EasyUI_Generates_Siteswaps()
    {
        await Page.GotoAsync(BaseUrl);

        await Page.ClickAsync("#generate");
        await Page.WaitForSelectorAsync("#passist-link-aaa00");
        (await Page.Locator("#passist-link-aaa00").CountAsync()).Should().Be(1);
        
    }
}