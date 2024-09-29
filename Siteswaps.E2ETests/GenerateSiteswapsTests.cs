using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Playwright;
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
    
    [Test]
    public async Task Fill_Out_Easy_Filter()
    {
        await Page.GotoAsync(BaseUrl);

        await Page.Locator("#numeric-input-period").GetByRole(AriaRole.Textbox).FillAsync("6");

        await Page.Locator("#numeric-input-period").GetByRole(AriaRole.Textbox).PressAsync("Enter");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Add Filter" }).ClickAsync();

        await Page.Locator(".rz-chkbox-icon").First.ClickAsync();

        await Page.Locator("#pattern-filter-dropdown-0").ClickAsync();

        await Page.GetByRole(AriaRole.Option, new() { Name = "Self", Exact = true}).ClickAsync();

        await Page.Locator("#pattern-filter-dropdown-1").ClickAsync();

        await Page.GetByRole(AriaRole.Option, new() { Name = "Heff" }).ClickAsync();

        await Page.Locator("#pattern-filter-dropdown-2").ClickAsync();


        await Page.GetByRole(AriaRole.Option, new() { Name = "Zap" }).ClickAsync();

        await Page.GetByLabel("New Pattern Filter").GetByRole(AriaRole.Button, new() { Name = "Add Filter" }).ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Generate" }).ClickAsync();
        
        await Page.Locator("//h3[normalize-space()='Siteswaps']").WaitForAsync();


    }
    
}
