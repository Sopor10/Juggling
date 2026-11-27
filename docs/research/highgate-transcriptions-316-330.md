# Highgate transcription batch: 2017 printed pages 316–330

Work file: [highgate-transcriptions-316-330.json](highgate-transcriptions-316-330.json). It now contains ten checked, importable graph records: the four standalone thirteen-club counts, five remaining line-feed variants, and Thirteen club weave. No central preset catalog, inventory, or test file was changed.

## Included patterns

| Stable inventory ID | Pattern | 2017 body page | 2014 PDF page | Graph |
|---|---|---|---|---|
| `highgate-thirteenclubfourcount` | Thirteen club four count | printed p. 319 / PDF p. 320 | p. 228 | 16 nodes / 16 causal edges |
| `highgate-thirteenclubthreecount` | Thirteen club three count | printed p. 319 / PDF p. 320 | p. 228 | 12 nodes / 12 causal edges |
| `highgate-thirteenclubtwocount` | Thirteen club two count | printed p. 320 / PDF p. 321 | p. 229 | 8 nodes / 8 causal edges |
| `highgate-thirteenclubonecount` | Thirteen club one count | printed p. 320 / PDF p. 321 | p. 229 | 4 nodes / 4 causal edges |

Each person gets one throw node per entry in their written sequence, including self throws. Every throw has a causal edge to the next event that receives that club, so each complete periodic graph has one edge per node. Self edges use height 3; pass edges use the Prechac heights 4, 3¾, 3½, and 3¼. Those heights, event phases, and cycle offsets make the arrow endpoints line up with the recipients' event nodes. There are no holds in these four sequences. The source's starting club inventory and the straight/crossing trajectory details are noted but are not state fields.

For the four-count pattern, the 2014 book also has a differently named `Thirteen club four count box` pattern on PDF p. 221. Its written sequences and routing differ, so it is not treated as an edition-equivalent alias. The corresponding standalone four-count, three-count, two-count, and one-count patterns were visually compared between the 2017 and 2014 sources on pp. 228–229; their phases, sequences, and routes match.

## State validation

The permanent source-record validator checks all ten IDs, unique throw IDs, one outgoing edge per throw, and valid edge endpoints, source destinations, and feasible edge heights. The first four count patterns assert the exact heights stated by their Prechac sequences. The five line-feed graphs and the weave also check each source row's recipient, hand route where annotated, endpoint, and arrival offset. All ten states pass `IsValidForEditor()` after height synchronization, which must preserve every source `cycleOffset`. The separate Runaway validator checks its four periodic source components; none is a complete standalone Runaway cycle.

- `Highgate316To330StandaloneRecordValidationTests`: 3 passed.
- `HighgateRunawayStandaloneStateValidationTests`: 2 passed.
- The ten source records are separate from the catalog data. Runaway's four cyclic states remain research components until the editor can represent their finite transitions.

## Additional line-feed diagrams transcribed

| Stable inventory ID | Pattern | 2017 body page | 2014 page | Graph |
|---|---|---|---|---|
| `highgate-thirteenclubfunkybookendslinefeedfeedingfivecoun-cde45e90c5` | Funky bookends line feed feeding five-count popcorn and five-club why-not | printed p. 316 / PDF p. 317 | p. 226 | 40 nodes / 40 edges |
| `highgate-thirteenclubmartinsonecountlinefeedfeedingfiveco-44febdca3e` | Martin's one-count line feed feeding five-count popcorn and why-not | printed p. 316 / PDF p. 317 | p. 226 | 40 nodes / 40 edges |
| `highgate-thirteenclubonecountlinefeedfeedingfivecountpopcornandparsnip` | One-count line feed feeding five-count popcorn and Parsnip | printed pp. 316–317 / PDF pp. 317–318 | p. 226 | 40 nodes / 40 edges |
| `highgate-thirteenclubonecountlinefeedfeedingwhynotandfivecountpopcorn` | One-count line feed feeding Why Not? and five-count popcorn | printed p. 317 / PDF p. 318 | p. 227 | 40 nodes / 40 edges |
| `highgate-thirteenclubonecountlinefeedfeedingmaybeandfivecountpopcorn` | One-count line feed feeding Maybe and five-count popcorn | printed p. 318 / PDF p. 319 | p. 227 | 40 nodes / 40 edges |

The records preserve each actor's source row, starting phase, first hand where specified, crossing/straight route, and each edge's arrival offset. Heff and zip remain named in the source-row metadata; their endpoints follow the diagram's same-hand/earlier-event conventions. All five diagrams and row sequences match the 2014 diagrams on pp. 226–227. Their static event and causal routing is represented; the editor still does not model line formations or club color/count inventories.

## Thirteen club weave transcribed

| Stable inventory ID | Pattern | 2017 body page | 2014 page | Graph |
|---|---|---|---|---|
| `highgate-thirteenclubweave` | Thirteen club weave | printed p. 326 / PDF p. 327 | p. 234 | 24 nodes / 24 causal edges |

The diagram has six event positions per juggler in a six-beat repeat. I traced all eight cross-lane edges and the remaining same-lane edges from the diagram image; the row metadata records the target juggler at every event. The 2014 figure is pixel-for-pixel the same causal-row drawing. The book also describes the moving figure-eight formation and feeder's left/middle/right sweep; those spatial paths are recorded as notes because the editor state has no formation field.

## Remaining patterns in the requested span

The `Thirteen club runaway` phase records and model boundary are documented in [highgate-runaway-transition-review.md](highgate-runaway-transition-review.md). Its inventory row is classified as requiring finite-transition support. The adjacent index chunk maps printed pp. 327–330 to 2017 PDF pp. 328–331 and 2014 pp. 235–238. It includes verified records for *Dresser drawer weave* and *Gorilla weave*. *It's a good one (countdown weave)* remains out because its cross-lane endpoints have not been confirmed. *Synchronous gorilla weave* was checked against both editions, but its same-time hand events still collide in the editor's current graph/table presentation; it is documented as a model/UI boundary rather than added as a selectable graph. The four printed-index pages themselves contain titles and locators, not additional pattern diagrams.

## Manipulation review

The included patterns and the visible candidate entries in this span describe ordinary passing, feeding, accommodation, boxes, and walking/weaving. The source does not describe an external manipulator, takeout, hijack, or lo-jack in this span. Walking and movement are not classified as manipulation.

## Source coverage

The requested body range is 2017 printed pp. 316–330, corresponding to physical PDF pp. 317–331. Each source page in this range was visually scanned for named patterns and diagrams. Every graph included in this file was compared with its 2017 diagram and explanatory row sequence; the five line feeds and four standalone counts were also compared with their corresponding 2014 diagrams. The 2017 expanded PDF is [jugglingedge.com/pdf/highgate.pdf](https://jugglingedge.com/pdf/highgate.pdf); the 2014 PDF is [passing.zone/wp-content/uploads/highgate.pdf](https://passing.zone/wp-content/uploads/highgate.pdf).
