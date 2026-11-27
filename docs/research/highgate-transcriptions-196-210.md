# Highgate transcription review: printed pp. 196–210

This page-range chunk is stored separately in [highgate-transcriptions-196-210.json](highgate-transcriptions-196-210.json). It contains the existing ten-club one-count feeding countdown record, a newly source-checked Ten club gorilla, and three asynchronous three-count feeds. The Ten club gorilla is now imported into the runtime catalog and marked translated in the inventory.

## Added ordinary causal diagrams

| Stable ID | Pattern | 2017 source | 2014 comparison | Graph |
|---|---|---|---|---|
| `highgate-tenclubgorilla` | Ten club gorilla | printed p. 197 / PDF p. 198 | p. 138 | Two-beat cycle; six events and six causal edges. Ben alternates passes to Clare and Anne; each receiver alternates a pass to Ben and a self throw. |
| `highgate-passpassselffeedingfrenchthreecount` | Pass pass self feeding French three count | printed p. 199 / PDF p. 200 | p. 140 | Six-beat cycle; 18 events and 18 causal edges. Anne and Ben start in phase; Clare is half a beat later. |
| `highgate-passpasshefffeedingfrenchthreecount` | Pass pass heff feeding French three count | printed p. 199 / PDF p. 200 | p. 140 | Six-beat cycle; 18 events and 18 causal edges. All lanes start in phase. |
| `highgate-asynchronouspasspasshefffeedingthreecount` | Asynchronous pass pass heff feeding three count | printed p. 200 / PDF p. 201 | p. 141 | Six-beat cycle; 18 events and 18 causal edges. Ben starts in phase; Anne and Clare are half a beat later. |

For each record, the per-juggler sequences and printed arrow endpoints were checked against both editions. Each three-event sequence is repeated twice to preserve alternating right/left hands over the six-beat cycle. Self throws point to the next event; heffs point two beats forward to the same hand. Passes land on the receiving juggler's next available event: after one beat for synchronized lanes and 1.5 beats when the receiver is offset by half a beat. The JSON retains explicit throw heights separately from causal time and stores any wraparound in `cycleOffset`.

The source text also identifies synchronous feeds and simultaneous double passes in this page range. Those are excluded because this editor state represents a single event per hand timeline and the required simultaneous-event semantics are not established here. Zip patterns are excluded from this ordinary-pattern chunk and handled by the signed-zip work. Other period-five and two-hand patterns require separate model review before translation; no incomplete endpoint has been guessed.

The Ten club gorilla diagram has explicit alternating routes with no simultaneous throw: Ben passes to Clare then Anne, while each receiver alternates a pass to Ben and a self throw. The source starts Clare half a beat after Ben and Anne one and a half beats after Ben. In the two-beat cycle, Ben’s first pass to Clare and Anne’s pass to Ben cross a 1.5-beat event gap (height 3.5); Ben’s pass to Anne and Clare’s pass to Ben cross a 0.5-beat gap (height 2.5). The two self throws cross a one-beat gap (height 3). The 2014 diagram and sequence on p. 138 match the 2017 printed p. 197 graph.

## Validation

The focused standalone test reads all five records from the chunk, substitutes the registered `highgate-three-count` preset ID, calls `SynchronizeHeights()`, verifies that all serialized cycle offsets remain stable, and requires `IsValidForEditor()` for each record. For every record it checks unique throw and edge IDs, one outgoing edge per throw, resolvable endpoints, and the explicit height/time equation when the source data provides explicit heights. For Ten club gorilla it also checks every destination and cycle offset against the source sequence.

```text
dotnet test Siteswaps.Test/Siteswaps.Test.csproj --no-restore --filter EveryStandaloneRecordCreatesAValidEditorStateAndPreservesOffsets
Passed: 5, Failed: 0
```

The existing ten-club countdown is retained in this file and included in the same standalone validation. These records remain separate from the catalog until explicitly imported.
