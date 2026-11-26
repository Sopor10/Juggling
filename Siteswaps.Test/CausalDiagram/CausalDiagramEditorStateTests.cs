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
        "highgate-fiveclubonecount",
        "highgate-jims-one-count-async",
        "highgate-jims-two-count-async",
        "highgate-frenchthreecount",
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
        "highgate-tenclubonecountfeedingcountdown",
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
        "highgate-londonsburning",
        "highgate-fourpersonfourcountdropbackline1",
        "highgate-fourcounty",
        "highgate-fourcountdropbacky",
        "highgate-fourpersonfourcountdropbackline2",
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
        "highgate-thirteenclubfourcountbox",
        "highgate-thirteenclubdoubleurbanterrorfeed",
        "highgate-thirteenclubthreecountaccommodation",
        "highgate-thirteenclublowfatchocolatebox",
        "highgate-thirteenclubbookshelf",
        "highgate-thirteenclubchocolatebookshelf",
        "highgate-thirteenclubcountdownaccommodation",
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

        token.Should().StartWith("j3.");
        token.Length.Should().BeLessThan(2_048);
        CausalDiagramEditorState.TryDecode(token, out var decoded).Should().BeTrue();
        decoded.Should().BeEquivalentTo(state);
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
            decoded!.Version.Should().Be(3);
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
    public void TryDecode_PrunesConflictingEdgesFromPreviousJ2LinksAndKeepsTheirNodes()
    {
        var state = CausalDiagramEditorState.CreateSiteswapPattern("531");
        var payload = Encoding.UTF8.GetString(FromBase64Url(state.Encode()[3..]));
        var root = JsonNode.Parse(payload)!.AsObject();
        root["v"] = 2;
        var edges = root["e"]!.AsArray();
        edges.Add(new JsonArray("legacy-out", "A2", "A1", 5));
        edges.Add(new JsonArray("legacy-in", "A1", "A2", 5));
        var token = "j2." + Base64Url(Encoding.UTF8.GetBytes(root.ToJsonString()));

        CausalDiagramEditorState.TryDecode(token, out var migrated).Should().BeTrue();
        migrated!.Throws.Should().HaveCount(6);
        migrated.CausalEdges.Should().HaveCount(6);
        migrated.CausalEdges.Select(edge => edge.FromThrowId).Should().OnlyHaveUniqueItems();
        migrated.CausalEdges.Select(edge => edge.ToThrowId).Should().OnlyHaveUniqueItems();
        migrated.Encode().Should().StartWith("j3.");
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
    public void PresetCatalog_EveryPresetCreatesAValidEditorState()
    {
        var invalidPresetIds = CausalDiagramEditorPresets
            .All.Where(preset => !preset.Create().IsValidForEditor())
            .Select(preset => preset.Id)
            .ToArray();

        invalidPresetIds
            .Should()
            .BeEmpty(
                "every preset shown in the editor must be valid before serialization or decoding"
            );
    }

    [Test]
    public void PresetCatalog_ImportsTheReviewedHighgateCausalDiagrams()
    {
        var presets = CausalDiagramEditorPresets.VisibleHighgate.ToArray();

        presets.Should().HaveCount(136);
        presets.Select(preset => preset.Id).Should().BeEquivalentTo(ReviewedHighgatePresetIds);
        var threeCount = presets.Single(preset => preset.Id == "highgate-three-count").Create();
        threeCount.CycleLength.Should().Be(6m);
        threeCount.Jugglers.Select(item => item.Name).Should().Equal("Anne", "Ben");
        threeCount.Throws.Should().HaveCount(12);
        threeCount.CausalEdges.Should().HaveCount(12);
        threeCount.Throws.Select(item => item.Height).Should().OnlyContain(height => height == 3m);
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
            state.Encode().Should().StartWith("j3.", "preset {0}", preset.Id);
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
    public void MovingPastCausalTargetKeepsTargetNodeAndShiftsToValidCycleHeight()
    {
        var state = CausalDiagramEditorState.CreateFeeding75();
        var edgeIndex = state.CausalEdges.FindIndex(edge => edge.FromThrowId == "P1-1");
        var edge = state.CausalEdges[edgeIndex];
        var sourceIndex = state.Throws.FindIndex(node => node.Id == edge.FromThrowId);
        state.Throws[sourceIndex] = state.Throws[sourceIndex] with { StartTime = 6.75m };
        state.SynchronizeHeights();

        state.CausalEdges[edgeIndex].ToThrowId.Should().Be(edge.ToThrowId);
        state.CausalEdges[edgeIndex].CycleOffset.Should().BeGreaterThanOrEqualTo(edge.CycleOffset);
        state.Throws[sourceIndex].Height.Should().BeGreaterThanOrEqualTo(0m);
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
