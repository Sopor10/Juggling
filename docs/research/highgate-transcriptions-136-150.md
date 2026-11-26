# Highgate transcription review: printed pp. 136–150

This source block extends the existing [`136–150` JSON chunk](highgate-transcriptions-136-150.json), which already contains the three-person countdown feast. Five additional patterns on 2017 printed pp. 135, 137, and 140 have complete causal diagrams or explicitly composed source lanes. Their source diagrams and sequences were checked against 2014 pp. 90–93. All five are now imported into the causal editor's Highgate list and covered by the catalog-wide validity test.

## Added graphs

| Stable ID | Pattern | Source | Representation |
|---|---|---|---|
| `highgate-threepersonpasspassselffeastjedsnightmare` | Three-person pass pass self feast (JED's nightmare) | 2017 printed p. 135 / PDF p. 136; 2014 PDF p. 90 | Three 9-event destination sequences doubled to an 18-beat cycle; 54 events and 54 directed edges. |
| `highgate-threepersonpassselfpassfeastantiquasar` | Three-person pass self pass feast (anti quasar) | Same | Each listed destination becomes the next throw event of that receiver; 54 events and 54 edges. |
| `highgate-threepersonpasspassselffeastdoublethreecount` | Three-person pass pass self feast (double three count) | Same | All three per-person sequences and the diagram's pass destinations agree; 54 events and 54 edges. |
| `highgate-trifle` | Trifle | 2017 printed p. 137 / PDF p. 138; 2014 PDF p. 91 | The source assigns Anne's lane from feast #2, Ben's from #1, and Clare's from #3; these named diagrams provide all 36 events and edges. |
| `highgate-trilogy` | Trilogy | 2017 printed p. 140 / PDF p. 141; 2014 PDF p. 93 | The source assigns Anne's lane from bookends feast #1, Ben's from #2, and Clare's from #3; 90 events and edges preserve the 30-beat cycle. |

The source gives every person's ordered destination sequence. A pass or self throw lands at the next event of the named receiver; all throws in these Prechac sequences have height 3. Each nine-event sequence is repeated with opposite-hand labels, so the causal cycle has length 18 and right/left hand parity is preserved. An edge wrapping from event 17 to event 0 records cycle offset 1. This reproduces the drawn graph endpoints and the starting hand labels.

The patterns use the explicit texts and notation shown in both editions. The two composite patterns have no separate arrow diagram; their lanes are combined exactly as the source prescribes from the immediately preceding named diagrams. Color-coded club identity is explanatory metadata and is not modeled as a causal edge.

## Validation

`Highgate136To150StandaloneRecordValidationTests` loads every graph in the separate chunk, assigns a registered preset ID for the editor's known-ID guard, synchronizes heights, and verifies that every graph passes `IsValidForEditor()` without changing its serialized edge offsets. This checks graph validity independently of central catalog registration.

## Still open in this page block

The named **Trinity** pattern is not included yet. Its description says its lanes combine JED's nightmare, anti quasar, and double three count, but its printed Ben sequence has eight destinations while the other lanes have nine. The 2014 edition reproduces the same discrepancy and supplies a diagram; a complete lane period and all wrap offsets need to be reconciled with that diagram before a record is safe to create. The other open candidates in pp. 136–150 include three-person feeds, Pulsar/Quasar, Jim's three-count feasts, and additional triangles. They need their own page-by-page transcription and state validation.
