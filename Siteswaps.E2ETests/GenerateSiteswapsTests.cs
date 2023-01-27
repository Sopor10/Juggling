using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Siteswaps.E2ETests;

public class GenerateSiteswapsTests: E2ETestsBase
{
    [Test]
    public async Task Generator_UI_Generates_Siteswaps()
    {
        await Page.GotoAsync(ExpertUi);

        await Page.ClickAsync("#generate");

        await Page.Locator("//h3[normalize-space()='Siteswaps']").WaitForAsync();
    }
        
    [Test]
    public async Task EasyUI_Generates_Siteswaps()
    {
        await Page.GotoAsync(BaseUrl);

        await Page.ClickAsync("#generate");
        
        await Page.Locator("//h3[normalize-space()='Siteswaps']").WaitForAsync();

    }
}