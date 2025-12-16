using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests;

public class GenerateSiteswapsTests(BlazorWebassemblyFixture<Program> fixture)
    : IClassFixture<BlazorWebassemblyFixture<Program>>
{
    public BlazorWebassemblyFixture<Program> Fixture { get; } = fixture;

    [Fact]
    public async Task Generate_Siteswaps_In_Default_Conditions()
    {
        var page = await Fixture.Context!.NewPageAsync();

        var generator = await page.OpenGeneratorAsync(Fixture.RootUri.AbsoluteUri);
        var result = await generator.GenerateSiteswapsAsync();
        await result.WaitForSiteswapAsync("aaa00");
        await result.ShoulNotGenerateAsync("aa613");
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

    [Fact]
    public async Task Generate_Siteswaps_With_Custom_Period()
    {
        var page = await Fixture.Context!.NewPageAsync();

        var generator = await page.OpenGeneratorAsync(Fixture.RootUri.AbsoluteUri);
        await generator.SetPeriodAsync(4);
        var result = await generator.GenerateSiteswapsAsync();
        await result.WaitForGenerationToCompleteAsync();
        await result.ShouldGenerateAtLeastAsync(1);
    }

    [Fact]
    public async Task Generate_Siteswaps_With_Different_Number_Of_Clubs()
    {
        var page = await Fixture.Context!.NewPageAsync();

        var generator = await page.OpenGeneratorAsync(Fixture.RootUri.AbsoluteUri);
        await generator.SetMinClubsAsync(4);
        await generator.SetMaxClubsAsync(4);
        var result = await generator.GenerateSiteswapsAsync();
        await result.WaitForGenerationToCompleteAsync();
        await result.ShouldGenerateAtLeastAsync(1);
    }

    [Fact]
    public async Task Generate_Siteswaps_With_Number_Filter_Exactly()
    {
        var page = await Fixture.Context!.NewPageAsync();

        var generator = await page.OpenGeneratorAsync(Fixture.RootUri.AbsoluteUri);
        await generator.AddNumberFilterAsync(async x =>
        {
            await x.SetFilterTypeAsync("Exactly");
            await x.SetAmountAsync(1);
            await x.SetThrowAsync("5");
            await x.AddFilterAsync();
        });

        var result = await generator.GenerateSiteswapsAsync();
        await result.WaitForGenerationToCompleteAsync();
        await result.ShouldGenerateAtLeastAsync(1);
    }

    [Fact]
    public async Task Generate_Siteswaps_With_Number_Filter_AtLeast()
    {
        var page = await Fixture.Context!.NewPageAsync();

        var generator = await page.OpenGeneratorAsync(Fixture.RootUri.AbsoluteUri);
        await generator.AddNumberFilterAsync(async x =>
        {
            await x.SetFilterTypeAsync("AtLeast");
            await x.SetAmountAsync(2);
            await x.SetThrowAsync("3");
            await x.AddFilterAsync();
        });

        var result = await generator.GenerateSiteswapsAsync();
        await result.WaitForGenerationToCompleteAsync();
    }

    [Fact]
    public async Task Generate_Siteswaps_With_Number_Filter_Maximum()
    {
        var page = await Fixture.Context!.NewPageAsync();

        var generator = await page.OpenGeneratorAsync(Fixture.RootUri.AbsoluteUri);
        await generator.AddNumberFilterAsync(async x =>
        {
            await x.SetFilterTypeAsync("Maximum");
            await x.SetAmountAsync(1);
            await x.SetThrowAsync("9");
            await x.AddFilterAsync();
        });

        var result = await generator.GenerateSiteswapsAsync();
        await result.WaitForGenerationToCompleteAsync();
    }

    [Fact]
    public async Task Generate_Siteswaps_With_State_Filter()
    {
        var page = await Fixture.Context!.NewPageAsync();

        var generator = await page.OpenGeneratorAsync(Fixture.RootUri.AbsoluteUri);
        await generator.AddStateFilterAsync(async x =>
        {
            await x.ToggleStateAtHeightAsync(0);
            await x.ToggleStateAtHeightAsync(1);
            await x.ToggleStateAtHeightAsync(2);
            await x.AddFilterAsync();
        });

        var result = await generator.GenerateSiteswapsAsync();
        await result.WaitForGenerationToCompleteAsync();
    }

    [Fact]
    public async Task Generate_Siteswaps_With_Multiple_Filters()
    {
        var page = await Fixture.Context!.NewPageAsync();

        var generator = await page.OpenGeneratorAsync(Fixture.RootUri.AbsoluteUri);
        await generator.AddPatternFilterAsync(async x =>
        {
            await x.AddThrowAsync("Self");
            await x.AddFilterAsync();
        });
        await generator.AddNumberFilterAsync(async x =>
        {
            await x.SetFilterTypeAsync("Exactly");
            await x.SetAmountAsync(1);
            await x.SetThrowAsync("5");
            await x.AddFilterAsync();
        });

        var result = await generator.GenerateSiteswapsAsync();
        await result.WaitForGenerationToCompleteAsync();
    }

    [Fact]
    public async Task Generate_Siteswaps_With_Passing_Pattern()
    {
        var page = await Fixture.Context!.NewPageAsync();

        var generator = await page.OpenGeneratorAsync(Fixture.RootUri.AbsoluteUri);
        await generator.SetNumberOfJugglersAsync(2);
        await generator.SetPeriodAsync(4);
        var result = await generator.GenerateSiteswapsAsync();
        await result.WaitForGenerationToCompleteAsync();
        await result.ShouldGenerateAtLeastAsync(1);
    }

    [Fact]
    public async Task Generate_Siteswaps_With_Specific_Throws()
    {
        var page = await Fixture.Context!.NewPageAsync();

        var generator = await page.OpenGeneratorAsync(Fixture.RootUri.AbsoluteUri);
        await generator.AddThrowAsync("3");
        await generator.AddThrowAsync("5");
        await generator.AddThrowAsync("1");
        var result = await generator.GenerateSiteswapsAsync();
        await result.WaitForSiteswapAsync("531");
        await result.ShoulNotGenerateAsync("423");
    }

    [Fact]
    public async Task Generate_Siteswaps_With_Longer_Period()
    {
        var page = await Fixture.Context!.NewPageAsync();

        var generator = await page.OpenGeneratorAsync(Fixture.RootUri.AbsoluteUri);
        await generator.SetPeriodAsync(5);
        var result = await generator.GenerateSiteswapsAsync();
        await result.WaitForGenerationToCompleteAsync();
        await result.ShouldGenerateAtLeastAsync(1);
    }
}
