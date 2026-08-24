using FluentAssertions;
using Siteswaps.Generator.Components.Feeding;
using Siteswaps.Generator.Components.GenerationWorkflow;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test.Feeding;

/// <summary>
/// Where a fedee passes to A is the user's choice, not the generator's. Even a fedee that
/// receives a single pass — and therefore throws a single one back — can place that Pass on any
/// open Interface beat at A, so the UI has to offer that choice instead of taking the first fit.
/// </summary>
[TestFixture]
public class FeedInterfaceChoiceTests
{
    private static IReadOnlyList<Siteswap>? _singlePassB1Results;

    /// <summary>Period 5, four passes, B1 receives exactly one of them.</summary>
    private static NormalFeedSession CreateSinglePassFeed()
    {
        var feed = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 7, 5, 6));
        feed.AssignPass(0, "B1");
        feed.AssignPass(1, "B2");
        feed.AssignPass(2, "B2");
        feed.AssignPass(3, "B2");
        feed.ClubsB1 = new Between { MinNumber = 6, MaxNumber = 7 };
        feed.ClubsB2 = new Between { MinNumber = 6, MaxNumber = 7 };
        return feed;
    }

    /// <summary>Period 5, four passes, B1 receives two of them.</summary>
    private static NormalFeedSession CreateTwoPassFeed()
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

    private static async Task<IReadOnlyList<Siteswap>> SinglePassB1ResultsAsync() =>
        _singlePassB1Results ??= await GenerateAsync(CreateSinglePassFeed(), "B1");

    private static List<int> PassInterfaceBeatsOwnedBy(NormalFeedSession feed, string role) =>
        feed.FeederInterfaceOccupancy()
            .Where(slot =>
                slot.Owner == (role == "B1" ? FeedInterfaceOwner.B1 : FeedInterfaceOwner.B2)
            )
            .Select(slot => slot.Beat)
            .ToList();

    [Test]
    public async Task A_Single_Pass_Fedee_Can_Still_Land_On_Several_Beats_Of_The_Feeder()
    {
        var feed = CreateSinglePassFeed();
        var results = await SinglePassB1ResultsAsync();

        var beats = feed.SelectablePassInterfaceBeatsFor("B1", results);

        beats
            .Should()
            .HaveCountGreaterThan(
                1,
                "one pass back means one Pass on A's Interface, but the user picks which beat"
            );
        beats.Should().NotIntersectWith(feed.ForcedSelfInterfaceBeatsFor("B1"));
        beats.Should().BeSubsetOf(feed.OpenPassInterfaceBeats());
    }

    [Test]
    public async Task Picking_A_Beat_Puts_The_Return_Pass_Exactly_There()
    {
        var results = await SinglePassB1ResultsAsync();
        var offered = CreateSinglePassFeed().SelectablePassInterfaceBeatsFor("B1", results);

        foreach (var beat in offered)
        {
            var feed = CreateSinglePassFeed();

            feed.TrySelectPassInterfaceBeat("B1", beat, results).Should().BeTrue();

            PassInterfaceBeatsOwnedBy(feed, "B1")
                .Should()
                .Equal([beat], $"B1 was asked to pass back on beat {beat}");
        }
    }

    [Test]
    public async Task Moving_The_Pass_Interface_Beat_Releases_The_Old_Beat_And_Takes_The_New_One()
    {
        var results = await SinglePassB1ResultsAsync();
        var feed = CreateSinglePassFeed();
        var offered = feed.SelectablePassInterfaceBeatsFor("B1", results);
        var (first, second) = (offered[0], offered[1]);

        feed.TrySelectPassInterfaceBeat("B1", first, results).Should().BeTrue();
        feed.ForcedSelfInterfaceBeatsFor("B2").Should().Contain(first);

        feed.TrySelectPassInterfaceBeat("B1", second, results).Should().BeTrue();

        PassInterfaceBeatsOwnedBy(feed, "B1").Should().Equal([second]);
        feed.FeederInterfaceOccupancy()
            .Should()
            .ContainEquivalentOf(
                new FeedInterfaceBeat(first, FeedInterfaceOwner.Free),
                "the beat B1 left has to be free again"
            );
        feed.ForcedSelfInterfaceBeatsFor("B2").Should().Contain(second).And.NotContain(first);
    }

    [Test]
    public async Task Moving_B1_Changes_Which_Patterns_B2_Can_Still_Use()
    {
        var results = await SinglePassB1ResultsAsync();
        var offered = CreateSinglePassFeed().SelectablePassInterfaceBeatsFor("B1", results);

        var b2ByB1InterfaceBeat = new Dictionary<int, HashSet<string>>();
        foreach (var beat in offered)
        {
            var feed = CreateSinglePassFeed();
            feed.TrySelectPassInterfaceBeat("B1", beat, results).Should().BeTrue();
            b2ByB1InterfaceBeat[beat] = (await GenerateAsync(feed, "B2"))
                .Select(s => string.Join(",", s.Items))
                .ToHashSet();
        }

        b2ByB1InterfaceBeat
            .Values.Select(set => string.Join("|", set.Order()))
            .Distinct()
            .Should()
            .HaveCountGreaterThan(1, "B2 is constrained by where B1 actually lands");
    }

    [Test]
    public async Task Keeping_The_Current_Beat_Keeps_The_Current_Pattern()
    {
        var results = await SinglePassB1ResultsAsync();
        var feed = CreateSinglePassFeed();
        var beat = feed.SelectablePassInterfaceBeatsFor("B1", results)[0];

        feed.TrySelectPassInterfaceBeat("B1", beat, results).Should().BeTrue();
        var selected = feed.SelectedSiteswap("B1")!;

        feed.TrySelectPassInterfaceBeat("B1", beat, results).Should().BeTrue();

        feed.SelectedSiteswap("B1")!.Items.Should().Equal(selected.Items);
    }

    [Test]
    public async Task A_Beat_Forced_Self_By_The_Feeders_Own_Self_Is_Neither_Offered_Nor_Accepted()
    {
        var results = await SinglePassB1ResultsAsync();
        var feed = CreateSinglePassFeed();
        var selfBeat = feed.ForcedSelfInterfaceBeatsFor("B1").Single();

        feed.SelectablePassInterfaceBeatsFor("B1", results).Should().NotContain(selfBeat);
        feed.TrySelectPassInterfaceBeat("B1", selfBeat, results).Should().BeFalse();

        var select = () => feed.SelectSiteswap("B1", results[0], selfBeat);

        select
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("*cannot place a Pass on Interface beat*");
    }

    [Test]
    public async Task Interface_Options_Only_Claim_Open_Beats_And_Keep_The_Feeders_Pass_Beats()
    {
        var feed = CreateSinglePassFeed();
        var throwTime = feed.ThrowTimeInterfaceFor("B1");

        foreach (var candidate in await SinglePassB1ResultsAsync())
        {
            var options = feed.InterfaceOptionsFor("B1", candidate);
            options.Should().NotBeEmpty("every generated pattern must fit somewhere");
            options
                .Select(option => string.Join(",", option.PassBeats))
                .Should()
                .OnlyHaveUniqueItems("options differ by the beats they claim, not by phase");

            foreach (var option in options)
            {
                var placed = FeedRotation(candidate, option.RotationSteps);
                placed
                    .Items.Select(height => height % 2 == 0 ? Throw.AnySelf : Throw.AnyPass)
                    .Should()
                    .Equal(throwTime);
                option.PassBeats.Should().NotIntersectWith(feed.ForcedSelfInterfaceBeatsFor("B1"));
                option.PassBeats.Should().Equal(feed.PassInterfaceBeatsOf(placed));
            }
        }
    }

    [Test]
    public async Task A_Two_Pass_Fedee_Chooses_A_Pair_Of_Interface_Beats()
    {
        var feed = CreateTwoPassFeed();
        var results = await GenerateAsync(feed, "B1");

        var offered = feed.SelectablePassInterfaceBeatsFor("B1", results);
        offered
            .Should()
            .HaveCountGreaterThan(2, "two Interface Pass beats out of at least three open beats");

        foreach (var beat in offered)
        {
            var probe = CreateTwoPassFeed();
            probe.TrySelectPassInterfaceBeat("B1", beat, results).Should().BeTrue();

            var passBeats = PassInterfaceBeatsOwnedBy(probe, "B1");
            passBeats.Should().Contain(beat);
            passBeats.Should().HaveCount(2, "B1 throws back as many passes as it receives");
        }
    }

    [Test]
    public void A_Pattern_That_Misses_The_Feeders_Pass_Beats_Has_No_Interface_Option()
    {
        var feed = CreateSinglePassFeed();

        feed.InterfaceOptionsFor("B1", Siteswap.CreateFromCorrect(7, 5, 7, 5, 6))
            .Should()
            .BeEmpty("four passes cannot fit an interface that asks for one");
    }

    private static Siteswap FeedRotation(Siteswap siteswap, int steps)
    {
        var items = siteswap.Items;
        var period = items.Length;
        var rotated = new int[period];
        for (var i = 0; i < period; i++)
        {
            rotated[i] = items[(i + steps) % period];
        }

        return Siteswap.CreateFromCorrect(rotated);
    }
}

/// <summary>Beats are counted from 1 wherever the feeding UI names one.</summary>
[TestFixture]
public class FeedingBeatNumberingTests
{
    [Test]
    public void Interface_Row_Labels_Beats_From_One()
    {
        var razor = ReadGeneratorSource(
            Path.Combine("Components", "Feeding", "FeedingInterfaceOccupancy.razor")
        );

        razor.Should().Contain("""L["Beat {0}", beat.LocalBeat + 1]""");
        razor.Should().NotContain("""L["Beat {0}", beat.Beat]""");
    }

    [Test]
    public void Pass_Assignment_Row_Labels_Beats_From_One()
    {
        var razor = ReadGeneratorSource(Path.Combine("Components", "Feeding", "FeedingPage.razor"));

        razor.Should().Contain("beatIndex + 1, partnerLabel");
        razor.Should().Contain("""L["Beat {0} self throw", beatIndex + 1]""");
    }

    [Test]
    public void Empty_B2_Hint_Names_Beats_From_One()
    {
        var code = ReadGeneratorSource(
            Path.Combine("Components", "Feeding", "FeedingPage.razor.cs")
        );

        code.Should().Contain("""L["Beat {0}", slot.Beat + 1]""");
    }

    private static string ReadGeneratorSource(string relativePathUnderGeneratorProject) =>
        File.ReadAllText(
            Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "..",
                "..",
                "..",
                "..",
                "Siteswaps.Generator",
                relativePathUnderGeneratorProject
            )
        );
}
