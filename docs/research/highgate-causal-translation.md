# Highgate causal diagram translations

The editor imports curated Highgate entries from
[`highgate-causal-patterns.json`](../../Siteswaps.Components/CausalDiagram/Data/highgate-causal-patterns.json).
This JSON is the source dataset; `HighgateCausalDiagramPresets` converts its
throw nodes and causal edges into editor state, and the editor exposes them in a
separate **Highgate Collection** select group. The ordinary pattern generator
and its notation logic are not changed.

The source links are recorded per pattern in the JSON: the expanded edition is
[Aidan Burns's 2017 PDF](https://jugglingedge.com/pdf/highgate.pdf), and the
2014 edition is [the passing.zone PDF](https://passing.zone/wp-content/uploads/highgate.pdf).

## Completed translation

| Stable ID | Pattern | Source evidence | Representation |
| --- | --- | --- | --- |
| `highgate-three-count` | Three count | 2017 expanded PDF printed p. 19 (PDF p. 20); 2014 PDF p. 5 | Two jugglers, six beat positions each, and twelve causal arrows copied from the book's causal-diagram explanation. |
| `highgate-fiveclubthreecount` | Five club three count | 2017 printed p. 20 (PDF p. 21); 2014 PDF p. 6 | Half-beat phase offset, 3.5-beat floaty passes, explicit hold beats, and the repeating causal chain from the diagram and accompanying explanation. |
| `highgate-aselfdouble` | A self double (heff) | 2017 printed p. 21 (PDF p. 22); 2014 PDF p. 7 | Replace the two single self edges with one same-hand double edge; retain the empty intermediate beat as a node with no outgoing edge. |
| `highgate-jimsthreecount` | Jim's three count | 2017 printed p. 25 (PDF p. 26); 2014 PDF p. 11 | The source explicitly says to relabel the ordinary three-count diagram. The twelve hand labels are transcribed while preserving its causal edges. |
| `highgate-threecount-both-crossing` | Three count, both passes crossing (unnamed variation) | 2017 printed p. 25 (PDF p. 26); 2014 PDF p. 11 | Same causal graph with hand labels from the second, both-crossing variation. |
| `highgate-frenchthreecount` | French three count | 2017 printed p. 26 (PDF p. 27); 2014 PDF p. 12 | Anne's `pass, self, heff` and Ben's `heff, pass, self` sequence with a half-beat phase offset; direct graph preserves 3.5- and 4-beat throws. |
| `highgate-alatesingle` | A late single | 2017 printed p. 23 (PDF p. 24); 2014 PDF p. 9 | Ben substitutes a heff for the usual pass, then passes on the next beat; Anne's unlinked beat remains a hold before the late catch. |
| `highgate-alatedouble` | A late double | 2017 printed p. 21 (PDF p. 22); 2014 PDF p. 7 | Ben's usual pass is retargeted two beats later; Anne holds the intermediate beat, catches on her right, and then self-throws before the next pass. |
| `highgate-fiveclubonecount` | Five club one count | 2017 printed p. 40 (PDF p. 41); 2014 PDF p. 23 | Half-beat phase offset, pass/hold/hold rows, and all unconnected hold positions are preserved. |
| `highgate-zapholdself` | Zap hold self | 2017 printed p. 40 (PDF p. 41); 2014 PDF p. 23 | Zap, hold, and self actions use the source's half-beat phase and causal edges. |
| `highgate-onecountwithzaps` | One count with zaps | 2017 printed p. 41 (PDF p. 42); 2014 PDF p. 24 | Alternating hands, half-beat phase, and straight/crossing zap destinations are transcribed. |
| `highgate-parsnip`, `highgate-fiveclubwhynot`, `highgate-fiveclubnotwhy` | Parsnip, Five club why not, Five club not why | 2017 printed p. 42 (PDF p. 43); 2014 PDF p. 24 | Each five-action sequence repeats twice to restore hand parity; all 60 causal edges, phases, target hands, and cycle offsets match both editions. |
| `highgate-jims-one-count-async`, `highgate-jims-two-count-async` | Jim's one count and two count, asynchronous | 2017 printed pp. 49, 51 (PDF pp. 50, 52); 2014 PDF pp. 29–30 | Hand phases, holds, crossings, and the odd-period hand-parity repeats are represented as separate event nodes and causal edges. |
| `highgate-jimsthreecountasynchversion`, `highgate-synchronousonecount`, `highgate-synchronousonecountwithheffs` | Jim's three count (asynchronous), Synchronous one count, Synchronous one count with heffs | 2017 printed pp. 47, 49 (PDF pp. 48, 50); 2014 PDF pp. 27, 29 | Jim's source holds remain unconnected nodes; synchronous same-time hand events and all source causal edges are preserved. |
| `highgate-asailorwenttosea`, `highgate-countdown`, `highgate-geometriccountdown`, `highgate-fizzbuzz`, `highgate-jimsfizzbuzzsynchronousversion` | A sailor went to sea, Countdown, Geometric countdown, Fizz buzz, Jim's synchronous Fizz buzz | 2017 printed pp. 74–76 (PDF pp. 75–77); 2014 PDF pp. 46–47 | Repeated hand phases, hurry, holds, and synchronous pairs are represented; each causal row and cycle boundary is source-checked. |
| `highgate-779667withcrossingdoubles`, `highgate-777966withstraightdoubles`, `highgate-8x6x68x8x6withcrossingsinglesandstraightdoubles` | 779667 with crossing doubles, 777966 with straight doubles, (8x,6x)(6,8x)(8x,6) | 2017 printed pp. 83–84 (PDF pp. 84–85); 2014 PDF p. 53 | Straight/crossing singles and doubles, hands, and cycle offsets are represented in source-checked six-beat graphs. |
| Four seven-club patterns from printed p. 82 | Three count on doubles; three count on singles (techno); four count (triple self); four count on doubles | 2017 PDF p. 83; 2014 PDF p. 52 | All 48 nodes and edges preserve source hands, phases, causal destinations, and cycle offsets. |
| Four seven-club style patterns from printed pp. 101 and 103 | Why Not, Not Why, and two Maybe style patterns | 2017 PDF pp. 102, 104; 2014 PDF pp. 65–66 | All 112 nodes and edges are imported with explicit Prechac heights and verified source links. |
| 23 presets from 2017 pp. 61, 66–73 | Funky Bookends/Parsnip comparisons, period-seven patterns, Jim's three-count variations, 972 variants, Brolly/zap patterns | 2014 diagrams visually checked on printed pp. 35, 39–45; exact links are in the data | Each graph keeps its hand labels, phase, holds, and causal destinations. The full titles and source links are in the JSON. |
| Six presets from 2017 printed pp. 106–108 | Three Seven club Not Why variations, Seven club Maybe, Copenhagen countdown, Oslo countdown | 2014 printed pp. 68–69 | Arrow destinations, hands, phase, sequence, and notation are visually checked against both editions. |
| Eight three-person feed presets from 2017 printed pp. 116–118 | Two count, Pass pass self, Gorilla, One count feeds to Pass pass self/Three count, Bookends/Inverted bookends, Suburban terror, Countdown, and Civil war | 2014 PDF pp. 76–78 | All 156 nodes and edges were transcribed from the arrows and action rows, checked in both editions, and pass the standalone and catalog validity tests. |
| Four period-five feeds from 2017 printed pp. 127–129 | Martin's one count feeding Maybe; Why Not?/Funky Bookends; Why Not?/Parsnip; Five club Why Not?/Martin's one count | 2014 PDF pp. 84–85 | All 120 nodes and edges preserve half-beat phases, hands, endpoints, explicit heights, and cycle offsets; each graph passes standalone and catalog validity tests. |
| Five Pulsar-family patterns from 2017 printed pp. 143–145 | Pulsar and ambidextrous three-count, pass-pass-self, pass-self-pass, and one-count feeds | 2014 PDF pp. 95–96 | Destination rows are checked against each edge; links target each recipient's next event, preserving source height 3 and cycle wraps. |
| `highgate-quasar` | Quasar feast | 2017 printed p. 142 (PDF p. 143); 2014 PDF p. 94 | Three 15-event lanes repeat with opposite-hand parity as a 30-beat cycle; all 45 causal edges match the diagram and source rows. |
| Three slow-fast patterns from 2017 printed pp. 78–79 | Four count vs three count; Three count vs two count; Two count vs one count | 2014 printed p. 50 | Event phases at 3/4, 2/3, and 1/2 beat preserve the source diagrams' receiver arrows and cycle offsets. |
| Seven independent triangle patterns from 2017 printed pp. 161, 164–165 | Mixed-count triangle, Tarim's and Extended Tarim's triangles, two one-count mixed triangles, and two countdown triangles | 2014 PDF pp. 110, 112 | All destination lanes and offsets are checked against both editions; the seven graphs pass standalone and catalog validity tests. |
| Eight presets from 2017 printed pp. 94–97 | Seven count popcorn patterns with double passes, heffs, single passes, and triple-self variations | 2014 PDF pp. 61–63 | The seven-event cycles are doubled to preserve hand parity; all eight graphs pass editor validity and retain source offsets after height synchronization. |
| Three presets from 2017 printed pp. 76, 80 | Jim's Fizz Buzz and asynchronous Bookends/Brainstorm, plus Seven club one count on singles | 2014 Fizz Buzz / base pattern are related references; see per-pattern links | Full causal lanes and holds are retained; source offsets pass synchronization. |
| Six presets from 2017 printed pp. 58–60 | Why not? and Not why with alternative starts; Maybe and its alternative start | 2014 PDF pp. 32–33 | Ten-beat period-five sequences are doubled to preserve hand parity; all 120 edges retain their source heights and cycle offsets. |
| `highgate-777786`, `highgate-777867` | Seven-club pass pass self variants | 2017 printed pp. 83–84; 2014 PDF p. 53 for 777786 | Both diagrams use a six-beat cycle with twelve events and twelve checked edges; the 2017-only variant is separately sourced. |
| `highgate-passpasszipfeedingthreecount`, `highgate-passpasszipfeedingfrenchthreecount` | Three-person pass pass zip feeds | 2017 printed p. 120; 2014 PDF p. 80 | Three lanes and 18 events per graph; backward zip edges and source heights are preserved, with the index/body page mismatch recorded. |
| Seven presets from 2017 printed pp. 88–90 | Classic six-count popcorn; six-, five-, and four-count popcorn variants with heffs/double passes | 2014 PDF pp. 57–59 | Every event and edge in the seven diagrams is transcribed and checked; patterns that require simultaneous throws remain outside the import. |
| `highgate-tenclubtarimstriangle` | Ten club Tarim’s triangle | 2017 printed pp. 219–220; 2014 p. 154 | All 18 receiver, lane-index, phase and cycle-wrap assertions match both diagrams. |
| Four ten-club box patterns from 2017 printed pp. 235, 237–238 | Mild Madness, Why Not, Maybe, and 972 boxes | 2014 pp. 165–167 | Zip/heff and timing are tested against the source rows; the 2017 Maybe/972 diagrams are missing, so their visible 2014 diagrams are cross-checked against 2017 action rows. |
| `highgate-tenclubonecountvariation1`, `highgate-tenclubonecountvariation2` | Ten club one-count variations #1 and #2 | 2017 printed pp. 218–219; 2014 pp. 153–154 | Both endpoint rows, phases, and wrap offsets are checked against the two diagrams and the source-row test. |
| Five four-person feed presets from 2017 printed pp. 224–225 | Two count, Pass pass self, and one-count feeds to Four/Three/Two count | 2014 pp. 157–158 | All four lanes and causal destinations are visually checked; each graph passes source-row and editor-validity tests. |
| `highgate-tenclubpasspasshefftriangle`, `highgate-tenclubfrenchthreecounttriangle` | Ten club pass-pass-heff and French three-count triangles | 2017 printed p. 216 / PDF p. 217; 2014 p. 151 | Corrected source endpoints follow the printed arrows and Prechac heights; both graphs pass the standalone and catalog validity tests. |
| `highgate-fourpersonparsnipfeedingfivecountpopcorn` | Four person Parsnip feeding five count popcorn | 2017 printed p. 229 / PDF p. 230; 2014 p. 161 | Forty events preserve half-beat phases and pass/self/zip/heff arrows, checked against action rows and editor timing. |
| `highgate-tenclubgorilla` | Ten club gorilla | 2017 printed p. 197 / PDF p. 198; 2014 PDF p. 138 | Alternating three-juggler routes, half-beat phases, explicit heights, and cycle offsets are checked against both diagrams. |
| `highgate-clockwiseonecountrunaround` | Clockwise one count runaround | 2017 printed p. 178 / PDF p. 179; 2014 p. 116 | The 54-event destination cycle is checked against source rows; the diagram graph is validated independently of the physical walking route. |
| Five three-person feast presets from 2017 printed pp. 135, 137, 140 | JED's nightmare, Anti Quasar, Double Three Count, Trifle, Trilogy | 2014 PDF pp. 90–93 | Three source sequences and two explicit lane-compositions become 288 events and edges; all are checked by the catalog validity test. |
| `highgate-threepersonfourcountdropbackline` | Three-person four-count drop-back line | 2017 printed p. 154; 2014 PDF p. 103 | The source says the causal diagram matches Four count triangle on ins; formation and walking path are recorded as spatial notes. |
| Four three-person runarounds from 2017 printed pp. 167–176 | Clockwise/anticlockwise two count and pass pass self runarounds | 2014 PDF pp. 114, 117–118 | Their cycle phases encode the changing feeder; source descriptions carry the walking direction. |
| `highgate-passselfzip` | Pass self zip (726) | 2017 printed pp. 40–41; 2014 PDF p. 23 | Twelve events and causal edges; the final Anne zip uses cycle offset −1, matching the backward loop in the source. |
| `highgate-jimsinvertedbookendsfeed` | Jim's inverted bookends feed | 2017 printed p. 122; 2014 printed p. 81 | Three juggler lanes, half-beat phase, full 34-beat hand period, holds, and pass destinations are transcribed. Holds and hurry are represented geometrically, not as text labels. |
| Eleven three-person feast presets from 2017 printed pp. 137–140 | Chocolate bar, Bookends, Inverted Bookends, Two count, and One count feasts | 2014 PDF pp. 91–93 | Each translated graph has three juggler lanes, direct pass destinations, and explicit cyclic event positions. All pass editor-state validation. |
| `highgate-threepersoncountdownfeast` | Three person countdown feast | 2017 printed p. 141; 2014 PDF p. 94 | Three 24-event sequences are matched against the source diagram; all pass destinations, hand alternation, and cycle offsets are checked. |
| Five triangle presets from 2017 printed pp. 154–157 | Four, Three, Two, and One count triangles; Pass pass self triangle | 2014 printed pp. 103–106 | Three juggler lanes, inside/outside pass destinations, hands, phases, and Prechac sequences are visually checked in both editions. |
| `highgate-twocountfeastrunaround` | Two count feast runaround | 2017 printed p. 171; 2014 PDF p. 116 | Three two-beat feed phases and their opposite-hand repeats are represented; walking route is described in the source note because the editor has no movement animation. |
| `highgate-trafficlightspattern` | Traffic lights pattern | 2017 printed p. 167; 2014 printed p. 114 | Full three-feeder cycle, all three lanes, hand labels, pass destinations, and cycle transition are transcribed. |
| `highgate-antiquasarrunaround`, `highgate-quasarrunaround` | Anti quasar runaround and Quasar runaround | 2017 printed pp. 174, 176; 2014 printed pp. 117–118 | Complete feeder rotation cycles with all throw events and causal edges; walking positions remain documented separately from the throw graph. |
| `highgate-anticlockwisepulsarrunaround` | Anticlockwise pulsar runaround | 2017 printed pp. 169–170 | Three-person feed/phase cycle and all 72 causal edges are transcribed. |
| `highgate-tenclubonecountfeedingcountdown` | Ten club one count feeding countdown | 2017 printed p. 196 | Three target sequences, phase offsets, and pass destinations are represented in a three-person eight-beat cycle. |
| `highgate-passpassselffeedingfrenchthreecount`, `highgate-passpasshefffeedingfrenchthreecount`, `highgate-asynchronouspasspasshefffeedingthreecount` | Three-person French and asynchronous three-count feeds | 2017 printed pp. 199–200; 2014 printed pp. 140–141 | All 18 throw events and causal edges per graph were checked against both editions, including half-beat lane phases and cycle offsets. |
| Four ten-club feed presets from 2017 printed pp. 192–193 | Two count on singles; one count feeding pass-pass-self/three count; Bookends/inverted Bookends; Suburban Terror | 2014 PDF pp. 134, 136 | Four complete graphs use only single-club edges; club inventory/color and walking routes remain source notes. |
| Thirteen four-person, mixed-count, box, and line-feed presets from 2017 printed pp. 226–240 | One-count feeders; Chocolate bar/Bookends/Countdown; Civil War; Mixed count; Pass pass self/Chocolate square/Bookends boxes; Two count and Urban Terror line feeds | 2014 PDF pp. 158–169 | Complete event rows and phase offsets are transcribed; simultaneous, zip, and hold variants outside these records remain separate model-boundary candidates. |
| Four ten-club triangle presets from 2017 printed pp. 211–213 | Four-, Three-, Two-, and asynchronous One-count triangles | 2014 printed pp. 148–149 | Ordinary nonsynchronous causal graphs; synchronous and multiple-club variants are tracked separately. |
| Eight ordinary feeds and line feeds from 2017 printed pp. 241–244 | Bookends, Mob Mildness/Neurosis, Parsnip, Funky Bookends, and three one-count feeds | 2014 PDF pp. 169–172 | Phase shifts, holds, and signed zip offsets are transcribed from the source diagrams. |
| Eleven four-person feast and feed presets from 2017 printed pp. 247–254 | Pass-pass-self and Bookends feasts; revolving feeds; Quasar; Fizz Buzz; Selfless Fizz Buzz; Aidan’s feasts #1–#4 | 2014 PDF pp. 174–179 | Full event rows, throw destinations, and cycle offsets are transcribed; transition/countdown edge cases remain documented separately. |
| Nineteen four-person Y, feed, box, accommodation, and song-round presets from 2017 printed pp. 256–270 | Three count Y; Rick’s picnics; trapeze and civil-war feeds; box/bookshelf families; Row, Row, Row Your Boat and Frère Jacques | 2014 PDF pp. 181–191 | All 19 standalone graphs preserve the diagrammed event rows, phases, causal destinations, and cycle offsets; spatial routes and club details remain in notation. |
| Three ten-club triangle presets from 2017 printed p. 215 | One count with single/double passes #1/#2; Pass pass self triangle | 2014 printed p. 150 | Row destinations and cycle links are transcribed; the data notes that the editor graph does not encode double-pass multiplicity or straight/crossed trajectory. |
| Four four-person feast presets from 2017 printed pp. 245–246 | Four-, Three-, Two-, and One-count feasts | 2014 PDF pp. 173–174 | All event rows, hand labels, and causal edges are cross-checked against both editions. |
| `highgate-londonsburning` | London's burning | 2017 printed p. 272; 2014 printed p. 192 | Four 24-event alternating-hand rows and phase-shifted song-round cycles are represented; spatial layout remains source notation. |
| Six periodic zip/gap patterns from 2017 printed pp. 22–33 and p. 31 | Four four one; Four four four zero; A self triple; Double zap zip (531); The Recipe | 2014 PDF pp. 8–17 and 18–19 | Backward causal offsets and explicit positive source heights are preserved in the graph; the cycle test validates all seven standalone records including Pass self zip. |
| `highgate-58552` | 58552 · five-club one-count with zaps | 2014 PDF p. 25; no matching 2017 body diagram | The 2014-only pattern is transcribed as a periodic graph with source phases and signed zip offsets. |
| Nine walking and formation patterns from 2017 printed pp. 279–285 | Rotator, Three Leaf Clover, Pistons, Flying Trapeze, Benzene Ring, Dosado, Sticky Triangle, The Weave, Dresser Drawer Weave | 2014 PDF pp. 197–202 | Repeating throws and role rotations are encoded across each full graph cycle; walking and spatial arrangement remain source notation. |
| Four four-person four-count route presets from 2017 printed pp. 257–259 | Drop-back line #1, Four count Y, Drop-back Y, Drop-back line #2 | 2014 PDF pp. 182–183 | Same route graph appears under multiple named formations; each catalog entry remains selectable, while spatial movement is noted separately. |
| Ten presets from 2017 printed pp. 293–299 | Rotating Y; nine asynchronous thirteen-club feeds from Two count through Inverted Bookends | 2014 PDF pp. 211–216 | All event phases, hands, and edges are represented; spatial movement cases are listed separately in the research note. |
| `highgate-thirteenclubonecountfeedingcountdown` | Thirteen club one count feeding countdown | 2017 printed p. 301; 2014 PDF p. 216 | Clare’s and Daniel’s outgoing destinations were corrected against both printed diagrams and are covered by a source-row regression test. |
| `highgate-thirteenclubfourcountbox` | Thirteen club four count box | 2017 printed p. 310; 2014 PDF p. 221 | Four event rows and eight pass routes are translated; the 2014 sequence is checked, while its diagram visual review remains open. |
| `highgate-sixcount`, `highgate-fourcount`, `highgate-twocount` | Right-handed six-club counts | 2017 printed p. 45 (PDF p. 46); 2014 PDF p. 26 | Diagram and `3p` plus self-throw notation transcribed as repeating causal edges. |
| `highgate-fivecount-ambidextrous`, `highgate-threecount-ambidextrous` | Ambidextrous six-club counts | 2017 printed p. 45 (PDF p. 46); 2014 PDF p. 26 | Same transcription; odd count lengths double to preserve hand parity. |
| `highgate-passpassself`, `highgate-chocolatebar`, `highgate-bookends`, `highgate-invertedbookends` | Named six-club patterns | 2017 printed p. 48 (PDF p. 49); 2014 PDF p. 28 | Diagram and Prechac sequence transcribed into per-beat edges. |

These source pages show the diagrams and name each repeating pattern. On the
count-pattern pages, a pass connects to the other juggler's next opposite-hand
beat; a self throw connects to the same juggler's next beat. The repeated
sequence and its rendered arrow graph were checked against both PDF editions.
The reviewed pages describe ordinary passing and contain no manipulation
instructions. The CSV marks the corresponding candidates as visually reviewed
and translated. Other inventory rows remain open or explicitly require an
editor-model extension.

The five-club three-count entry stores all beat positions, including positions
with no causal arrow. Highgate explains these as holds. A half-beat phase offset
and causal-edge-derived heights preserve the 3.5-beat passes; nodes with no
outgoing edge remain unlabelled in the editor, matching the source diagram's
hold convention.

## Reviewed but still open

The causal diagrams on 2017 printed pp. 20–29 (PDF pp. 21–30) and matching
2014 PDF pp. 6–15 have also been inspected. The early double, triple variants,
combination tricks, seven-club three count, double self (53), and triple-pass
hold (642) are ordinary passing patterns with explicit causal diagrams; these
remain open until their edges are transcribed and checked. French three count
and the late single/double are transcribed above. Signed cycle offsets and
explicit source heights are now supported by the editor; the ordinary zip/gap
patterns below remain open because their source edges still need transcription.
Simultaneous same-time throws remain a layout limitation:

- **Simultaneous heffs** has two events by the same juggler at one time, one in
  each hand. The editor has one timeline lane per juggler, so those nodes would
  overlap. Rendering needs separate hand lanes or an equivalent same-time
  layout first. The same issue affects Techno, Scratch your head, Oddz godz,
  and simultaneous one-count Brolly variants.
**Four four one**, **Four four four zero**, **The Recipe**, **A self triple**,
**Double zap zip (531)**, and the 2014-only **58552** now preserve their signed
arrow direction in periodic causal diagrams. Finite transition patterns and
remaining zip endpoints still need transcription; a gap must remain
distinguishable from a throw in the pattern description. The reviewed rows are ordinary
two-person passing unless separately classified otherwise.

| Six presets from 2017 printed pp. 286–293 | Karamazov Shuffle, Rotating λ, Cyclone, Seattle Shuffle, It's a Good One countdown weave, Gorilla Weave | 2014 PDF pp. 203–211 | Source rows, cyclic destinations, role changes, and both-edition page links are transcribed; spatial movement stays in notation. |
| Seventeen presets from 2017 printed pp. 301–316 | Thirteen-club pass-pass feeds, 972 feeds, period-five feeds, and one-count line feeds | 2014 PDF pp. 216–221, 225 | 2014 comparison found matching sequences and diagrams; mixed crossing/straight routes are recorded per throw. |

| Five presets from 2017 printed pp. 317–318 | Thirteen-club line feeds to Five count popcorn/Parsnip, Why Not, and Maybe | 2014 PDF pp. 226–227 | All five event rows, hands, recipients, endpoints, and cycle offsets are cross-checked in both editions. |

| `highgate-thirteenclubweave` | Thirteen club weave | 2017 printed p. 326; 2014 p. 234 | The 24-event cycle preserves all source rows, eight cross-lane passes, and repeating hand continuations. |

| Two presets from 2017 printed pp. 327, 329 | Thirteen club Dresser Drawer Weave; Thirteen club Gorilla Weave | 2014 pp. 235, 237 | Separate records preserve the 24- and 72-event source cycles; source/base graphs were cross-checked and pass validity tests. |

| Eleven periodic presets from 2017 printed pp. 63, 100, 105–106 | Not Likely, Maybe Not, two five-club Why Not/popcorn orders, three Jim’s two-count variations, Seven-club Why Not variation, and 97586/78956/79685 zap patterns | 2014 PDF pp. 34, 64, 67 for matching diagrams | All 11 cyclic graphs preserve explicit heights, holds, routes, endpoints, and offsets; the zap graphs use the documented 2.5-beat half-spin pass. |

## Current coverage

The inventory has 919 catalog candidates. At this checkpoint, 385 rows have visual source review: 347 translated rows map to 350 presets, 3 are retained with specific editor-model requirements, 22 still need their graph transcription, and 13 are section, alias, or combined-index entries rather than independent patterns. The other 534 candidates have not yet received page review. The slow-fast graphs on printed pp. 78–79 account for five of the translated rows and presets. Each new preset is checked
against `IsValidForEditor()` before it can pass the catalog test; the Highgate
loader also rejects data if height synchronization changes a source edge offset.

## Translation limits and next work

The JSON representation records diagram event times and causal edges directly,
so these translations preserve the source arrows without changing the general
pattern builder. Other Highgate pages contain holds, zips, walking, and other
non-throw events; each still needs a source-page review and a concrete model
decision. This is not a full translation of the collection: the 919-row catalog
remains a title/alias candidate list, and unreviewed entries must not be treated
as normal or supported patterns.
