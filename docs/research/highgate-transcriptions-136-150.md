# Highgate transcription review: printed pp. 136–150

This source block extends the existing [`136–150` JSON chunk](highgate-transcriptions-136-150.json). It now contains twelve graph records: the countdown feast, three feasts and two composite feasts transcribed earlier, and five Pulsar-family patterns added in this review. The five newly transcribed Pulsar patterns and the newly transcribed Quasar feast are imported into the runtime catalog and marked as translated in the inventory; the six earlier records in this chunk were already imported.

## Added graphs

| Stable ID | Pattern | Source | Representation |
|---|---|---|---|
| `highgate-threepersoncountdownfeast` | Three-person countdown feast | [2017 printed p. 141 / PDF p. 142](https://jugglingedge.com/pdf/highgate.pdf#page=142); [2014 printed p. 94 / PDF p. 94](https://passing.zone/wp-content/uploads/highgate.pdf#page=94) | Three 24-event lanes; 72 throws and edges. |
| `highgate-threepersonpasspassselffeastjedsnightmare` | Three-person pass pass self feast (JED's nightmare) | [2017 printed p. 135 / PDF p. 136](https://jugglingedge.com/pdf/highgate.pdf#page=136); [2014 printed p. 90 / PDF p. 90](https://passing.zone/wp-content/uploads/highgate.pdf#page=90) | Three 9-event destination sequences doubled to an 18-beat cycle; 54 events and 54 directed edges. |
| `highgate-threepersonpassselfpassfeastantiquasar` | Three-person pass self pass feast (anti quasar) | Same pages as JED's nightmare above | Each listed destination becomes the next throw event of that receiver; 54 events and 54 edges. |
| `highgate-threepersonpasspassselffeastdoublethreecount` | Three-person pass pass self feast (double three count) | Same pages as JED's nightmare above | All three per-person sequences and the diagram's pass destinations agree; 54 events and 54 edges. |
| `highgate-trifle` | Trifle | [2017 printed p. 137 / PDF p. 138](https://jugglingedge.com/pdf/highgate.pdf#page=138); [2014 printed p. 91 / PDF p. 91](https://passing.zone/wp-content/uploads/highgate.pdf#page=91) | The source assigns Anne's lane from feast #2, Ben's from #1, and Clare's from #3; these named diagrams provide all 36 events and edges. |
| `highgate-trilogy` | Trilogy | [2017 printed p. 140 / PDF p. 141](https://jugglingedge.com/pdf/highgate.pdf#page=141); [2014 printed p. 93 / PDF p. 93](https://passing.zone/wp-content/uploads/highgate.pdf#page=93) | The source assigns Anne's lane from bookends feast #1, Ben's from #2, and Clare's from #3; 90 events and edges preserve the 30-beat cycle. |

## Quasar feast added in this review

The separate `highgate-quasar` record is the standalone selfless countdown feast, not the already catalogued `Quasar runaround`, whose sequence changes after walks. The inventory's 2017 index reference is printed p. 144, but the matching full pattern appears on printed p. 142 / PDF p. 143. The source diagram and the three 15-event action rows agree; the pattern explicitly repeats with the other hand, so the graph duplicates those rows into a 30-beat cycle with alternating hand labels. The 2014 PDF p. 94 contains the same diagram, notation, and action rows even though this title is absent from its index. Each event targets the next event on its named recipient's lane (or its own lane for `self`), and the final event wraps with cycle offset 1. The record has 45 events and 45 causal edges, all deriving height 3 after synchronization. Color-coding is not represented because it does not change the causal endpoints.

| Stable ID | Pattern | Exact source pages | Graph |
|---|---|---|---|
| `highgate-quasar` | Quasar | [2017 printed p. 142 / PDF p. 143](https://jugglingedge.com/pdf/highgate.pdf#page=143); [2014 printed/PDF p. 94](https://passing.zone/wp-content/uploads/highgate.pdf#page=94) | Three 15-event lanes repeated with opposite hand parity; 30-beat cycle, 45 throws and edges. |

## Pulsar family added in this review

The destination rows and Prechac strings were transcribed from the source diagrams and checked visually against the printed rows. The 2014 edition reproduces these same strings and destination sequences verbatim. In these five patterns, each juggler has one event at each integer beat and a listed destination names the receiving juggler (or the same juggler for `self`). Each throw points to the recipient's next listed throw event; the editor's timing relation adds the two-beat hand interval and derives the source's height 3. When that next event lies beyond the cycle boundary, `cycleOffset` records the wrap count. This follows the causal arrows in the source diagram and keeps endpoints consistent with the other records in this batch.

| Stable ID | Pattern | Exact source pages | Graph |
|---|---|---|---|
| `highgate-pulsar` | Pulsar | [2017 printed p. 143 / PDF p. 144](https://jugglingedge.com/pdf/highgate.pdf#page=144); [2014 printed p. 95 / PDF p. 95](https://passing.zone/wp-content/uploads/highgate.pdf#page=95) | Three 24-event destination lanes; 72 throws and 72 edges. |
| `highgate-ambidextrouspulsarthreecountfeed` | Ambidextrous pulsar (three count feed) | [2017 printed p. 143 / PDF p. 144](https://jugglingedge.com/pdf/highgate.pdf#page=144); [2014 printed p. 95 / PDF p. 95](https://passing.zone/wp-content/uploads/highgate.pdf#page=95) | Three 36-event lanes; 108 throws and edges. |
| `highgate-ambidextrouspulsarpasspassselffeed` | Ambidextrous pulsar (pass pass self feed) | [2017 printed p. 144 / PDF p. 145](https://jugglingedge.com/pdf/highgate.pdf#page=145); [2014 printed p. 96 / PDF p. 96](https://passing.zone/wp-content/uploads/highgate.pdf#page=96) | Three 18-event lanes; 54 throws and edges. |
| `highgate-ambidextrouspulsarpassselfpassfeed` | Ambidextrous pulsar (pass self pass feed) | [2017 printed p. 144 / PDF p. 145](https://jugglingedge.com/pdf/highgate.pdf#page=145); [2014 printed p. 96 / PDF p. 96](https://passing.zone/wp-content/uploads/highgate.pdf#page=96) | Three 18-event lanes; 54 throws and edges. |
| `highgate-ambidextrouspulsaronecountfeed` | Ambidextrous pulsar (one count feed) | [2017 printed p. 145 / PDF p. 146](https://jugglingedge.com/pdf/highgate.pdf#page=146); [2014 printed p. 96 / PDF p. 96](https://passing.zone/wp-content/uploads/highgate.pdf#page=96) | Three 12-event lanes; 36 throws and edges. |

The source gives every person's ordered destination sequence. A pass or self throw lands at the next event of the named receiver; all throws in these Prechac sequences have height 3. Each nine-event sequence is repeated with opposite-hand labels, so the causal cycle has length 18 and right/left hand parity is preserved. An edge wrapping from event 17 to event 0 records cycle offset 1. This reproduces the drawn graph endpoints and the starting hand labels.

The patterns use the explicit texts and notation shown in both editions. The two composite patterns have no separate arrow diagram; their lanes are combined exactly as the source prescribes from the immediately preceding named diagrams. Color-coded club identity is explanatory metadata and is not modeled as a causal edge.

## Validation

`Highgate136To150StandaloneRecordValidationTests` loads every graph in the separate chunk, assigns a registered preset ID for the editor's known-ID guard, synchronizes heights, and verifies that every graph passes `IsValidForEditor()` without changing its serialized edge offsets. It also checks one edge per throw and resolvable endpoints for every record. For the five Pulsar records, it checks that each endpoint is the next listed throw event for the recorded recipient, and that the edge and two-beat hand interval derive height 3. This checks graph validity independently of central catalog registration. The filtered test passes (12 records). It checks the Quasar source lanes over both hand phases and verifies all 45 endpoints and cycle wraps.

## Still open in this page block

The named **Trinity** pattern is not included yet. Its description says its lanes combine JED's nightmare, anti quasar, and double three count, but its printed Ben sequence has eight destinations while the other lanes have nine. The 2014 edition reproduces the same discrepancy and supplies a diagram; a complete lane period and all wrap offsets need to be reconciled with that diagram before a record is safe to create. The **three-person geometric countdown feast** remains open. Its source diagram is present on printed p. 141 / PDF p. 142, but the printed action rows currently transcribe to uneven lane lengths (27/26/27) and need a careful node-by-node diagram reconciliation before import. Quasar has now been transcribed and imported as described above. Jim's feasts are explicitly left out because their source rows use `hurried` throws, which are manipulation/timing variants outside this pass. The asynchronous pass pass self two-count feed also has a half-beat start phase and a faster feeder; it needs a fractional-time representation before it can be added without losing that source timing.
