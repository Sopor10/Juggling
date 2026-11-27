# Highgate triangle review: printed pp. 151–165

The separate [151–165 JSON chunk](highgate-transcriptions-151-165.json) contains six previously transcribed patterns and seven additional independent triangles. The seven new graphs have been imported into the runtime causal-editor catalog and marked as translated in the inventory. The selected body diagrams were checked against the 2017 expanded PDF and the matching 2014 pages where available.

## Complete graphs in this chunk

| Stable ID | Pattern | Source diagram | Graph |
|---|---|---|---|
| `highgate-fourcounttriangleonins` | Four count triangle on ins | [2017 printed p. 154 / PDF p. 155](https://jugglingedge.com/pdf/highgate.pdf#page=155); [2014 printed p. 103 / PDF p. 103](https://passing.zone/wp-content/uploads/highgate.pdf#page=103) | 12 events and 12 edges on a four-beat cycle |
| `highgate-threecounttriangleonins` | Three count triangle on ins | [2017 printed p. 154 / PDF p. 155](https://jugglingedge.com/pdf/highgate.pdf#page=155); [2014 printed p. 103 / PDF p. 103](https://passing.zone/wp-content/uploads/highgate.pdf#page=103) | 18 events and 18 edges on a six-beat cycle |
| `highgate-twocounttriangleoninsandouts` | Two count triangle on ins and outs | [2017 printed p. 155 / PDF p. 156](https://jugglingedge.com/pdf/highgate.pdf#page=156); [2014 printed p. 104 / PDF p. 104](https://passing.zone/wp-content/uploads/highgate.pdf#page=104) | 12 events and 12 edges on a four-beat cycle |
| `highgate-onecounttriangleoninsandouts` | One count triangle on ins and outs | [2017 printed p. 156 / PDF p. 157](https://jugglingedge.com/pdf/highgate.pdf#page=157); [2014 printed p. 105 / PDF p. 105](https://passing.zone/wp-content/uploads/highgate.pdf#page=105) | 6 events and 6 edges on a two-beat cycle |
| `highgate-passpassselftriangle` | Pass pass self triangle | [2017 printed p. 157 / PDF p. 158](https://jugglingedge.com/pdf/highgate.pdf#page=158); [2014 printed p. 106 / PDF p. 106](https://passing.zone/wp-content/uploads/highgate.pdf#page=106) | 18 events and 18 edges on a six-beat cycle |
| `highgate-threepersonfourcountdropbackline` | Three person four count drop-back line | [2017 printed p. 154 / PDF p. 155](https://jugglingedge.com/pdf/highgate.pdf#page=155); [2014 printed p. 103 / PDF p. 103](https://passing.zone/wp-content/uploads/highgate.pdf#page=103) | Reuses the four-count triangle graph, as the source explicitly directs |

## Additional independent graphs

I visually checked the source diagrams and lane text on 2017 printed pp. 161, 164, and 165 against the corresponding 2014 pages 110 and 112. Each table row links to the body page in both PDFs. Destination lanes are transcribed in the separate JSON chunk, and the batch test asserts every edge endpoint against those lanes.

| Stable ID | Pattern | Exact source pages | Graph |
|---|---|---|---|
| `highgate-twocounttwocountfourcounttriangle` | Two count, two count, four count triangle | [2017 printed p. 161 / PDF p. 162](https://jugglingedge.com/pdf/highgate.pdf#page=162); [2014 printed p. 110 / PDF p. 110](https://passing.zone/wp-content/uploads/highgate.pdf#page=110) | Three four-event lanes; 12 throws and edges. |
| `highgate-tarimstriangle` | Tarim’s triangle | [2017 printed p. 161 / PDF p. 162](https://jugglingedge.com/pdf/highgate.pdf#page=162); [2014 printed p. 110 / PDF p. 110](https://passing.zone/wp-content/uploads/highgate.pdf#page=110) | Three three-event lanes doubled to a six-beat cycle; 18 throws and edges. This is independent of the existing pass pass self triangle because Anne, Ben, and Clare have different receiving lanes. |
| `highgate-extendedtarimstriangle` | Extended Tarim’s triangle | [2017 printed p. 161 / PDF p. 162](https://jugglingedge.com/pdf/highgate.pdf#page=162); [2014 printed p. 110 / PDF p. 110](https://passing.zone/wp-content/uploads/highgate.pdf#page=110) | Three 18-event lanes; 54 throws and edges. The stated six-beat rotation is carried by the changing destination rows. |
| `highgate-onecountonecountthreecounttriangle` | One count, one count, three count triangle | [2017 printed p. 164 / PDF p. 165](https://jugglingedge.com/pdf/highgate.pdf#page=165); [2014 printed p. 112 / PDF p. 112](https://passing.zone/wp-content/uploads/highgate.pdf#page=112) | Three three-event lanes doubled to six beats; 18 throws and edges. |
| `highgate-onecountonecountpasspassselftriangle` | One count, one count, pass pass self triangle | [2017 printed p. 164 / PDF p. 165](https://jugglingedge.com/pdf/highgate.pdf#page=165); [2014 printed p. 112 / PDF p. 112](https://passing.zone/wp-content/uploads/highgate.pdf#page=112) | Three three-event lanes doubled to six beats; 18 throws and edges. |
| `highgate-passpassselfpasspassselfcountdowntriangle` | Pass pass self, pass pass self, countdown triangle | [2017 printed p. 164 / PDF p. 165](https://jugglingedge.com/pdf/highgate.pdf#page=165); [2014 printed p. 112 / PDF p. 112](https://passing.zone/wp-content/uploads/highgate.pdf#page=112) | Three 12-event lanes; 36 throws and edges. |
| `highgate-onecountonecountcountdowntriangle` | One count, one count, countdown triangle | [2017 printed p. 165 / PDF p. 166](https://jugglingedge.com/pdf/highgate.pdf#page=166); [2014 printed p. 112 / PDF p. 112](https://passing.zone/wp-content/uploads/highgate.pdf#page=112) | Three eight-event lanes; 24 throws and edges. |

For these graphs, each source row lists the receiving juggler for the throw at that event. An edge goes to the receiver’s next listed event; self throws go to the same juggler’s next event. When that event crosses the cycle boundary, the edge carries offset 1. The editor’s two-beat hand interval then derives height 3. This preserves the source’s causal-diagram connections and hand parity. The graph records causal throw flow; it omits club-color schemes and any floor movement.

The drop-back line's causal graph represents who throws to whom. The source says its causal diagram is the same as the four-count triangle. The line formation and the overhead drop-back flight path are spatial details, so they are retained in the record's notes rather than drawn as throw-causality edges.

## Validation

`Highgate151To165StandaloneRecordValidationTests` constructs all thirteen records using a registered preset ID, synchronizes heights, checks that no cycle offset changes, and requires `IsValidForEditor()` to pass. For every graph, it checks that all edge endpoints resolve and edge count equals throw count. For the seven additions, it checks each source destination lane against its expected receiver event and cycle offset, and requires the synchronized throw heights to be 3. The filtered test passes. The catalog-wide validity test and reviewed-ID allowlist also cover the seven imported graphs.

## Remaining inventory candidates

Patterns not added in this pass include Rick’s picnic feasts and Jim’s style triangles, whose source action rows contain `hurried` throws; Martin’s triangles and Why Not/Maybe patterns, which contain `zip`/`heff` actions and fractional starting phases; and Parsnip/972, which use double throws or zips not represented by this batch’s one-event-per-beat graph model. Other in/out variants of existing triangles have the same causal topology and are aliases rather than independent records. These details remain documented here rather than guessed into a graph.
