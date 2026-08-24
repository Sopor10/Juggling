using FluentAssertions;
using Siteswaps.Generator.Components.Feeding;
using Siteswaps.Generator.Components.GenerationWorkflow;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test.Feeding;

/// <summary>
/// Who A throws to and who lands at A are two independent things. Period 5 with four passes gives
/// four beats where a pass has to land at A; the pattern picked for B1 claims some of them and B2
/// has to work with what is left.
/// </summary>
[TestFixture]
public class FeedCouplingTests
{
    /// <summary>Period 5, four passes — the case the coupling was reported for.</summary>
    private static NormalFeedSession CreateFeed()
    {
        var feed = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 7, 5, 6));
        feed.AssignPass(0, "B1");
        feed.AssignPass(2, "B1");
        feed.AssignPass(1, "B2");
        feed.AssignPass(3, "B2");
        feed.ClubsB1 = new Between { MinNumber = 6, MaxNumber = 6 };
        feed.ClubsB2 = new Between { MinNumber = 6, MaxNumber = 6 };
        return feed;
    }

    private static async Task<List<Siteswap>> GenerateAsync(NormalFeedSession feed, string role) =>
        (
            await GenerationWorkflowSession
                .Create(feed.ToGenerationWorkflowConfig(role))
                .GenerateAsync()
        ).ToList();

    private static string Key(Siteswap siteswap) => string.Join(",", siteswap.Items);

    /// <summary>Slots at A that this candidate would claim, once its phase is pinned.</summary>
    private static IReadOnlyList<int> ArrivalsAt(string role, Siteswap candidate)
    {
        var probe = CreateFeed();
        probe.SelectSiteswap(role, candidate);
        return probe.PassInterfaceBeatsOf(probe.SelectedSiteswap(role)!);
    }

    /// <summary>One representative B1 result per distinct set of slots it claims at A.</summary>
    private static async Task<Dictionary<string, Siteswap>> B1ByArrivalAsync()
    {
        var byArrival = new Dictionary<string, Siteswap>();
        foreach (var candidate in await GenerateAsync(CreateFeed(), "B1"))
        {
            byArrival.TryAdd(string.Join(",", ArrivalsAt("B1", candidate)), candidate);
        }

        return byArrival;
    }

    [Test]
    public void Four_Passes_Mean_Four_Beats_Where_A_Pass_Lands_At_The_Feeder()
    {
        var feed = CreateFeed();

        feed.OpenPassInterfaceBeats().Should().Equal(1, 2, 3, 4);
        feed.ForcedSelfInterfaceBeatsFor("B1")
            .Should()
            .Equal(
                new[] { 0 },
                "slot 0 carries the feeder's own self, the other four carry passes"
            );
    }

    [Test]
    public async Task B1_Can_Arrive_At_The_Feeder_On_Different_Slots()
    {
        var byArrival = await B1ByArrivalAsync();

        byArrival
            .Should()
            .HaveCountGreaterThan(
                1,
                "where B1 passes back to is a free choice, not a consequence of the pass assignment"
            );
        byArrival
            .Keys.Should()
            .OnlyContain(key => key.Split(',').Length == 2, "B1 receives two of the four passes");
    }

    /// <summary>
    /// A catches on t=0,2,4,6,8, i.e. slots 0,2,4,1,3. Slot 0 carries its own self, so the arrival
    /// pattern runs over slots 2,4,1,3 — the "B1 B2 B1 B2 vs B1 B1 B2 B2" choice.
    /// </summary>
    private static readonly int[] ArrivalOrder = [2, 4, 1, 3];

    [Test]
    public async Task Arrival_Pattern_At_The_Feeder_Is_Completed_By_B2()
    {
        var optionsPerPattern = new Dictionary<string, int>();

        foreach (var (_, b1) in await B1ByArrivalAsync())
        {
            var feed = CreateFeed();
            feed.SelectSiteswap("B1", b1);
            var claimedByB1 = feed.PassInterfaceBeatsOf(feed.SelectedSiteswap("B1")!);

            var b2Results = await GenerateAsync(feed, "B2");
            foreach (var b2 in b2Results)
            {
                feed.SelectSiteswap("B2", b2);
                var claimedByB2 = feed.PassInterfaceBeatsOf(feed.SelectedSiteswap("B2")!);

                claimedByB2.Should().NotIntersectWith(claimedByB1);
                claimedByB1
                    .Concat(claimedByB2)
                    .Should()
                    .BeEquivalentTo(
                        ArrivalOrder,
                        "together the fedees have to fill every open Pass beat of A's Interface"
                    );
            }

            var pattern = string.Concat(
                ArrivalOrder.Select(slot => claimedByB1.Contains(slot) ? "B1" : "B2")
            );
            optionsPerPattern[pattern] = b2Results.Count;
        }

        optionsPerPattern
            .Keys.Should()
            .BeEquivalentTo(
                ["B1B2B1B2", "B1B1B2B2", "B2B1B1B2"],
                "the arrival at A is a choice of its own, so several patterns are reachable"
            );
        optionsPerPattern["B1B1B2B2"].Should().BePositive();
        optionsPerPattern["B1B2B1B2"]
            .Should()
            .Be(0, "B1 taking slots 1 and 2 leaves B2 no workable pattern at six clubs");
    }

    [Test]
    public async Task Different_B1_Arrivals_Leave_Different_Options_For_B2()
    {
        var byArrival = await B1ByArrivalAsync();
        byArrival.Should().HaveCountGreaterThan(1);

        var b2Sets = new Dictionary<string, HashSet<string>>();
        foreach (var (arrival, b1) in byArrival)
        {
            var feed = CreateFeed();
            feed.SelectSiteswap("B1", b1);
            feed.ForcedSelfInterfaceBeatsFor("B2")
                .Should()
                .Contain(arrival.Split(',').Select(int.Parse));

            b2Sets[arrival] = (await GenerateAsync(feed, "B2")).Select(Key).ToHashSet();
        }

        var unconstrained = (await GenerateAsync(CreateFeed(), "B2")).Select(Key).ToHashSet();
        foreach (var (arrival, set) in b2Sets)
        {
            set.IsProperSubsetOf(unconstrained)
                .Should()
                .BeTrue($"B1 arriving on [{arrival}] rules B2 candidates out");
        }

        b2Sets
            .Values.Select(set => string.Join("|", set.Order()))
            .Distinct()
            .Should()
            .HaveCountGreaterThan(
                1,
                "B2's options follow B1's concrete arrival, not the assignment"
            );
    }

    [Test]
    public async Task Candidate_Pass_On_Forced_Self_Interface_Beat_Is_Excluded_And_Rejected_On_Select()
    {
        var byArrival = await B1ByArrivalAsync();
        var (arrival, b1) = byArrival.First(x => x.Key == "2,4");
        var blocked = arrival.Split(',').Select(int.Parse).ToList();

        var feed = CreateFeed();
        feed.SelectSiteswap("B1", b1);

        var colliding = (await GenerateAsync(CreateFeed(), "B2")).First(candidate =>
            ArrivalsAt("B2", candidate).Intersect(blocked).Any()
        );

        (await GenerateAsync(feed, "B2")).Select(Key).Should().NotContain(Key(colliding));

        var select = () => feed.SelectSiteswap("B2", colliding);
        select.Should().Throw<ArgumentException>().WithMessage("*already forced to Self*");
    }

    [Test]
    public async Task Changing_B1_Drops_A_Now_Incompatible_B2_Selection()
    {
        var byArrival = await B1ByArrivalAsync();
        var feed = CreateFeed();
        feed.SelectSiteswap("B1", byArrival["2,4"]);

        var b2 = (await GenerateAsync(feed, "B2")).First();
        feed.SelectSiteswap("B2", b2);
        var b2Arrivals = feed.PassInterfaceBeatsOf(feed.SelectedSiteswap("B2")!);
        feed.SelectedSiteswap("B2").Should().NotBeNull();

        var replacement = byArrival.Values.First(candidate =>
            ArrivalsAt("B1", candidate).Intersect(b2Arrivals).Any()
        );

        feed.SelectSiteswap("B1", replacement);

        feed.SelectedSiteswap("B1").Should().NotBeNull();
        feed.SelectedSiteswap("B2")
            .Should()
            .BeNull("B2 was counting on a Interface beat that B1 now forces to Self");
    }

    private static IEnumerable<int> BeatsOwnedBy(
        NormalFeedSession feed,
        FeedInterfaceOwner owner
    ) =>
        feed.FeederInterfaceOccupancy()
            .Where(slot => slot.Owner == owner)
            .Select(slot => slot.Beat);

    [Test]
    public void Interface_Occupancy_Starts_With_Only_The_Feeders_Own_Self_Taken()
    {
        CreateFeed()
            .FeederInterfaceOccupancy()
            .Should()
            .Equal(
                [
                    new FeedInterfaceBeat(0, FeedInterfaceOwner.Self),
                    new FeedInterfaceBeat(1, FeedInterfaceOwner.Free),
                    new FeedInterfaceBeat(2, FeedInterfaceOwner.Free),
                    new FeedInterfaceBeat(3, FeedInterfaceOwner.Free),
                    new FeedInterfaceBeat(4, FeedInterfaceOwner.Free),
                ],
                "before anything is picked only A's own self occupies a beat"
            );
    }

    [Test]
    public async Task Interface_Occupancy_Attributes_Every_Taken_Beat_To_Its_Owner()
    {
        foreach (var (_, b1) in await B1ByArrivalAsync())
        {
            var feed = CreateFeed();
            feed.SelectSiteswap("B1", b1);

            BeatsOwnedBy(feed, FeedInterfaceOwner.Self).Should().Equal(new[] { 0 });
            BeatsOwnedBy(feed, FeedInterfaceOwner.B1)
                .Should()
                .Equal(
                    feed.PassInterfaceBeatsOf(feed.SelectedSiteswap("B1")!),
                    "the occupancy must name B1 on exactly the slots its return passes claim"
                );

            if ((await GenerateAsync(feed, "B2")).FirstOrDefault() is not { } b2)
            {
                continue;
            }

            feed.SelectSiteswap("B2", b2);
            BeatsOwnedBy(feed, FeedInterfaceOwner.B2)
                .Should()
                .Equal(feed.PassInterfaceBeatsOf(feed.SelectedSiteswap("B2")!));
            BeatsOwnedBy(feed, FeedInterfaceOwner.Free)
                .Should()
                .BeEmpty("B1 and B2 together fill every open Pass beat of A's Interface");
        }
    }
}
