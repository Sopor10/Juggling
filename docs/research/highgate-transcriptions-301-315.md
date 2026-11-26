# Highgate transcription batch: 2017 printed pp. 301–315

Work file: [highgate-transcriptions-301-315.json](highgate-transcriptions-301-315.json). Source: Aidan Burns, *The Highgate Collection*, [2017 expanded PDF](https://jugglingedge.com/pdf/highgate.pdf), body PDF pp. 302–316; compared with the [2014 PDF](https://passing.zone/wp-content/uploads/highgate.pdf). The 2014 PDF page footer and physical PDF page agree for the pages cited below.

## Accepted graphs

The JSON contains nine records: the four-count box and eight additional patterns. Every record has a stable inventory ID, event nodes, a complete set of causal edges, source pages, and direct state validation.

| Stable ID | Pattern | 2017 printed page | 2014 printed page | Causal edges |
|---|---|---:|---:|---:|
| `highgate-thirteenclubfourcountbox` | Thirteen club four count box | 310 | 221 | 32/32; diagrams checked in both editions |
| `highgate-thirteenclubonecountfeedingcountdown` | Thirteen club one count feeding countdown | 301 | 216 | 32/32 |
| `highgate-thirteenclubthreecountbox` | Thirteen club three count box | 311 | 222 | 24/24 |
| `highgate-thirteenclubthreecountboxvariation` | Thirteen club three count box variation | 311 | 222 | 48/48 |
| `highgate-thirteenclubtwocountbox` | Thirteen club two count box | 312 | 223 | 16/16 |
| `highgate-thirteenclubchocolatesquare` | Thirteen club chocolate square | 312 | 223 | 16/16 |
| `highgate-weirdpasspassselfbox` | Weird pass pass self box | 312 | 223 | 12/12 |
| `highgate-thirteenclubtwocountlinefeed` | Thirteen club two count line feed | 313 | 224 | 24/24 |
| `highgate-thirteencluburbanterrorfeedonecountlinefeed` | Urban terror feed (one count line feed) | 313 | 224 | 48/48 |

For each accepted record, I compared the printed throw rows and hand order to the diagram arrows in the 2017 page image, then checked the corresponding 2014 page image. The state records the written phase offsets and one event per listed throw. Single and double passes use their corresponding arrival intervals; self events connect to the next throw by the same juggler. Pass angle, formation, and starting club inventory remain source notes rather than editor state.

The component `CausalDiagramEditorState` was built directly for every record, followed by `SynchronizeHeights()` and `IsValidForEditor()`. All nine states returned valid and every serialized `CycleOffset` remained unchanged. Since these new stable IDs are not registered centrally, validation temporarily used the registered preset ID `highgate-three-count`; the exact registered-ID limitation and node/edge counts are recorded per entry. A completeness audit checks one outgoing causal edge for every throw node in all nine records, without duplicate or missing source IDs. The four-count box now has all 32 edges: eight pass edges plus 24 self-throw continuation edges. These self edges follow the visible horizontal continuation arrows and wrap to the next cycle at each row’s final throw.

## What the model can encode for zip, heff, and pass spins

The editor state has no separate `ActionType` field. It stores throw nodes (hand, time, derived height) and causal edges (source, destination, cycle offset). That still allows several special actions to be represented when the source diagram identifies the destination node:

- [Modern Club Passing notation](https://modernpassing.com/4a-4hsw-notation.html) defines zip as hand-across without rotation (4-handed siteswap `2`, solo siteswap `1`) and heff as a double-spin self to the same hand (4-handed `8`, solo `4`). A temporary state check confirmed a graph edge deriving height 1 with offset −1 and a same-hand self edge deriving height 4; both states passed `IsValidForEditor()` and retained their offsets. This verifies numeric/causal representability, not an automatic zip/heff parser.
- The editor also accepts pass heights 4 and 5; the accepted box records include double and triple passes. A double/triple here is represented as one causal throw edge with its height and endpoint, not as multiple independent club objects on one node.
- The source notation (`zip`, `heff`, straight/crossing, double/triple) therefore needs to stay in each entry's source notes. The graph can encode timing and causality, but does not store a separate label for the action's name or club inventory.

## Remaining named candidates and classification

I reviewed the full pp. 301–315 block. The descriptions and diagrams show cooperative feeds, boxes, and line feeds; I found no solo manipulator, takeout, hijack, or lo-jack role in this range. The following candidates are not accepted as graphs here because their exact arrow endpoints or full phase cycle have not been transcribed and checked. This is a source-specific gap, not a claim that the editor cannot represent zip/heff heights.

| 2017 printed pages | Pattern / stable inventory ID | Reason left out |
|---|---|---|
| 301–302 | Thirteen club pass pass self feeding French three count — `highgate-thirteenclubpasspassselffeedingfrenchthreecount`; Thirteen club pass pass heff feeding three count — `highgate-thirteenclubpasspasshefffeedingthreecount` | Heff actions; per-arrow destination and same-hand timing need an audit |
| 302–303 | Thirteen club 972 feeding three count — `highgate-thirteenclub972feedingthreecount`; Thirteen club 972 feeding French three count — `highgate-thirteenclub972feedingfrenchthreecount`; One count feeding 972 — `highgate-onecountfeeding972` | Zip/heff actions, unequal starts/hand assignments, and doubles; full edge audit remains |
| 303–309 | Thirteen club why not? feeding five count popcorn — `highgate-thirteenclubwhynotfeedingfivecountpopcorn`; maybe feeding five count popcorn — `highgate-thirteenclubmaybefeedingfivecountpopcorn`; funky bookends feeding why not?/maybe — `highgate-thirteenclubfunkybookendsfeedingwhynot`, `highgate-thirteenclubfunkybookendsfeedingmaybe`; Martin's one count feeding five count popcorn/funky bookends — `highgate-thirteenclubmartinsonecountfeedingfivecountpopcorn`, `highgate-thirteenclubmartinsonecountfeedingfunkybookends`; one count feeding why not?/maybe/Martin's one count — `highgate-thirteenclubonecountfeedingwhynot`, `highgate-thirteenclubonecountfeedingmaybe`, `highgate-thirteenclubonecountfeedingmartinsonecount` | Zip/heff sequences with several half-beat starts; no speculative token conversion |
| 314 | Thirteen one count line feed, feeding four count and two count — `highgate-thirteenonecountlinefeedfeedingfourcountandtwocount` | Different individual periods (four, four, four, and two events); exact endpoints need a full diagram read |
| 314 | Thirteen club one count line feed, feeding French three count — `highgate-thirteenclubonecountlinefeedfeedingfrenchthreecount` | Heff actions and phase details require exact destination audit |
| 314–315 | Thirteen club parsnip line feed, feeding five count popcorn — `highgate-thirteenclubparsniplinefeedfeedingfivecountpopcorn` | Zip actions and staggered starts require exact destination audit |

The period-five section heading on p. 303 is not itself a pattern. The inventory’s merged countdown/four-count row is resolved by retaining the stable IDs on the actual named headings. No inventory or central preset file was changed.
