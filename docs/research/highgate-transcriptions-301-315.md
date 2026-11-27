# Highgate transcription batch: 2017 printed pp. 301–315

Work file: [highgate-transcriptions-301-315.json](highgate-transcriptions-301-315.json). Source: Aidan Burns, *The Highgate Collection*, [2017 expanded PDF](https://jugglingedge.com/pdf/highgate.pdf), body PDF pp. 302–316; compared with the [2014 PDF](https://passing.zone/wp-content/uploads/highgate.pdf). The 2014 PDF page footer and physical PDF page agree for the pages cited below.

## Accepted graphs

The JSON contains 26 records: the nine original accepted patterns plus 17 diagram transcriptions added in this batch. Every record has a stable inventory ID, event nodes, a complete set of causal edges, source pages, and direct state validation.

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
| `highgate-thirteenclubpasspassselffeedingfrenchthreecount` | Pass pass self feeding French three count | 301 | 216 | 24/24 |
| `highgate-thirteenclubpasspasshefffeedingthreecount` | Pass pass heff feeding three count | 301 | 216 | 24/24 |
| `highgate-thirteenclub972feedingthreecount` | 972 feeding three count | 303 | 217 | 24/24 |
| `highgate-thirteenclub972feedingfrenchthreecount` | 972 feeding French three count | 303 | 217 | 24/24 |
| `highgate-onecountfeeding972` | One count feeding 972 | 303 | 217 | 24/24 |
| `highgate-thirteenclubwhynotfeedingfivecountpopcorn` | Why Not? feeding Five Count Popcorn | 304 | 218 | 40/40 |
| `highgate-thirteenclubmaybefeedingfivecountpopcorn` | Maybe feeding Five Count Popcorn | 304 | 218 | 40/40 |
| `highgate-thirteenclubfunkybookendsfeedingwhynot` | Funky Bookends feeding Why Not? | 306 | 219 | 40/40 |
| `highgate-thirteenclubfunkybookendsfeedingmaybe` | Funky Bookends feeding Maybe | 306 | 219 | 40/40 |
| `highgate-thirteenclubmartinsonecountfeedingfivecountpopcorn` | Martin’s one count feeding Five Count Popcorn | 306 | 219 | 40/40 |
| `highgate-thirteenclubmartinsonecountfeedingfunkybookends` | Martin’s one count feeding Funky Bookends | 308 | 220 | 40/40 |
| `highgate-thirteenclubonecountfeedingwhynot` | One count feeding Why Not? | 308 | 220 | 40/40 |
| `highgate-thirteenclubonecountfeedingmaybe` | One count feeding Maybe | 308 | 220 | 40/40 |
| `highgate-thirteenclubonecountfeedingmartinsonecount` | One count feeding Martin’s one count | 310 | 221 | 40/40 |
| `highgate-thirteenonecountlinefeedfeedingfourcountandtwocount` | One count line feed, feeding Four Count and Two Count | 315 | 225 | 16/16 |
| `highgate-thirteenclubonecountlinefeedfeedingfrenchthreecount` | One count line feed, feeding French Three Count | 315 | 225 | 18/18 |
| `highgate-thirteenclubparsniplinefeedfeedingfivecountpopcorn` | Parsnip line feed, feeding Five Count Popcorn | 315–316 | 225 | 40/40 |

For the original nine records, I compared the printed throw rows and hand order to the diagram arrows in the 2017 page image, then checked the corresponding 2014 page image. For the 17 additions, I transcribed the printed throw rows, start phases and route directions. I checked row hand order and pass directions against the 2017 diagrams, reconstructed periodic endpoints from recipient/action/hand timing, then compared titles, rows, phases, routes, and diagrams with the 2014 edition. The 17 are present on 2014 pp. 216–221 and 225, with no sequence or diagram differences found. In the added JSON records, `sourceRows` stores a normalized per-throw transcription; forms such as `double:Ben:straight` encode a double pass to Ben with a straight route. `edgeMethod` records the endpoint rule, and the focused test recomputes each endpoint/offset from the token, phase, and route. The state records the written phase offsets and one event per listed throw. Single and double passes use their corresponding arrival intervals; self events connect to the next throw by the same juggler. Pass angle, formation, and starting club inventory remain source notes rather than editor state.

The focused test in `Highgate301To315StandaloneRecordValidationTests` builds a `CausalDiagramEditorState` for every record, calls `SynchronizeHeights()`, and requires `IsValidForEditor()` with all offsets preserved. It checks unique record and throw IDs, one outgoing edge per throw, resolvable endpoints, and the explicit height/time equation. Source-sequence assertions check the countdown feed and each of the 17 added graphs directly against the printed target rows and routes.

### Countdown feed revalidation

Rechecking the countdown graph against the row sequences and both page diagrams found that the previous graph had copied Anne's destinations onto Clare's and Daniel's rows. The standalone record now has these source sequences:

- Clare: Ben, Ben, Daniel, Ben, Daniel, Daniel, Ben, Daniel.
- Daniel: Clare, self, self, Clare, self, Clare, Clare, self.

The corrected Clare edges point to `P2-3`, `P2-4`, `P4-5`, `P2-6`, `P4-7`, `P4-8`, `P2-1` at offset 1, and `P4-2` at offset 1. Daniel's pass edges now point to Clare's corresponding opposite-hand events; his self throws continue along the Daniel row. The central preset with the same ID was compared against the corrected source graph and already matches it; `HighgateThirteenClubOneCountCountdown_MatchesSourceRowDestinations` checks the key Clare and Daniel routes in the runtime catalog.

## What the model can encode for zip, heff, and pass spins

The editor state has no separate `ActionType` field. It stores throw nodes (hand, time, derived height) and causal edges (source, destination, cycle offset). Action height and route determine the destination event; source notes retain zip/heff labels and club inventory.

- [Modern Club Passing notation](https://modernpassing.com/4a-4hsw-notation.html) defines zip as hand-across without rotation (4-handed siteswap `2`, solo siteswap `1`) and heff as a double-spin self to the same hand (4-handed `8`, solo `4`). A temporary state check confirmed a graph edge deriving height 1 with offset −1 and a same-hand self edge deriving height 4; both states passed `IsValidForEditor()` and retained their offsets. This verifies numeric/causal representability, not an automatic zip/heff parser.
- The editor also accepts pass heights 4 and 5; the accepted box records include double and triple passes. A double/triple here is represented as one causal throw edge with its height and endpoint, not as multiple independent club objects on one node.
- The source notation (`zip`, `heff`, straight/crossing, double/triple) therefore needs to stay in each entry's source notes. The graph can encode timing and causality, but does not store a separate label for the action's name or club inventory.

## Coverage and edition comparison

All 17 added graphs are also present in the 2014 edition: pp. 216–221 cover the pass-pass, 972, one-count and period-five feeds; p. 225 contains the three line feeds. I compared their names, start phases, written rows, pass directions, and diagrams with the matching 2014 pages. I found no differences. The parsnip line-feed diagram begins on 2017 printed p. 315 and its written rows continue on p. 316; the 2014 edition has the complete pattern on p. 225.

Within printed pp. 301–315, all named cooperative pattern diagrams with periodic throw rows are transcribed. The period-five section introduction on p. 304 describes interchangeable substitutions but is not itself a pattern. None of these pages describes a solo manipulator, takeout, hijack, or lo-jack role. Zip and heff are actions within cooperative feeds here. No inventory or central preset file was changed.
