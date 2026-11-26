# Highgate feed transcription review: printed pp. 121–135

The separate [121–135 data file](highgate-transcriptions-121-135.json) already contained Jim's inverted bookends feed. I added two complete three-person causal graphs after checking their body diagrams and per-juggler sequences in both editions.

## Added graphs

| Stable ID | Pattern | Body source pages | Graph |
|---|---|---|---|
| `highgate-passpasszipfeedingthreecount` | Pass pass zip feeding three count | 2017 printed p. 120 / PDF p. 121; 2014 PDF p. 80 | Six-beat cycle; 18 nodes and 18 causal edges. Ben's alternating pass/pass/zip lane and Anne/Clare's three-count lanes are repeated with hand parity preserved. |
| `highgate-passpasszipfeedingfrenchthreecount` | Pass pass zip feeding French three count | 2017 printed p. 120 / PDF p. 121; 2014 PDF p. 80 | Six-beat cycle; 18 nodes and 18 causal edges. Ben's pass/pass/zip lane, Anne's heff/pass/self lane, and Clare's self/heff/pass lane are transcribed. |

The two 2017 body diagrams appear on printed p. 120 (physical PDF p. 121), although the index candidates in the inventory point to printed p. 123 / PDF p. 124. The matching 2014 diagrams and sequences appear on PDF p. 80. Each of the three extracted diagrams on the two body pages has the same SHA-256 hash across editions. The JSON records cite the body pages and retain the index/body mismatch in their source notes. The inventory now marks both patterns reviewed and translated.

Each graph has one outgoing causal edge per event. The zip arrows point back to the preceding Ben event and carry explicit height 1. In the first pattern, Ben's passes use height 3.5 and Anne/Clare's passes use 3.5; self throws are 3. In the French-three-count pattern, all jugglers start in phase, so passes use height 3; heffs use height 4, self throws use 3, and zips use 1. Explicit source heights keep diagram coordinates separate from causal timing.

## Validation

The focused NUnit test constructs both standalone records using an already registered preset ID, runs `SynchronizeHeights`, checks that every cycle offset is preserved, and requires `IsValidForEditor()` to pass. The central catalog-wide test also validates both new presets under their actual IDs:

```text
dotnet test Siteswaps.Test/Siteswaps.Test.csproj --no-restore --filter PassPassZipFeedRecordsCreateValidEditorStatesAndPreserveOffsets
Passed: 1, Failed: 0
```

The 2017 and 2014 body-page image extractions match byte-for-byte. JSON structural checks confirm 18 unique nodes, 18 unique outgoing edges, and resolvable endpoints per record.

## Remaining scope

The inventory lists many other feed and multi-juggler candidates around pp. 121–135. These two diagrams are a bounded source-verified addition, not a claim that the surrounding feeding patterns have been transcribed. The rest need page-by-page diagram or sequence review before their causal edges can be imported.
