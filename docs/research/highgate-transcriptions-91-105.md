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

## Reviewed patterns that still need translation or a model decision

Printed pp. 91–93 contain the six-count/five-count/four-count popcorn families, French three-count techno, and *The dark side of two count* with its practice and ambidextrous variants. The matching 2014 diagrams on pp. 58–60 confirm the listed sequences. The three “dark side” diagrams use simultaneous two-hand throws; the current editor has one timeline lane per juggler, so a faithful graph would overlap nodes. The six-count techno/practice and synchronous bookends likewise need a layout/model decision before they can be treated as ordinary presets. The page also continues *Scratch your head*, already retained as a model-limit candidate.

Printed pp. 98–103 contain seven-club zap and heff-zap patterns, including *The holy grail aka Willy Wonka's one count*, *Heff zap heff*, *Seven club bookends with zaps*, and siteswap variants `97586`, `78956`, and `79685`. The 2017 diagrams and text show explicit zap/manipulation actions. These remain separate manipulation candidates; they are not imported as ordinary passing graphs. Their corresponding 2014 diagrams and sequences on p. 64 match the three numbered siteswaps and Oddz godz; the 2014 source also confirms that the collection has explicit manipulation-adjacent zaps.

Printed pp. 100–105 also include *Oddz godz* synchronous, *Seven club Jim's two count*, *Seven club why not? style*, *Seven club not why style*, Jim's two-count variations, and *Programming in why rei?*. The synchronous diagram has simultaneous throws; the “why” patterns contain backwards/small transition arrows and pattern steals. Their arrow semantics need a dedicated representation review. They remain open or model-limit entries in the inventory.

Printed p. 91 contains the section heading “Popcorn patterns”; it is not an independent pattern. Printed pp. 91, 93, 99, 102, and 104 include text that continues a diagram or sequence from the preceding page rather than a new standalone pattern.

## Coverage boundary

This is a source and catalog audit, not a complete translation of pp. 91–105. Eight already-imported graphs were cross-checked and validated. The ordinary popcorn patterns still marked `open` in the inventory need transcription; the synchronous and manipulation families need a model decision first. No runtime catalog data was changed by this audit.
