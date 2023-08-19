namespace Siteswaps.Components.Tests;

using Feeding;
using FluentAssertions;
using Generator.Generator;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var feedingPattern = FeedingPattern.NormalFeed();

        var jugglerA = feedingPattern.Jugglers[0];

        jugglerA.SelectedSiteswap = Siteswap.CreateFromCorrect(7, 5, 6);
        feedingPattern.UpdateFeedingFilter();
        
        jugglerA.InterfaceAsPassOrSelf().Should().BeEquivalentTo(new List<InterfaceSplitting.PassOrSelf>()
        {
            InterfaceSplitting.PassOrSelf.Pass,
            InterfaceSplitting.PassOrSelf.Pass,
            InterfaceSplitting.PassOrSelf.Self
        });
    }
    
    [Test]
    public void Test3()
    {
        var feedingPattern = FeedingPattern.NormalFeed();

        var jugglerA = feedingPattern.Jugglers[0];

        jugglerA.SelectedSiteswap = Siteswap.CreateFromCorrect(7,5,6);
        jugglerA.PassingSelection = new List<string>() {"B1", "B2", ""};
        feedingPattern.UpdateFeedingFilter();

        var jugglerB = feedingPattern.Jugglers[1];
        jugglerB.VisibleFilter.Count.Should().Be(1);
    }
}
