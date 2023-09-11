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
        jugglerA.PassingTargets = new List<string>() {"B1", "B2", ""};
        feedingPattern.UpdateFeedingFilter();

        var jugglerB = feedingPattern.Jugglers[1];
        jugglerB.VisibleFilter.Count.Should().Be(1);
    }
    
    [Test]
    public void Test4()
    {
        var feedingPattern = FeedingPattern.N_Feed();

        var jugglerA1 = feedingPattern.Jugglers[0];

        jugglerA1.SelectedSiteswap = Siteswap.CreateFromCorrect(7,5,6);
        jugglerA1.PassingTargets = new List<string>() {"B1", "B2", ""};
        feedingPattern.UpdateFeedingFilter();

        var jugglerB1= feedingPattern.Jugglers[1];
        jugglerB1.VisibleFilter.Count.Should().Be(1);
    }
}
