using FluentAssertions;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class CausalDiagramEditorStateTests
{
    private static readonly string[] ReviewedHighgatePresetIds =
    {
        "highgate-7786266",
        "highgate-7266867",
        "highgate-7966266",
        "highgate-972",
        "highgate-972alternativestart",
        "highgate-passselfzap",
        "highgate-zapzapheff",
        "highgate-funkybookendsvsparsnip",
        "highgate-parsnipvsfunkybookends",
        "highgate-alatedouble",
        "highgate-alatesingle",
        "highgate-aselfdouble",
        "highgate-fiveclubthreecount",
        "highgate-parsnip",
        "highgate-fiveclubwhynot",
        "highgate-fiveclubnotwhy",
        "highgate-jimsthreecountasynchversion",
        "highgate-synchronousonecount",
        "highgate-synchronousonecountwithheffs",
        "highgate-asailorwenttosea",
        "highgate-countdown",
        "highgate-geometriccountdown",
        "highgate-fizzbuzz",
        "highgate-jimsfizzbuzzsynchronousversion",
        "highgate-779667withcrossingdoubles",
        "highgate-777966withstraightdoubles",
        "highgate-sevenclubthreecountondoubles",
        "highgate-sevenclubthreecountonsinglestechno",
        "highgate-sevenclubfourcounttripleself",
        "highgate-sevenclubfourcountondoubles",
        "highgate-8x6x68x8x6withcrossingsinglesandstraightdoubles",
        "highgate-sevenclubwhynotstylepattern",
        "highgate-sevenclubnotwhystylepattern",
        "highgate-sevenclubmaybestylepattern1",
        "highgate-sevenclubmaybestylepattern2",
        "highgate-fiveclubonecount",
        "highgate-jims-one-count-async",
        "highgate-jims-two-count-async",
        "highgate-frenchthreecount",
        "highgate-civilwarfeed",
        "highgate-gorillafeed",
        "highgate-onecountfeedingbookendsandinvertedbookends",
        "highgate-onecountfeedingcountdown",
        "highgate-onecountfeedingpasspassselfandthreecount",
        "highgate-passpassselffeed",
        "highgate-suburbanterrorfeed",
        "highgate-twocountfeed",
        "highgate-martinsonecountfeedingfiveclubwhynotandfunkybookends",
        "highgate-martinsonecountfeedingmaybe",
        "highgate-onecountfeedingfiveclubwhynotandmartinsonecount",
        "highgate-onecountfeedingwhynotandparsnip",
        "highgate-ambidextrouspulsaronecountfeed",
        "highgate-ambidextrouspulsarpasspassselffeed",
        "highgate-ambidextrouspulsarpassselfpassfeed",
        "highgate-ambidextrouspulsarthreecountfeed",
        "highgate-extendedtarimstriangle",
        "highgate-onecountonecountcountdowntriangle",
        "highgate-onecountonecountpasspassselftriangle",
        "highgate-onecountonecountthreecounttriangle",
        "highgate-passpassselfpasspassselfcountdowntriangle",
        "highgate-pulsar",
        "highgate-tarimstriangle",
        "highgate-twocounttwocountfourcounttriangle",
        "highgate-clockwiseonecountrunaround",
        "highgate-tenclubgorilla",
        "highgate-fourpersonparsnipfeedingfivecountpopcorn",
        "highgate-tenclubfrenchthreecounttriangle",
        "highgate-tenclubpasspasshefftriangle",
        "highgate-fourpersononecountfeedingfourcount",
        "highgate-fourpersononecountfeedingthreecount",
        "highgate-fourpersononecountfeedingtwocount",
        "highgate-fourpersonpasspassselffeed",
        "highgate-fourpersontwocountfeed",
        "highgate-tenclubonecountvariation1",
        "highgate-tenclubonecountvariation2",
        "highgate-972box",
        "highgate-maybebox",
        "highgate-mildmadnessbox",
        "highgate-tenclubtarimstriangle",
        "highgate-whynotbox",
        "highgate-three-count",
        "highgate-sixcount",
        "highgate-fourcount",
        "highgate-twocount",
        "highgate-fivecount-ambidextrous",
        "highgate-threecount-ambidextrous",
        "highgate-jimsthreecount",
        "highgate-threecount-both-crossing",
        "highgate-passpassself",
        "highgate-chocolatebar",
        "highgate-bookends",
        "highgate-invertedbookends",
        "highgate-onecountwithzaps",
        "highgate-zapholdself",
        "highgate-sevenclubnotwhyvariation1",
        "highgate-sevenclubnotwhyvariation2",
        "highgate-sevenclubnotwhyvariation3",
        "highgate-sevenclubmaybevariation",
        "highgate-copenhagencountdown",
        "highgate-oslocountdown",
        "highgate-sevencountpopcornwithdoublepasses",
        "highgate-sevencountpopcornwithdoublepassesandheffs",
        "highgate-sevencountpopcornwithsinglepassesand534",
        "highgate-sevencountpopcornwithsinglepassesand453",
        "highgate-sevencountpopcornwithsinglepassesandheffs",
        "highgate-whynot",
        "highgate-whynotalternativestart",
        "highgate-notwhy",
        "highgate-notwhyalternativestart",
        "highgate-maybe",
        "highgate-maybealternativestart",
        "highgate-passpasszipfeedingthreecount",
        "highgate-passpasszipfeedingfrenchthreecount",
        "highgate-classicsixcountpopcorn",
        "highgate-sixcountpopcornwithheffs",
        "highgate-fivecountpopcorn",
        "highgate-fivecountpopcornwithheffs",
        "highgate-fivecountpopcornwithdoublepassesandheffs",
        "highgate-fourcountpopcorn",
        "highgate-fourcountpopcornwithheffs",
        "highgate-threepersonpasspassselffeastjedsnightmare",
        "highgate-threepersonpassselfpassfeastantiquasar",
        "highgate-threepersonpasspassselffeastdoublethreecount",
        "highgate-trifle",
        "highgate-trilogy",
        "highgate-threepersonfourcountdropbackline",
        "highgate-anticlockwisetwocountrunaround",
        "highgate-clockwisetwocountrunaround",
        "highgate-anticlockwisepasspassselfrunaround",
        "highgate-clockwisepasspassselfrunaround",
        "highgate-passselfzip",
        "highgate-therecipe",
        "highgate-fourfourone",
        "highgate-fourfourfourzero",
        "highgate-aselftriple",
        "highgate-58552",
        "highgate-doublezapzip531",
        "highgate-doublehold42",
        "highgate-sevenclubonecountonsingles",
        "highgate-jimsfizzbuzzasynchronousversion",
        "highgate-jimsbookendsakabrainstormasynchronousversion",
        "highgate-jimsinvertedbookendsfeed",
        "highgate-threepersonchocolatebarfeast1",
        "highgate-threepersonchocolatebarfeast2",
        "highgate-threepersonchocolatebarfeast3",
        "highgate-threepersonbookendsfeast1",
        "highgate-threepersonbookendsfeast2",
        "highgate-threepersonbookendsfeast3",
        "highgate-threepersoninvertedbookendsfeast1",
        "highgate-threepersoninvertedbookendsfeast2",
        "highgate-threepersoninvertedbookendsfeast3",
        "highgate-threepersontwocountfeast",
        "highgate-threepersononecountfeast",
        "highgate-threepersoncountdownfeast",
        "highgate-quasar",
        "highgate-fourcountvsthreecount",
        "highgate-threecountvstwocount",
        "highgate-twocountvspasspassself",
        "highgate-twocountvsonecount",
        "highgate-passpassselfvsonecount",
        "highgate-fourcounttriangleonins",
        "highgate-threecounttriangleonins",
        "highgate-twocounttriangleoninsandouts",
        "highgate-onecounttriangleoninsandouts",
        "highgate-passpassselftriangle",
        "highgate-twocountfeastrunaround",
        "highgate-trafficlightspattern",
        "highgate-antiquasarrunaround",
        "highgate-quasarrunaround",
        "highgate-anticlockwisepulsarrunaround",
        "highgate-onecount",
        "highgate-sevenclubtwocountonsingles",
        "highgate-anearlydouble",
        "highgate-anearlytriple",
        "highgate-alatetriple",
        "highgate-averylatetriple",
        "highgate-thirteenclubfourcountbox",
        "highgate-thirteenclubonecountfeedingcountdown",
        "highgate-thirteenclubthreecountbox",
        "highgate-thirteenclubthreecountboxvariation",
        "highgate-thirteenclubtwocountbox",
        "highgate-thirteenclubchocolatesquare",
        "highgate-weirdpasspassselfbox",
        "highgate-thirteenclubtwocountlinefeed",
        "highgate-thirteencluburbanterrorfeedonecountlinefeed",
        "highgate-thirteenclubfourcount",
        "highgate-thirteenclubthreecount",
        "highgate-thirteenclubtwocount",
        "highgate-thirteenclubonecount",
        "highgate-tenclubonecountfeedingcountdown",
        "highgate-passpassselffeedingfrenchthreecount",
        "highgate-passpasshefffeedingfrenchthreecount",
        "highgate-asynchronouspasspasshefffeedingthreecount",
        "highgate-tenclubtwocountfeedonsingles",
        "highgate-tenclubonecountfeedingpasspassselfandthreecount",
        "highgate-tenclubonecountfeedingbookendsandinvertedbookends",
        "highgate-tenclubsuburbanterrorfeed",
        "highgate-fourpersononecountfeedingpasspassself",
        "highgate-fourpersononecountfeedingchocolatebar",
        "highgate-fourpersononecountfeedingbookends",
        "highgate-fourpersononecountfeedinginvertedbookends",
        "highgate-fourpersononecountfeedingcountdown",
        "highgate-fourpersoncivilwarfeed",
        "highgate-mixedcountfeed",
        "highgate-passpassselfbox",
        "highgate-chocolatesquare",
        "highgate-bookendsbox",
        "highgate-invertedbookendsbox",
        "highgate-twocountlinefeed",
        "highgate-urbanterrorfeedonecountlinefeed",
        "highgate-tenclubfourcounttriangle",
        "highgate-tenclubthreecounttriangle",
        "highgate-tenclubtwocounttriangle",
        "highgate-tenclubonecounttriangle",
        "highgate-fourpersonfourcountfeast",
        "highgate-fourpersonthreecountfeast",
        "highgate-fourpersontwocountfeast",
        "highgate-fourpersononecountfeast",
        "highgate-fourpersonpasspassselffeast",
        "highgate-fourpersonbookendsfeast",
        "highgate-fourpersontwocountrevolvingfeed",
        "highgate-fourpersononecountrevolvingfeed",
        "highgate-fourpersonquasar",
        "highgate-fourpersonfizzbuzzfeast",
        "highgate-fourpersonselflessfizzbuzz",
        "highgate-aidansfeast1",
        "highgate-aidansfeast2",
        "highgate-aidansfeast3",
        "highgate-aidansfeast4",
        "highgate-bookendslinefeed",
        "highgate-mobmildnessmadness",
        "highgate-parsniplinefeed",
        "highgate-mobneurosispsychosis",
        "highgate-funkybookendslinefeed",
        "highgate-onecountfeedingfiveclubwhynotandfunkybookends",
        "highgate-onecountfeedingwhynotandfiveclubwhynot",
        "highgate-onecountfeedingmaybeandfiveclubwhynot",
        "highgate-londonsburning",
        "highgate-rotator",
        "highgate-pistons",
        "highgate-flyingtrapeze",
        "highgate-benzenering",
        "highgate-dosado",
        "highgate-threeleafclover",
        "highgate-stickytriangle",
        "highgate-theweave",
        "highgate-dresserdrawerweave",
        "highgate-fourpersonfourcountdropbackline1",
        "highgate-fourcounty",
        "highgate-fourcountdropbacky",
        "highgate-fourpersonfourcountdropbackline2",
        "highgate-threecounty",
        "highgate-fourcounttrapeze",
        "highgate-alloutcivilwarfeed",
        "highgate-bookendsinvertedbookendsbox",
        "highgate-doubleurbanterrorfeed",
        "highgate-threecountaccommodation",
        "highgate-lowfatchocolatebox",
        "highgate-bookshelf",
        "highgate-chocolatebookshelf",
        "highgate-countdownaccommodation1",
        "highgate-countdownaccommodation2",
        "highgate-jimsthreecounty",
        "highgate-rowrowrowyourboat",
        "highgate-rowrowrowyourboatambidextrousvariation",
        "highgate-frerejacques",
        "highgate-fourpersonthreecountrickspicnic",
        "highgate-fourpersontwocountrickspicnic",
        "highgate-fourpersononecountrickspicnic",
        "highgate-funkybookendsparsnipbox",
        "highgate-tenclubonecounttrianglewithsingleanddoublepasses1",
        "highgate-tenclubonecounttrianglewithsingleanddoublepasses2",
        "highgate-tenclubpasspassselftriangle",
        "highgate-rotatingy",
        "highgate-thirteenclubtwocountfeed",
        "highgate-thirteenclubpasspassselffeed",
        "highgate-thirteenclubonecountfeedingfourcount",
        "highgate-thirteenclubonecountfeedingthreecount",
        "highgate-thirteenclubonecountfeedingtwocount",
        "highgate-thirteenclubonecountfeedingpasspassself",
        "highgate-thirteenclubonecountfeedingchocolatebar",
        "highgate-thirteenclubonecountfeedingbookends",
        "highgate-thirteenclubonecountfeedinginvertedbookends",
        "highgate-thirteenclubdoubleurbanterrorfeed",
        "highgate-thirteenclubthreecountaccommodation",
        "highgate-thirteenclublowfatchocolatebox",
        "highgate-thirteenclubbookshelf",
        "highgate-thirteenclubchocolatebookshelf",
        "highgate-thirteenclubcountdownaccommodation",
        "highgate-karamazovshuffle",
        "highgate-rotatingλ",
        "highgate-cyclone",
        "highgate-seattleshuffle",
        "highgate-itsagoodonecountdownweave",
        "highgate-gorillaweave",
        "highgate-thirteenclubpasspassselffeedingfrenchthreecount",
        "highgate-thirteenclubpasspasshefffeedingthreecount",
        "highgate-thirteenclub972feedingthreecount",
        "highgate-thirteenclub972feedingfrenchthreecount",
        "highgate-onecountfeeding972",
        "highgate-thirteenclubwhynotfeedingfivecountpopcorn",
        "highgate-thirteenclubmaybefeedingfivecountpopcorn",
        "highgate-thirteenclubfunkybookendsfeedingwhynot",
        "highgate-thirteenclubfunkybookendsfeedingmaybe",
        "highgate-thirteenclubmartinsonecountfeedingfivecountpopcorn",
        "highgate-thirteenclubmartinsonecountfeedingfunkybookends",
        "highgate-thirteenclubonecountfeedingwhynot",
        "highgate-thirteenclubonecountfeedingmaybe",
        "highgate-thirteenclubonecountfeedingmartinsonecount",
        "highgate-thirteenonecountlinefeedfeedingfourcountandtwocount",
        "highgate-thirteenclubonecountlinefeedfeedingfrenchthreecount",
        "highgate-thirteenclubparsniplinefeedfeedingfivecountpopcorn",
        "highgate-thirteenclubfunkybookendslinefeedfeedingfivecoun-cde45e90c5",
        "highgate-thirteenclubmartinsonecountlinefeedfeedingfiveco-44febdca3e",
        "highgate-thirteenclubonecountlinefeedfeedingfivecountpopcornandparsnip",
        "highgate-thirteenclubonecountlinefeedfeedingwhynotandfivecountpopcorn",
        "highgate-thirteenclubonecountlinefeedfeedingmaybeandfivecountpopcorn",
        "highgate-thirteenclubweave",
        "highgate-thirteenclubdresserdrawerweave",
        "highgate-thirteenclubgorillaweave",
        "highgate-sevenclubjimstwocountvariation1",
        "highgate-sevenclubjimstwocountvariation2",
        "highgate-sevenclubjimstwocountvariation3",
        "highgate-sevenclubwhynotvariation",
        "highgate-notlikely",
        "highgate-maybenot",
        "highgate-fivecountpopcornvsfiveclubwhynot",
        "highgate-fiveclubwhynotvsfivecountpopcorn",
    };

    [Test]
    public void Encode_RoundTripsNodesHandsAndExplicitCausalEdgesWithoutCachedHeights()
    {
        var state = CausalDiagramEditorState.CreateSiteswap531();
        state.Repetitions = 7;
        state.SnapEnabled = false;
        state.TimelineWidth = 720m;
        state.LaneSpacing = 135m;
        state.ActiveJugglerId = "A";
        state.Throws[0] = state.Throws[0] with { Hand = "L" };

        var token = state.Encode();

        token.Should().StartWith("j4.");
        token.Length.Should().BeLessThan(2_048);
        CausalDiagramEditorState.TryDecode(token, out var decoded).Should().BeTrue();
        decoded.Should().BeEquivalentTo(state);
    }

    [Test]
    public void PresetCatalog_ContainsLoadableAsynchronousAndSynchronousStarters()
    {
        CausalDiagramEditorPresets
            .VisibleAsynchronous.Select(preset => preset.Id)
            .Should()
            .BeEquivalentTo(
                "siteswap-3",
                "feeding-7-5",
                "jugglers-1",
                "jugglers-2",
                "jugglers-3",
                "jugglers-4"
            );
        CausalDiagramEditorPresets
            .VisibleSynchronous.Select(preset => preset.Id)
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be("jugglers-2-synchronous-4");
        foreach (var preset in CausalDiagramEditorPresets.All)
        {
            var state = preset.Create();
            state.PresetId.Should().Be(preset.Id);
            state.Throws.Should().NotBeEmpty();
            state.CausalEdges.Should().NotBeEmpty();
            CausalDiagramEditorState
                .TryDecode(state.Encode(), out var decoded)
                .Should()
                .BeTrue(preset.Id);
            decoded.Should().BeEquivalentTo(state);
        }
    }

    [Test]
    public void PresetCatalog_EveryProductionPresetCreatesAValidPattern()
    {
        var invalidPresetIds = CausalDiagramEditorPresets
            .All.Where(preset => !preset.Create().IsValidPattern())
            .Select(preset => preset.Id)
            .ToArray();

        invalidPresetIds.Should().BeEmpty(string.Join(", ", invalidPresetIds));
    }

    [Test]
    public void WorkInProgressPatterns_RemainSelectableAndEditableButAreNotValidPresets()
    {
        var presets = CausalDiagramEditorPresets.VisibleHighgateWorkInProgress.ToArray();

        presets.Should().HaveCount(58);
        foreach (var preset in presets)
        {
            var state = preset.Create();
            state.PresetId.Should().Be(preset.Id);
            state.IsValidForEditor().Should().BeTrue(preset.Id);
            state.IsValidPattern().Should().BeFalse(preset.Id);
            CausalDiagramEditorState
                .TryDecode(state.Encode(), out var decoded)
                .Should()
                .BeTrue(preset.Id);
            decoded!.IsValidPattern().Should().BeFalse(preset.Id);
        }
    }

    [Test]
    public void PresetCatalog_ImportsReviewedHighgatePatternsWithWorkInProgressSeparately()
    {
        var presets = CausalDiagramEditorPresets.VisibleHighgate.ToArray();
        var workInProgressIds = CausalDiagramEditorPresets
            .VisibleHighgateWorkInProgress.Select(preset => preset.Id)
            .ToArray();

        presets.Should().HaveCount(270);
        presets
            .Select(preset => preset.Id)
            .Concat(workInProgressIds)
            .Should()
            .BeEquivalentTo(ReviewedHighgatePresetIds);
    }

    [Test]
    public void IsValidPattern_RejectsUnbalancedProductionPresetAfterLoading()
    {
        var state = CausalDiagramEditorPresets
            .All.Single(preset => preset.Id == "siteswap-531")
            .Create();
        state.IsValidPattern().Should().BeTrue();

        state.CausalEdges.RemoveAt(0);

        state.IsValidForEditor().Should().BeTrue();
        state.IsValidPattern().Should().BeFalse();
    }

    [Test]
    public void TryCreateSiteswap_UsesCausalHeightMinusHandPeriodForPeriodicNodeEdges()
    {
        CausalDiagramEditorState
            .TryCreateSiteswap("531", "custom", out var state)
            .Should()
            .BeTrue();
        state!.CycleLength.Should().Be(6m);
        state
            .Throws.Select(item => item.Hand)
            .Should()
            .ContainInOrder("R", "L", "R", "L", "R", "L");
        state
            .Throws.Select(item => state.HeightOf(item.Id))
            .Should()
            .ContainInOrder(5m, 3m, 1m, 5m, 3m, 1m);
        state
            .CausalEdges.Should()
            .Contain(edge =>
                edge.FromThrowId == "A1" && edge.ToThrowId == "A4" && edge.CycleOffset == 0
            );
        state
            .CausalEdges.Should()
            .Contain(edge =>
                edge.FromThrowId == "A2" && edge.ToThrowId == "A3" && edge.CycleOffset == 0
            );
        state
            .CausalEdges.Should()
            .Contain(edge =>
                edge.FromThrowId == "A3" && edge.ToThrowId == "A2" && edge.CycleOffset == 0
            );
    }

    [TestCase("!")]
    [TestCase("A")]
    public void TryCreateSiteswap_RejectsCharactersOutsideNotationAlphabet(string notation)
    {
        CausalDiagramEditorState
            .TryCreateSiteswap(notation, "custom", out var state)
            .Should()
            .BeFalse();
        state.Should().BeNull();
    }

    [Test]
    public void TryCreateSiteswap_ZeroIsABeatNodeWithoutCausalThrowEdges()
    {
        CausalDiagramEditorState.TryCreateSiteswap("0", "custom", out var state).Should().BeTrue();
        state!.CycleLength.Should().Be(1m);
        state.HeightOf(state.Throws.Single().Id).Should().BeNull();
        state.CausalEdges.Should().BeEmpty();
        CausalDiagramEditorState.TryDecode(state.Encode(), out var decoded).Should().BeTrue();
        decoded.Should().BeEquivalentTo(state);
    }

    [Test]
    public void Polyrhythm33_CausalEdgesStayOnTheirOwnThrowSequence()
    {
        var state = CausalDiagramEditorState.CreateFeedingPattern(3, 3, "feeding-3-3");
        var edge = state.CausalEdges.Single(item => item.FromThrowId == "P1-1");
        var target = state.Throws.Single(node => node.Id == edge.ToThrowId);
        target.JugglerId.Should().Be("P1");
        (target.StartTime + edge.CycleOffset * state.CycleLength).Should().Be(-1m);
    }

    [Test]
    public void Polyrhythm23_UsesGlobalHeightsDerivedFromCausalArrows()
    {
        var state = CausalDiagramEditorState.CreateFeedingPattern(2, 3, "custom");
        state.CycleLength.Should().Be(6m);
        state.Throws.Count(node => node.JugglerId == "P1").Should().Be(2);
        state.Throws.Count(node => node.JugglerId == "P2").Should().Be(3);
        state
            .Throws.Where(node => node.JugglerId == "P1")
            .Select(node => node.StartTime)
            .Should()
            .Equal(0, 3);
        state
            .Throws.Where(node => node.JugglerId == "P2")
            .Select(node => node.StartTime)
            .Should()
            .Equal(0, 2, 4);
        state.Throws.Select(node => state.HeightOf(node.Id)).Should().Equal(3m, 3m, 2m, 2m, 2m);
    }

    [Test]
    public void CausalGraph_HasAtMostOneOutgoingAndOneIncomingPerPeriodicThrowAndCouplesHeightToTarget()
    {
        var state = CausalDiagramEditorState.CreateFeedingPattern(2, 3, "custom");
        state
            .CausalEdges.GroupBy(edge => edge.FromThrowId)
            .Should()
            .OnlyContain(group => group.Count() == 1);
        state
            .CausalEdges.GroupBy(edge => (edge.ToThrowId, edge.CycleOffset))
            .Should()
            .OnlyContain(group => group.Count() == 1);
        state.CausalEdges.Should().HaveCount(5);
        state.Throws.Select(node => state.HeightOf(node.Id)).Should().Equal(3m, 3m, 2m, 2m, 2m);
        CausalDiagramEditorState.TryDecode(state.Encode(), out var decoded).Should().BeTrue();
        decoded!
            .Throws.Select(node => decoded.HeightOf(node.Id))
            .Should()
            .Equal(3m, 3m, 2m, 2m, 2m);
    }

    [Test]
    public void MovingPastCausalTargetKeepsTargetNodeAndShiftsToValidCycleHeight()
    {
        var state = CausalDiagramEditorState.CreateFeedingPattern(2, 3, "custom");
        var edgeIndex = state.CausalEdges.FindIndex(edge => edge.FromThrowId == "P1-1");
        var edge = state.CausalEdges[edgeIndex];
        var sourceIndex = state.Throws.FindIndex(node => node.Id == edge.FromThrowId);
        state.Throws[sourceIndex] = state.Throws[sourceIndex] with { StartTime = 5.75m };
        state.NormalizeCausalEdgeOffsets();

        state.CausalEdges[edgeIndex].ToThrowId.Should().Be(edge.ToThrowId);
        state.CausalEdges[edgeIndex].CycleOffset.Should().BeGreaterThanOrEqualTo(edge.CycleOffset);
        state.HeightOf(state.Throws[sourceIndex].Id).Should().BeGreaterThanOrEqualTo(0m);
        CausalDiagramEditorState.TryDecode(state.Encode(), out var decoded).Should().BeTrue();
        decoded!
            .HeightOf(state.Throws[sourceIndex].Id)
            .Should()
            .Be(state.HeightOf(state.Throws[sourceIndex].Id));
    }

    [Test]
    public void TryDecode_RejectsMultipleOutgoingButAllowsTemporaryMultipleIncomingEdges()
    {
        var state = CausalDiagramEditorState.CreateSiteswapPattern("531");
        state.CausalEdges.Add(new EditorCausalEdge("duplicate-out", "A1", "A2", 2));
        CausalDiagramEditorState.TryDecode(state.Encode(), out _).Should().BeFalse();

        state = CausalDiagramEditorState.CreateEmpty();
        state.CycleLength = 3m;
        state.Throws =
        [
            new EditorThrow("A1", "A", "R", 0m),
            new EditorThrow("A2", "A", "L", 1m),
            new EditorThrow("A3", "A", "R", 2m),
        ];
        state.CausalEdges =
        [
            new EditorCausalEdge("e1", "A1", "A2", 0),
            new EditorCausalEdge("duplicate-in", "A3", "A2", 0),
        ];
        state.NormalizeCausalEdgeOffsets();
        CausalDiagramEditorState.TryDecode(state.Encode(), out var decoded).Should().BeTrue();
        decoded!.CausalEdges.Should().Contain(edge => edge.Id == "duplicate-in");
    }

    [Test]
    public void FeedingSameJugglerTarget_UsesThatRowsReturnPeriod()
    {
        const string pattern = "A: 2>A, 2>A, 2>A\nB: 2>B, 2>B, 2>B";
        CausalDiagramEditorState
            .TryCreateFeeding(pattern, out var state, out var error)
            .Should()
            .BeTrue(error);
        var edge = state!.CausalEdges.Single(item => item.FromThrowId == "P1-1");
        var target = state.Throws.Single(node => node.Id == edge.ToThrowId);
        target.JugglerId.Should().Be("P1");
        (target.StartTime + edge.CycleOffset * state.CycleLength).Should().Be(0m);
    }

    [Test]
    public void TryCreateFeeding_AppliesHandSequenceAndHalfLocalBeatOffsetPerRow()
    {
        const string pattern = "A: (LLRR) 2>A, 2>A, 2>A, 2>A\nB: (RL 0.5) 2>B, 2>B";
        CausalDiagramEditorState
            .TryCreateFeeding(pattern, out var state, out var error)
            .Should()
            .BeTrue(error);

        state!
            .Throws.Where(node => node.JugglerId == "P1")
            .Select(node => node.Hand)
            .Should()
            .Equal("L", "L", "R", "R");
        state
            .Throws.Where(node => node.JugglerId == "P1")
            .Select(node => node.StartTime)
            .Should()
            .Equal(0m, 1m, 2m, 3m);
        state
            .Throws.Where(node => node.JugglerId == "P2")
            .Select(node => node.Hand)
            .Should()
            .Equal("R", "L");
        state
            .Throws.Where(node => node.JugglerId == "P2")
            .Select(node => node.StartTime)
            .Should()
            .Equal(1m, 3m);
    }

    [Test]
    public void TryCreateFeeding_WithoutHandSequenceKeepsAlternatingDefault()
    {
        CausalDiagramEditorState
            .TryCreateFeeding("A: 2>A, 2>A, 2>A", out var state, out var error)
            .Should()
            .BeTrue(error);
        state!.Throws.Select(node => node.Hand).Should().Equal("R", "L", "R");
    }

    [Test]
    public void FeedingTallThrow_ResolvesCausalTargetAcrossManyPatternCycles()
    {
        const string pattern = "A: 100>B\nB: 1>A, 1>A, 1>A, 1>A, 1>A, 1>A, 1>A";
        CausalDiagramEditorState
            .TryCreateFeeding(pattern, out var state, out var error)
            .Should()
            .BeTrue(error);
        var edge = state!.CausalEdges.Single(item => item.FromThrowId == "P1-1");
        edge.CycleOffset.Should().Be(99);
        var target = state.Throws.Single(node => node.Id == edge.ToThrowId);
        target.JugglerId.Should().Be("P2");
        (target.StartTime + edge.CycleOffset * state.CycleLength).Should().Be(698m);
    }

    [Test]
    public void TryCreateFeeding_LoadsIndependentThrowCountsAndCreatesExplicitEdges()
    {
        const string pattern =
            "Alice: 7>Bob, 7>Bob, 7>Bob, 7>Bob, 7>Bob, 7>Bob, 7>Bob\nBob: 5>Alice, 5>Alice, 5>Alice, 5>Alice, 5>Alice";
        CausalDiagramEditorState
            .TryCreateFeeding(pattern, out var state, out var error)
            .Should()
            .BeTrue(error);
        state!.CycleLength.Should().Be(35m);
        state.Throws.Count(item => item.JugglerId == "P1").Should().Be(7);
        state.Throws.Count(item => item.JugglerId == "P2").Should().Be(5);
        state
            .Throws.Where(item => item.JugglerId == "P1")
            .Select(item => item.StartTime)
            .Should()
            .Equal(0, 5, 10, 15, 20, 25, 30);
        state
            .Throws.Where(item => item.JugglerId == "P2")
            .Select(item => item.StartTime)
            .Should()
            .Equal(0, 7, 14, 21, 28);
        state.CausalEdges.Should().HaveCount(12);
    }

    [Test]
    public void LargeCustomGraph_StaysWithinDeeplinkLimitAndRoundTrips()
    {
        var jugglers = Enumerable
            .Range(1, 8)
            .Select(i => new EditorJuggler($"P{i}", $"Jongleur {i}"))
            .ToList();
        var throws = Enumerable
            .Range(0, 8)
            .SelectMany(person =>
                Enumerable
                    .Range(0, 60)
                    .Select(beat => new EditorThrow(
                        $"P{person + 1}-{beat + 1}",
                        $"P{person + 1}",
                        beat % 2 == 0 ? "R" : "L",
                        beat
                    ))
            )
            .ToList();
        var edges = throws
            .Select(
                (node, index) =>
                {
                    var target = throws[(index + 1) % throws.Count];
                    return new EditorCausalEdge(
                        $"E{index}",
                        node.Id,
                        target.Id,
                        target.StartTime < node.StartTime ? 1 : 0
                    );
                }
            )
            .ToList();
        var state = new CausalDiagramEditorState
        {
            CycleLength = 60,
            Repetitions = 20,
            Jugglers = jugglers,
            Throws = throws,
            CausalEdges = edges,
            ActiveJugglerId = "P8",
        };
        var token = state.Encode();
        token.Length.Should().BeLessThan(50_000);
        CausalDiagramEditorState.TryDecode(token, out var decoded).Should().BeTrue();
        decoded.Should().BeEquivalentTo(state);
    }

    [TestCase("")]
    [TestCase("j1.dW5zdXBwb3J0ZWQ=")]
    [TestCase("j2.dW5zdXBwb3J0ZWQ=")]
    [TestCase("j3.dW5zdXBwb3J0ZWQ=")]
    [TestCase("eyJWZXJzaW9uIjoyfQ")]
    public void TryDecode_RejectsMalformedOrUnsupportedTokens(string token)
    {
        CausalDiagramEditorState.TryDecode(token, out var decoded).Should().BeFalse();
        decoded.Should().BeNull();
    }

    [Test]
    public void TryDecode_RejectsTokensThatExceedTheInputLimit() =>
        CausalDiagramEditorState
            .TryDecode("j4." + new string('A', 300_001), out _)
            .Should()
            .BeFalse();

    [Test]
    public void TryDecode_RejectsDuplicateIdentifiersAndInvalidEdges()
    {
        var invalid = CausalDiagramEditorState.CreateSiteswap531();
        invalid.Throws[1] = invalid.Throws[1] with { Id = invalid.Throws[0].Id };
        CausalDiagramEditorState.TryDecode(invalid.Encode(), out _).Should().BeFalse();

        var dangling = CausalDiagramEditorState.CreateSiteswap531();
        dangling.CausalEdges[0] = dangling.CausalEdges[0] with { ToThrowId = "missing" };
        CausalDiagramEditorState.TryDecode(dangling.Encode(), out _).Should().BeFalse();
    }

    [Test]
    public void MoveStart_ChangesOnlyTheSelectedNodeAndPreservesExplicitEdges()
    {
        var state = CausalDiagramEditorState.CreateSiteswap531();
        var item = state.Throws[0];
        var otherNode = state.Throws[1];
        var edges = state.CausalEdges.ToArray();
        var moved = EditorThrowPositioning.MoveStart(state, item, 1.137m);
        moved.StartTime.Should().Be(1.25m);
        state.Throws[1].Should().Be(otherNode);
        state.CausalEdges.Should().BeEquivalentTo(edges);
    }

    [Test]
    public void MoveStart_AllowsFreePositionWhenSnapIsOff()
    {
        var state = CausalDiagramEditorState.CreateSiteswap531();
        state.SnapEnabled = false;
        var moved = EditorThrowPositioning.MoveStart(state, state.Throws[0], 1.137m);
        moved.StartTime.Should().Be(1.137m);
    }
}
