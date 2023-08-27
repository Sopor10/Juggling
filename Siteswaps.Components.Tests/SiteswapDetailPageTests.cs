namespace Siteswaps.Components.Tests;

using Feeding;
using FluentAssertions;
using Generator.Components.State;
using Generator.Generator;

public class SiteswapDetailPageTests
{
    [Test]
    public void Test1()
    {
        var localSiteswaps = Siteswap.CreateFromCorrect(7, 5, 6).GetLocalSiteswaps(2);
        
    }
}


public class FeedingPatternTest
{
    [Test]
    public void Test1()
    {
        var sut = FeedingPattern.NormalFeed();

        var jugglerA = sut.Jugglers[0];
        jugglerA.SelectedSiteswap = Siteswap.CreateFromCorrect(7, 8, 6, 2, 7);
        jugglerA.PassingSelection = new List<string> {"B1", "", "", "", "B2"};
        sut.UpdateFeedingFilter();

        sut.Jugglers[1].VisibleFilter.Should().HaveCount(1);
        sut.Jugglers[1].VisibleFilter[0]
            .Should().BeOfType<InterfaceFilterInformation>()
            .Which.Pattern.Should().BeEquivalentTo(new[]{Throw.AnySelf, Throw.AnySelf, Throw.AnyPass, Throw.AnySelf, Throw.AnySelf, }, x => x.WithStrictOrdering());
        
        
        sut.Jugglers[2].VisibleFilter.Should().HaveCount(1);
        sut.Jugglers[2].VisibleFilter[0]
            .Should().BeOfType<InterfaceFilterInformation>()
            .Which.Pattern.Should().BeEquivalentTo(new[]{Throw.AnySelf, Throw.AnyPass, Throw.AnySelf, Throw.AnySelf, Throw.AnySelf, }, x => x.WithStrictOrdering());
        
        sut.Jugglers[1].SelectedSiteswap = Siteswap.CreateFromCorrect(8,6,8,6,7);
        sut.Jugglers[2].SelectedSiteswap = Siteswap.CreateFromCorrect(7,8,6,8,6);

        sut.UpdateFeedingFilter();
        var localSiteswap = sut.Jugglers[1].SelectedSiteswap!.GetLocalSiteswap(sut.Jugglers[1].TimeZone, 2, sut.Jugglers[1].Name);
        localSiteswap.Values.EnumerateValues(1).Should().BeEquivalentTo(new[]{6,6,8,8,7}, x => x.WithStrictOrdering());
        
        var localSiteswap2 = sut.Jugglers[2].SelectedSiteswap!.GetLocalSiteswap(sut.Jugglers[2].TimeZone, 2, sut.Jugglers[2].Name);
        
        localSiteswap2.Values.EnumerateValues(1).Should().BeEquivalentTo(new[]{8,8,7,6,6}, x => x.WithStrictOrdering());

    }
}
