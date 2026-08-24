using FluentAssertions;
using Siteswaps.Generator.Components.Feeding;
using Siteswaps.Generator.Components.GenerationWorkflow;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test.Feeding;

[TestFixture]
public class FeedingThrowLandingTests
{
    [Test]
    public void Pattern_97522_Uses_As_Local_Half_Beat_Timeline()
    {
        var feed = Create97522();

        feed.FeederInterfaceTimeline()
            .Select(beat => beat.GlobalBeat)
            .Should()
            .Equal(0, 2, 4, 1, 3);
        feed.FeederInterfaceTimeline()
            .Select(beat => beat.Owner)
            .Should()
            .Equal(
                FeedInterfaceOwner.Self,
                FeedInterfaceOwner.Free,
                FeedInterfaceOwner.Free,
                FeedInterfaceOwner.Self,
                FeedInterfaceOwner.Free
            );
    }

    [Test]
    public async Task Pass_Owner_Matches_The_Actual_Return_Throw_And_Landing()
    {
        var feed = await CreateComplete97522Async();

        AssertReturnPassesMatchOwners(feed, "B1", FeedInterfaceOwner.B1);
        AssertReturnPassesMatchOwners(feed, "B2", FeedInterfaceOwner.B2);
    }

    [Test]
    public async Task Rotation_Preserves_Assignments_Owners_And_Landing_Timeline()
    {
        var feed = await CreateComplete97522Async();
        var assignmentsBefore = feed
            .PassAssignments.Where(value => value is not null)
            .Order()
            .ToList();

        feed.Rotate(1);

        feed.PassAssignments.Where(value => value is not null)
            .Order()
            .Should()
            .Equal(assignmentsBefore);
        AssertReturnPassesMatchOwners(feed, "B1", FeedInterfaceOwner.B1);
        AssertReturnPassesMatchOwners(feed, "B2", FeedInterfaceOwner.B2);
    }

    [Test]
    public void Feeder_Pass_Lands_On_Its_Assigned_Partner()
    {
        var feed = Create97522();

        var landing = feed.LandingFor("A", localBeat: 0);

        landing.SourceGlobalBeat.Should().Be(0);
        landing.Height.Should().Be(9);
        landing.TargetRole.Should().Be("B1");
        landing.TargetGlobalBeat.Should().Be(4);
        landing.TargetLocalBeat.Should().Be(4);
    }

    private static NormalFeedSession Create97522()
    {
        var feed = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(9, 7, 5, 2, 2));
        feed.AssignPass(0, "B1");
        feed.AssignPass(1, "B2");
        feed.AssignPass(2, "B1");
        return feed;
    }

    private static async Task<NormalFeedSession> CreateComplete97522Async()
    {
        var feed = Create97522();
        var b1Results = await GenerationWorkflowSession
            .Create(feed.ToGenerationWorkflowConfig("B1"))
            .GenerateAsync();
        feed.SelectSiteswap("B1", b1Results[0]);
        var b2Results = await GenerationWorkflowSession
            .Create(feed.ToGenerationWorkflowConfig("B2"))
            .GenerateAsync();
        feed.SelectSiteswap("B2", b2Results[0]);
        return feed;
    }

    private static void AssertReturnPassesMatchOwners(
        NormalFeedSession feed,
        string role,
        FeedInterfaceOwner owner
    )
    {
        var siteswap = feed.SelectedSiteswap(role)!;
        var localPeriod = siteswap.Period.GetLocalPeriod(2).Value;
        var passLandings = Enumerable
            .Range(0, localPeriod)
            .Select(localBeat => feed.LandingFor(role, localBeat))
            .Where(landing => landing.Kind == PassOrSelf.Pass)
            .ToList();

        passLandings.Should().NotBeEmpty();
        passLandings.Should().OnlyContain(landing => landing.TargetRole == "A");
        foreach (var landing in passLandings)
        {
            feed.FeederInterfaceTimeline()[landing.TargetLocalBeat]
                .Should()
                .Be(
                    new FeedInterfaceTimelineBeat(
                        landing.TargetLocalBeat,
                        landing.TargetGlobalBeat,
                        owner
                    )
                );
        }
    }
}
