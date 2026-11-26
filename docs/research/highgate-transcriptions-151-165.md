# Highgate triangle review: printed pp. 151–165

The separate [151–165 JSON chunk](highgate-transcriptions-151-165.json) contains five previously transcribed triangle patterns and one newly transcribed three-person drop-back line. The selected body diagrams were checked against the 2017 expanded PDF and the matching 2014 pages where available.

## Complete graphs in this chunk

| Stable ID | Pattern | Source diagram | Graph |
|---|---|---|---|
| `highgate-fourcounttriangleonins` | Four count triangle on ins | 2017 printed p. 154 / PDF p. 155; 2014 printed p. 103 | 12 events and 12 edges on a four-beat cycle |
| `highgate-threecounttriangleonins` | Three count triangle on ins | 2017 printed p. 154 / PDF p. 155; 2014 printed p. 103 | 18 events and 18 edges on a six-beat cycle |
| `highgate-twocounttriangleoninsandouts` | Two count triangle on ins and outs | 2017 printed p. 155 / PDF p. 156; 2014 printed p. 104 | 12 events and 12 edges on a four-beat cycle |
| `highgate-onecounttriangleoninsandouts` | One count triangle on ins and outs | 2017 printed p. 156 / PDF p. 157; 2014 printed p. 105 | 6 events and 6 edges on a two-beat cycle |
| `highgate-passpassselftriangle` | Pass pass self triangle | 2017 printed p. 157 / PDF p. 158; 2014 printed p. 106 | 18 events and 18 edges on a six-beat cycle |
| `highgate-threepersonfourcountdropbackline` | Three person four count drop-back line | 2017 printed p. 154 / PDF p. 155; 2014 printed p. 103 | Reuses the four-count triangle graph, as the source explicitly directs |

The drop-back line's causal graph represents who throws to whom. The source says its causal diagram is the same as the four-count triangle. The line formation and the overhead drop-back flight path are spatial details, so they are retained in the record's notes rather than drawn as throw-causality edges.

## Validation

`Highgate151To165StandaloneRecordValidationTests` constructs all six records using a registered preset ID, synchronizes heights, checks that no cycle offset changes, and requires `IsValidForEditor()` to pass. The normal catalog-wide validity test will also cover the new drop-back line after it is imported under its own ID.

## Remaining inventory candidates

Other entries in this range still need individual review and translation, including Rick's picnic feasts, Jim's style triangles, Martin's triangles, Maybe/Why Not/Parsnip triangles, and the eight-club 972 and Extended Tarim triangles. The presence of their titles in the inventory is not evidence that their graph endpoints have been transcribed. Each remains open until its source diagram and sequence are mapped and validated.
