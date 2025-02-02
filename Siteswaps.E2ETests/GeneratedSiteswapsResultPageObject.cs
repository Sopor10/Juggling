using FluentAssertions;
using Microsoft.Playwright;

namespace Siteswaps.E2ETests;

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