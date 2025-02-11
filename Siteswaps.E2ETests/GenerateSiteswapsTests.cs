﻿using CliWrap;
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
        await generator.AddPatternFilterAsync(async x =>
        {
            await x.AddThrowAsync("Self");
            await x.AddFilterAsync();
        });

        var result = await generator.GenerateSiteswapsAsync();
        await result.WaitForSiteswapAsync("a7562");
        await result.ShoulNotGenerateAsync("aaa00");
    }
}
