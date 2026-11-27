# Highgate source audit: printed pp. 91–105

Sources: Aidan Burns, *The New Highgate Collection* (2017 expanded PDF), printed pp. 91–105 / PDF pp. 92–106; *The Highgate Collection* (2014), PDF pp. 58–67. The page diagrams and adjacent sequence text were checked visually in both PDFs where a matching 2014 diagram exists.

## Already translated and verified

The separate data file `highgate-transcriptions-91-105.json` contains eight seven-club popcorn graphs from 2017 printed pp. 94–97 (PDF pp. 95–98), matching the 2014 diagrams on pp. 61–63:

- `highgate-sevencountpopcornwithdoublepasses`
- `highgate-sevencountpopcornwithdoublepassesandheffs`
- `highgate-sevencountpopcornwithsinglepassesand534`
- `highgate-sevencountpopcornwithsinglepassesand453`
- `highgate-sevencountpopcornwithsinglepassesandheffs`
- `highgate-7778686`
- `highgate-7786786`
- `highgate-7777786`

These were already present in the runtime catalog; no duplicate import was needed. Each graph contains 28 event nodes and 28 edges, with exactly one outgoing edge per node and no orphan or duplicate source. The transcription nodes and edge triples match their central runtime records exactly. The catalog validity test passed for all presets, and the loader verifies that height synchronization preserves every source cycle offset.

## Additional ordinary popcorn graphs

| Stable ID | Pattern | 2017 source | 2014 source | Graph |
|---|---|---|---|---|
| `highgate-classicsixcountpopcorn` | Classic six count popcorn | printed p. 88 / PDF p. 89 | PDF p. 57 | 12 events and 12 causal edges over a six-beat cycle |
| `highgate-sixcountpopcornwithheffs` | Six count popcorn with heffs | printed p. 89 / PDF p. 90 | PDF p. 58 | 12 events and 12 edges over six beats |
| `highgate-fivecountpopcorn`, `highgate-fivecountpopcornwithheffs`, `highgate-fivecountpopcornwithdoublepassesandheffs` | Five count popcorn variants | printed p. 89 / PDF p. 90 | PDF p. 58 | Each graph has 20 events and 20 edges over ten beats |
| `highgate-fourcountpopcorn`, `highgate-fourcountpopcornwithheffs` | Four count popcorn variants | printed p. 90 / PDF p. 91 | PDF p. 59 | Each graph has 8 events and 8 edges over four beats |

The inventory index points this title at printed p. 91, but the body diagram and sequence are on printed p. 88 in the expanded 2017 PDF. I cite the diagram itself, not the index reference. Both editions show the same graph and per-juggler sequences. The Prechac sequence is `4p33353`: Anne's double pass, three self throws, triple-self, self; Ben starts one beat later and throws triple-self, self, double pass, then three self throws. The two double-pass edges go to the other juggler; the self and triple-self edges return to the same juggler. Throw heights 4, 3, and 5 determine the target event time as `source time + height - 2`; the target events and cycle wraps match the diagram. All source heights are explicit because the diagram's row positions are layout coordinates.

The six-count pattern and its heff variant use six throws per juggler; the four-count variants use four. The five-count variants have odd-length hand cycles, so each source sequence is repeated once and the graph uses ten beats to keep right/left hands alternating. For every record, a pass token targets the other juggler and self/heff/triple-self tokens target the same juggler. Source event heights determine the receiving event via `target time = source time + height - 2`; each target hand and wrap count matches the repeating diagram. All source heights are explicit because diagram row positions are layout coordinates. A focused NUnit test constructs all seven new graphs from the separate JSON, checks node/edge counts and source heights, verifies cycle-offset stability after `SynchronizeHeights()`, and calls `IsValidForEditor()`.

## Reviewed patterns that still need translation or a model decision

The reviewed popcorn pages around printed pp. 88–93 contain the six-count/five-count/four-count families, French three-count techno, and *The dark side of two count* with its practice and ambidextrous variants. The matching 2014 diagrams on pp. 57–60 confirm several sequences. `Classic six count popcorn` is now transcribed above. The three “dark side” diagrams use simultaneous two-hand throws; the current editor has one timeline lane per juggler, so a faithful graph would overlap nodes. The six-count techno/practice and synchronous bookends likewise need a layout/model decision before they can be treated as ordinary presets. The page also continues *Scratch your head*, already retained as a model-limit candidate.

Printed pp. 98–103 contain seven-club zap and heff-zap patterns, including *The holy grail aka Willy Wonka's one count*, *Heff zap heff*, *Seven club bookends with zaps*, and siteswap variants `97586`, `78956`, and `79685`. The 2017 diagrams and text show explicit zap/manipulation actions. These remain separate manipulation candidates; they are not imported as ordinary passing graphs. Their corresponding 2014 diagrams and sequences on p. 64 match the three numbered siteswaps and Oddz godz; the 2014 source also confirms that the collection has explicit manipulation-adjacent zaps.

Printed pp. 100–105 also include *Oddz godz* synchronous, *Seven club Jim's two count*, *Seven club why not? style*, *Seven club not why style*, Jim's two-count variations, and *Programming in why rei?*. The synchronous diagram has simultaneous throws; the “why” patterns contain backwards/small transition arrows and pattern steals. Their arrow semantics need a dedicated representation review. They remain open or model-limit entries in the inventory.

Printed p. 91 contains the section heading “Popcorn patterns”; it is not an independent pattern. Printed pp. 91, 93, 99, 102, and 104 include text that continues a diagram or sequence from the preceding page rather than a new standalone pattern.

## Coverage boundary

This remains a partial translation of the candidate families in this range. Eight seven-count graphs were already in the runtime catalog; the seven additional ordinary popcorn graphs above are now imported as well. The catalog-wide validity test checks every graph under its intended preset ID, and the focused standalone test checks source chunk consistency plus offset preservation. Other ordinary popcorn patterns remain open until their event cycles and target endpoints are transcribed; simultaneous and manipulation families need a model decision first.

## Additional seven-club style patterns transcribed

Four more independent, non-manipulation patterns are now recorded in the same source JSON:

| Stable ID | Pattern | 2017 source | 2014 source | Events / edges |
| --- | --- | --- | --- | ---: |
| `highgate-sevenclubwhynotstylepattern` | Seven club why not? style pattern | printed p. 101 / PDF p. 102 | PDF p. 65 | 28 / 28 |
| `highgate-sevenclubnotwhystylepattern` | Seven club not why style pattern | printed p. 101 / PDF p. 102 | PDF p. 65 | 28 / 28 |
| `highgate-sevenclubmaybestylepattern1` | Seven club maybe style pattern #1 | printed p. 103 / PDF p. 104 | PDF p. 66 | 28 / 28 |
| `highgate-sevenclubmaybestylepattern2` | Seven club maybe style pattern #2 | printed p. 103 / PDF p. 104 | PDF p. 66 | 28 / 28 |

Each source row is seven or five actions per person. I repeat the odd-length rows to restore the starting hand phase. The action names, stated start hands, and half-beat / one-and-a-half-beat starts determine each event's hand and phase. For the four style patterns, the source prints Prechac heights; I checked every target hand, event time, and cycle wrap against the diagrams in both editions. Every node has one incoming and one outgoing edge, with no unresolved target.

These four source records remain separate from the runtime preset JSON, and all four have been imported into the causal editor's select list. The catalog validity test now checks each imported state directly. I excluded `highgate-whyrei`: the 2014 inventory classifies its sequence as pattern-transition manipulation, so it does not meet this batch’s non-manipulation criterion. The zap/heff-zap family is also excluded. Synchronous patterns with same-time hand events and the remaining non-zap seven-club patterns still need source transcription and verification.


For the four new records, each explicit source height equals the elapsed time from its throw node to the landing/action node, including backward zip arrows (height 1). This agrees with the labeled Prechac sequences and visible diagram edges in both editions. All four are complete periodic cycles: every node has exactly one incoming and one outgoing edge, and each edge’s cycle offset yields the printed throw height.
