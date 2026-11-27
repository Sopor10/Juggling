using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class CausalDiagramEditorStateTests
{
    private static readonly string[] ReviewedHighgatePresetIds =
    {
        "highgate-8978226",
        "highgate-9788226",
        "highgate-7882962",
        "highgate-8897226",
        "highgate-9268827",
        "highgate-9682782",
        "highgate-7892682",
        "highgate-7786266",
        "highgate-7266867",
        "highgate-7966266",
        "highgate-9667266",
        "highgate-972",
        "highgate-972alternativestart",
        "highgate-passselfzap",
        "highgate-75666",
        "highgate-75756",
        "highgate-zapzapheff",
        "highgate-56685",
        "highgate-78456",
        "highgate-75864",
        "highgate-74685",
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
        "highgate-777786",
        "highgate-777867",
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
        "highgate-7778686",
        "highgate-7786786",
        "highgate-7777786",
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
        "highgate-97586",
        "highgate-78956",
        "highgate-79685",
    };

    [Test]
    public void Encode_RoundTripsNodesHeightsHandsAndExplicitCausalEdges()
    {
        var state = CausalDiagramEditorState.CreateSiteswap531();
        state.Repetitions = 7;
        state.SnapEnabled = false;
        state.TimelineWidth = 720m;
        state.LaneSpacing = 135m;
        state.ActiveJugglerId = "A";
        state.Throws[0] = state.Throws[0] with { Hand = "L", Height = 3.25m };

        var token = state.Encode();

        token.Should().StartWith("j4.");
        token.Length.Should().BeLessThan(2_048);
        CausalDiagramEditorState.TryDecode(token, out var decoded).Should().BeTrue();
        decoded.Should().BeEquivalentTo(state);
    }

    [Test]
    public void J4_RoundTripsSignedOffsetsAndExplicitThrowHeight()
    {
        var state = CausalDiagramEditorState.CreateSiteswapPattern("3");
        var edge = state.CausalEdges[0];
        state.CausalEdges[0] = edge with { CycleOffset = -1 };
        var sourceIndex = state.Throws.FindIndex(node => node.Id == edge.FromThrowId);
        state.Throws[sourceIndex] = state.Throws[sourceIndex] with
        {
            Height = 5m,
            HeightIsExplicit = true,
        };

        state.Throws[sourceIndex] = EditorThrowPositioning.MoveStart(
            state,
            state.Throws[sourceIndex],
            1m
        );
        state.SynchronizeHeights();
        var token = state.Encode();
        token.Should().StartWith("j4.");
        CausalDiagramEditorState.TryDecode(token, out var decoded).Should().BeTrue();
        decoded!.CausalEdges[0].CycleOffset.Should().Be(-1);
        decoded.Throws[sourceIndex].Height.Should().Be(5m);
        decoded.Throws[sourceIndex].HeightIsExplicit.Should().BeTrue();
        decoded.Throws[sourceIndex].StartTime.Should().Be(1m);
    }

    [Test]
    public void SynchronizeHeights_PreservesSignedOffsetsAndClampsInvalidDerivedHeightForValidation()
    {
        var state = CausalDiagramEditorState.CreateSiteswapPattern("3");
        state.CausalEdges[0] = state.CausalEdges[0] with { CycleOffset = -2 };

        state.SynchronizeHeights();

        state.CausalEdges[0].CycleOffset.Should().Be(-2);
        state
            .Throws.First(node => node.Id == state.CausalEdges[0].FromThrowId)
            .Height.Should()
            .Be(0m);
        CausalDiagramEditorState.TryDecode(state.Encode(), out _).Should().BeFalse();
    }

    [Test]
    public void TryDecode_MigratesPreviousCompactAndJsonLinksToExplicitCausalGraph()
    {
        const string oldState =
            "{\"v\":1,\"p\":\"feeding75\",\"c\":7,\"r\":2,\"s\":true,\"q\":0.25,\"j\":[[\"P1\",\"Ada\"]],\"t\":[[\"t1\",\"P1\",\"L\",1,3,\"P1\"]]}";
        var compact = "j1." + Base64Url(Encoding.UTF8.GetBytes(oldState));
        const string legacyJson =
            "{\"Version\":1,\"PresetId\":\"feeding75\",\"CycleLength\":7,\"Repetitions\":2,\"SnapEnabled\":true,\"SnapStep\":0.25,\"Jugglers\":[{\"Id\":\"P1\",\"Name\":\"Ada\"}],\"Throws\":[{\"Id\":\"t1\",\"JugglerId\":\"P1\",\"Hand\":\"L\",\"StartTime\":1,\"LandingTime\":3,\"TargetJugglerId\":\"P1\"}]}";

        foreach (var token in new[] { compact, Base64Url(Encoding.UTF8.GetBytes(legacyJson)) })
        {
            CausalDiagramEditorState.TryDecode(token, out var decoded).Should().BeTrue();
            decoded!.Version.Should().Be(4);
            decoded.ActiveJugglerId.Should().Be("P1");
            decoded.PresetId.Should().Be("feeding-7-5");
            decoded.TimelineWidth.Should().Be(560m);
            decoded.LaneSpacing.Should().Be(105m);
            decoded.Throws[0].Height.Should().Be(2m);
            decoded.CausalEdges.Should().ContainSingle();
            CausalDiagramEditorState
                .TryDecode(decoded.Encode(), out var roundTrip)
                .Should()
                .BeTrue();
            roundTrip.Should().BeEquivalentTo(decoded);
        }
    }

    [Test]
    public void TryDecode_MigratesJ3CompactLinksToJ4WithoutRejectingLegacyShape()
    {
        var current = CausalDiagramEditorState.CreateSiteswapPattern("531");
        var root = JsonNode
            .Parse(Encoding.UTF8.GetString(FromBase64Url(current.Encode()[3..])))!
            .AsObject();
        root["v"] = 3;
        foreach (var throwNode in root["t"]!.AsArray())
            throwNode!.AsArray().RemoveAt(5);
        var legacyToken = "j3." + Base64Url(Encoding.UTF8.GetBytes(root.ToJsonString()));

        CausalDiagramEditorState.TryDecode(legacyToken, out var migrated).Should().BeTrue();
        migrated!.Version.Should().Be(4);
        migrated.Encode().Should().StartWith("j4.");
    }

    [Test]
    public void TryDecode_PrunesConflictingEdgesFromPreviousJ2LinksAndKeepsTheirNodes()
    {
        var state = CausalDiagramEditorState.CreateSiteswapPattern("531");
        var payload = Encoding.UTF8.GetString(FromBase64Url(state.Encode()[3..]));
        var root = JsonNode.Parse(payload)!.AsObject();
        root["v"] = 2;
        foreach (var throwNode in root["t"]!.AsArray())
            throwNode!.AsArray().RemoveAt(5);
        var edges = root["e"]!.AsArray();
        edges.Add(new JsonArray("legacy-out", "A2", "A1", 5));
        edges.Add(new JsonArray("legacy-in", "A1", "A2", 5));
        var token = "j2." + Base64Url(Encoding.UTF8.GetBytes(root.ToJsonString()));

        CausalDiagramEditorState.TryDecode(token, out var migrated).Should().BeTrue();
        migrated!.Throws.Should().HaveCount(6);
        migrated.CausalEdges.Should().HaveCount(6);
        migrated.CausalEdges.Select(edge => edge.FromThrowId).Should().OnlyHaveUniqueItems();
        migrated.CausalEdges.Select(edge => edge.ToThrowId).Should().OnlyHaveUniqueItems();
        migrated.Encode().Should().StartWith("j4.");
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
    public void PresetCatalog_EveryPresetCreatesAValidPattern()
    {
        var invalidPresetIds = new List<string>();
        foreach (var preset in CausalDiagramEditorPresets.All)
        {
            var state = preset.Create();
            if (state.IsValidPattern())
                continue;
            CausalDiagramEditorJifAdapter.TryMap(state, out _, out var error);
            invalidPresetIds.Add($"{preset.Id}: {error}");
        }

        invalidPresetIds.Should().BeEmpty(string.Join("; ", invalidPresetIds));
    }

    [Test]
    public void WorkInProgressPatterns_RemainSelectableAndEditableButAreNotValidPresets()
    {
        var presets = CausalDiagramEditorPresets.VisibleHighgateWorkInProgress.ToArray();

        presets.Should().HaveCount(31);
        foreach (var preset in presets)
        {
            var state = preset.Create();
            state.PresetId.Should().Be(preset.Id);
            state.IsValidForEditor().Should().BeTrue(preset.Id);
            state.IsValidPattern().Should().BeFalse(preset.Id);
        }
    }

    [Test]
    public void IsValidPattern_RejectsAProductionPresetWithUnbalancedCausalNodes()
    {
        var state = CausalDiagramEditorPresets
            .All.Single(preset => preset.Id == "highgate-74685")
            .Create();
        state.IsValidPattern().Should().BeTrue();

        state.CausalEdges.RemoveAt(0);

        state.IsValidPattern().Should().BeFalse();
        state.IsValidForEditor().Should().BeTrue();
        CausalDiagramEditorState.TryDecode(state.Encode(), out var decoded).Should().BeTrue();
        decoded!.IsValidForEditor().Should().BeTrue();
        decoded.IsValidPattern().Should().BeFalse();
    }

    [Test]
    public void IsValidPattern_Highgate74685CanBeMappedToAPeriodicJuggle()
    {
        var state = CausalDiagramEditorPresets
            .All.Single(preset => preset.Id == "highgate-74685")
            .Create();

        state.IsValidPattern().Should().BeTrue();
        CausalDiagramEditorJifAdapter.TryMap(state, out _, out var error).Should().BeTrue(error);
    }

    [Test]
    public void IsValidPattern_RejectsAnInvalidSiteswapInAProductionEditorState()
    {
        var state = CausalDiagramEditorState.CreateSiteswapPattern("97531", "siteswap-97531");
        var heights = new[] { 7, 4, 5, 8, 5, 7, 4, 5, 8, 5 };
        var ordered = state.Throws.OrderBy(item => item.StartTime).ToArray();
        state.CycleLength.Should().Be(10m);

        state.Throws = ordered
            .Select((item, index) => item with { Height = heights[index], HeightIsExplicit = true })
            .ToList();
        state.CausalEdges = ordered
            .Select(
                (item, beat) =>
                {
                    var targetTime = beat + heights[beat] - 2;
                    var targetBeat =
                        ((targetTime % ordered.Length) + ordered.Length) % ordered.Length;
                    var cycleOffset = (int)Math.Floor((decimal)targetTime / ordered.Length);
                    return new EditorCausalEdge(
                        $"e-{item.Id}",
                        item.Id,
                        ordered[targetBeat].Id,
                        cycleOffset
                    );
                }
            )
            .ToList();

        state.IsValidForEditor().Should().BeTrue();
        state.IsValidPattern().Should().BeFalse();
    }

    [Test]
    public void HighgateThirteenClubOneCountCountdown_MatchesSourceRowDestinations()
    {
        var state = CausalDiagramEditorPresets
            .All.Single(preset => preset.Id == "highgate-thirteenclubonecountfeedingcountdown")
            .Create();
        var targets = state.CausalEdges.ToDictionary(
            edge => edge.FromThrowId,
            edge => edge.ToThrowId
        );

        targets["P3-2"].Should().Be("P2-4");
        targets["P3-3"].Should().Be("P4-5");
        targets["P3-5"].Should().Be("P4-7");
        targets["P3-6"].Should().Be("P4-8");
        targets["P3-8"].Should().Be("P4-2");
        targets["P4-1"].Should().Be("P3-2");
        targets["P4-2"].Should().Be("P4-3");
        targets["P4-4"].Should().Be("P3-5");
        targets["P4-6"].Should().Be("P3-7");
        targets["P4-7"].Should().Be("P3-8");
        state.CausalEdges.Single(edge => edge.FromThrowId == "P3-8").CycleOffset.Should().Be(1);
    }

    [Test]
    public void PresetCatalog_ImportsTheReviewedHighgateCausalDiagrams()
    {
        var presets = CausalDiagramEditorPresets.VisibleHighgate.ToArray();
        var workInProgressIds = CausalDiagramEditorPresets
            .VisibleHighgateWorkInProgress.Select(preset => preset.Id)
            .ToArray();

        presets.Should().HaveCount(319);
        presets
            .Select(preset => preset.Id)
            .Concat(workInProgressIds)
            .Should()
            .BeEquivalentTo(ReviewedHighgatePresetIds);
        var threeCount = presets.Single(preset => preset.Id == "highgate-three-count").Create();
        threeCount.CycleLength.Should().Be(6m);
        threeCount.Jugglers.Select(item => item.Name).Should().Equal("Anne", "Ben");
        threeCount.Throws.Should().HaveCount(12);
        threeCount.CausalEdges.Should().HaveCount(12);
        threeCount.Throws.Select(item => item.Height).Should().OnlyContain(height => height == 3m);
        foreach (
            var patternId in new[]
            {
                "highgate-whynot",
                "highgate-whynotalternativestart",
                "highgate-notwhy",
                "highgate-notwhyalternativestart",
                "highgate-maybe",
                "highgate-maybealternativestart",
            }
        )
        {
            var periodFivePattern = presets.Single(preset => preset.Id == patternId).Create();
            periodFivePattern.Throws.Should().HaveCount(20, patternId);
            periodFivePattern.CausalEdges.Should().HaveCount(20, patternId);
            periodFivePattern.Throws.Should().OnlyContain(item => item.HeightIsExplicit, patternId);
            periodFivePattern.IsValidForEditor().Should().BeTrue(patternId);
        }
        var fiveClub = presets
            .Single(preset => preset.Id == "highgate-fiveclubthreecount")
            .Create();
        fiveClub.CycleLength.Should().Be(14m);
        fiveClub.Throws.Should().HaveCount(28);
        fiveClub.CausalEdges.Should().HaveCount(12);
        fiveClub.Throws.Select(item => item.Height).Should().Contain(3.5m);
        var selfDouble = presets.Single(preset => preset.Id == "highgate-aselfdouble").Create();
        selfDouble.Throws.Single(item => item.Id == "A-R3").Height.Should().Be(4m);
        selfDouble.Throws.Single(item => item.Id == "A-L3").Height.Should().Be(0m);
        var doubleHold = presets.Single(preset => preset.Id == "highgate-doublehold42").Create();
        doubleHold.Throws.Single(item => item.Id == "A-R1").Height.Should().Be(3m);
        doubleHold.Throws.Single(item => item.Id == "A-R1").HeightIsExplicit.Should().BeTrue();
        doubleHold.Throws.Single(item => item.Id == "B-R1").Height.Should().Be(4m);
        doubleHold.Throws.Single(item => item.Id == "B-R1").HeightIsExplicit.Should().BeTrue();
        doubleHold
            .CausalEdges.Should()
            .ContainSingle(edge => edge.FromThrowId == "A-R1" && edge.ToThrowId == "B-R1");
        doubleHold
            .CausalEdges.Should()
            .ContainSingle(edge => edge.FromThrowId == "B-R1" && edge.ToThrowId == "A-L2");
        doubleHold
            .CausalEdges.Should()
            .ContainSingle(edge => edge.FromThrowId == "A-L2" && edge.ToThrowId == "A-R1");
        doubleHold.CausalEdges.Select(edge => edge.FromThrowId).Should().NotContain("B-L2");
        var jimThreeCount = presets
            .Single(preset => preset.Id == "highgate-jimsthreecount")
            .Create();
        jimThreeCount.CycleLength.Should().Be(12m);
        jimThreeCount.Throws.Single(item => item.Id == "A-2").Hand.Should().Be("R");
        jimThreeCount.Throws.Single(item => item.Id == "B-2").Hand.Should().Be("L");
        var frenchThreeCount = presets
            .Single(preset => preset.Id == "highgate-frenchthreecount")
            .Create();
        frenchThreeCount.CycleLength.Should().Be(6m);
        frenchThreeCount.Throws.Single(item => item.Id == "B-1").StartTime.Should().Be(0.5m);
        frenchThreeCount.Throws.Single(item => item.Id == "A-1").Height.Should().Be(3.5m);
        frenchThreeCount.Throws.Single(item => item.Id == "A-3").Height.Should().Be(4m);
        var lateSingle = presets.Single(preset => preset.Id == "highgate-alatesingle").Create();
        lateSingle.Throws.Single(item => item.Id == "A-L1").Height.Should().Be(0m);
        lateSingle.Throws.Single(item => item.Id == "B-R1").Height.Should().Be(4m);
        var lateDouble = presets.Single(preset => preset.Id == "highgate-alatedouble").Create();
        lateDouble.Throws.Single(item => item.Id == "A-L1").Height.Should().Be(0m);
        lateDouble.Throws.Single(item => item.Id == "B-R1").Height.Should().Be(4m);
        foreach (var preset in presets)
        {
            var state = preset.Create();
            state.Throws.Should().NotBeEmpty("preset {0}", preset.Id);
            state.CausalEdges.Should().NotBeEmpty("preset {0}", preset.Id);
            state.Encode().Should().StartWith("j4.", "preset {0}", preset.Id);
        }
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
        state.Throws.Select(item => item.Height).Should().ContainInOrder(5m, 3m, 1m, 5m, 3m, 1m);
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
        state.Throws.Should().ContainSingle().Which.Height.Should().Be(0m);
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
    public void Polyrhythm75_UsesIndependentTrackCyclesAndThrowHeights()
    {
        var state = CausalDiagramEditorState.CreateFeeding75();
        var aToB = state.CausalEdges.Single(edge => edge.FromThrowId == "P1-1");
        var aTarget = state.Throws.Single(node => node.Id == aToB.ToThrowId);
        aTarget.JugglerId.Should().Be("P1");
        aTarget.Id.Should().Be("P1-7");
        aToB.CycleOffset.Should().Be(-1);
        (aTarget.StartTime + aToB.CycleOffset * state.CycleLength).Should().Be(-5m);

        var bToA = state.CausalEdges.Single(edge => edge.FromThrowId == "P2-1");
        var bTarget = state.Throws.Single(node => node.Id == bToA.ToThrowId);
        bTarget.JugglerId.Should().Be("P2");
        bTarget.Id.Should().Be("P2-5");
        bToA.CycleOffset.Should().Be(-1);
        (bTarget.StartTime + bToA.CycleOffset * state.CycleLength).Should().Be(-7m);
        state.Throws.Should().OnlyContain(item => item.Height == 1m);
        state.CycleLength.Should().Be(35m);
        state.Throws.Count(node => node.JugglerId == "P1").Should().Be(7);
        state.Throws.Count(node => node.JugglerId == "P2").Should().Be(5);
        state
            .Throws.Where(node => node.JugglerId == "P1")
            .Select(node => node.StartTime)
            .Should()
            .Equal(0, 5, 10, 15, 20, 25, 30);
        state
            .Throws.Where(node => node.JugglerId == "P2")
            .Select(node => node.StartTime)
            .Should()
            .Equal(0, 7, 14, 21, 28);
    }

    [Test]
    public void TryDecode_MigratesSavedLegacy75PresetFromEventCountsToTwoBeatThrows()
    {
        var legacy = CausalDiagramEditorState.CreateFeeding75();
        legacy.PresetId = "custom";
        foreach (var row in new[] { (JugglerId: "P1", Count: 7), (JugglerId: "P2", Count: 5) })
        {
            var nodes = legacy
                .Throws.Where(node => node.JugglerId == row.JugglerId)
                .OrderBy(node => node.StartTime)
                .ToArray();
            for (var beat = 0; beat < nodes.Length; beat++)
            {
                var target = nodes[(beat - 2 + nodes.Length) % nodes.Length];
                var offset = beat < 2 ? 0 : 1;
                var edgeIndex = legacy.CausalEdges.FindIndex(edge =>
                    edge.FromThrowId == nodes[beat].Id
                );
                legacy.CausalEdges[edgeIndex] = legacy.CausalEdges[edgeIndex] with
                {
                    ToThrowId = target.Id,
                    CycleOffset = offset,
                };
            }
        }
        legacy.Throws = legacy
            .Throws.Select(node => node with { Height = node.JugglerId == "P1" ? 7m : 5m })
            .ToList();

        CausalDiagramEditorState.TryDecode(legacy.Encode(), out var migrated).Should().BeTrue();
        migrated!.PresetId.Should().Be("feeding-7-5");
        migrated.Throws.Should().OnlyContain(node => node.Height == 2m);
        migrated
            .CausalEdges.Should()
            .OnlyContain(edge => edge.FromThrowId == edge.ToThrowId && edge.CycleOffset == 0);
    }

    [Test]
    public void CausalGraph_HasAtMostOneOutgoingAndOneIncomingPerPeriodicThrowAndCouplesHeightToTarget()
    {
        var state = CausalDiagramEditorState.CreateFeeding75();
        state
            .CausalEdges.GroupBy(edge => edge.FromThrowId)
            .Should()
            .OnlyContain(group => group.Count() == 1);
        state
            .CausalEdges.GroupBy(edge => (edge.ToThrowId, edge.CycleOffset))
            .Should()
            .OnlyContain(group => group.Count() == 1);
        state.CausalEdges.Should().HaveCount(12);

        var edge = state.CausalEdges[0];
        var sourceIndex = state.Throws.FindIndex(node => node.Id == edge.FromThrowId);
        var oldHeight = state.Throws[sourceIndex].Height;
        var source = state.Throws[sourceIndex];
        state.Throws[sourceIndex] = source with { StartTime = source.StartTime + 0.25m };
        state.SynchronizeHeights();
        state.CausalEdges[0].ToThrowId.Should().Be(edge.ToThrowId);
        state.Throws[sourceIndex].Height.Should().Be(oldHeight - 0.05m);
        CausalDiagramEditorState.TryDecode(state.Encode(), out var decoded).Should().BeTrue();
        decoded!.Throws[sourceIndex].Height.Should().Be(state.Throws[sourceIndex].Height);
    }

    [Test]
    public void MovingPastCausalTargetClampsBeforeBreakingDerivedHeight()
    {
        var state = CausalDiagramEditorState.CreateFeeding75();
        var edgeIndex = state.CausalEdges.FindIndex(edge => edge.FromThrowId == "P1-1");
        var edge = state.CausalEdges[edgeIndex];
        var sourceIndex = state.Throws.FindIndex(node => node.Id == edge.FromThrowId);
        var moved = EditorThrowPositioning.MoveStart(state, state.Throws[sourceIndex], 6.75m);
        state.Throws[sourceIndex] = moved;
        state.SynchronizeHeights();

        state.CausalEdges[edgeIndex].ToThrowId.Should().Be(edge.ToThrowId);
        state.CausalEdges[edgeIndex].CycleOffset.Should().Be(edge.CycleOffset);
        state.Throws[sourceIndex].Height.Should().BeGreaterThanOrEqualTo(0m);
        CausalDiagramEditorState.TryDecode(state.Encode(), out _).Should().BeTrue();
        CausalDiagramEditorState.TryDecode(state.Encode(), out var decoded).Should().BeTrue();
        decoded!.Throws[sourceIndex].Height.Should().Be(state.Throws[sourceIndex].Height);
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
            new EditorThrow("A1", "A", "R", 0m, 0m),
            new EditorThrow("A2", "A", "L", 1m, 0m),
            new EditorThrow("A3", "A", "R", 2m, 0m),
        ];
        state.CausalEdges =
        [
            new EditorCausalEdge("e1", "A1", "A2", 0),
            new EditorCausalEdge("duplicate-in", "A3", "A2", 0),
        ];
        state.SynchronizeHeights();
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
                        beat,
                        2m
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
    [TestCase("j3.dW5zdXBwb3J0ZWQ=")]
    [TestCase("j2.%%%not-base64%%")]
    [TestCase("eyJWZXJzaW9uIjoyfQ")]
    public void TryDecode_RejectsMalformedOrUnsupportedTokens(string token)
    {
        CausalDiagramEditorState.TryDecode(token, out var decoded).Should().BeFalse();
        decoded.Should().BeNull();
    }

    [Test]
    public void TryDecode_RejectsTokensThatExceedTheInputLimit() =>
        CausalDiagramEditorState
            .TryDecode("j2." + new string('A', 300_001), out _)
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

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] FromBase64Url(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        base64 += new string('=', (4 - base64.Length % 4) % 4);
        return Convert.FromBase64String(base64);
    }
}
