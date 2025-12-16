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

    public async Task<int> GetSiteswapCountAsync()
    {
        return await page.GetByTestId(
                new System.Text.RegularExpressions.Regex("generated-siteswap-.*")
            )
            .CountAsync();
    }

    public async Task ShouldContainSiteswapAsync(string siteswap)
    {
        var element = page.GetByTestId($"generated-siteswap-{siteswap}");
        await element.WaitForAsync();
        var count = await element.CountAsync();
        count.Should().BeGreaterThan(0);
    }

    public async Task ShouldGenerateAtLeastAsync(int minimumCount)
    {
        var count = await GetSiteswapCountAsync();
        count.Should().BeGreaterOrEqual(minimumCount);
    }

    public async Task WaitForGenerationToCompleteAsync()
    {
        await page.WaitForSelectorAsync("text=Finished...", new() { Timeout = 60000 });
    }
}
