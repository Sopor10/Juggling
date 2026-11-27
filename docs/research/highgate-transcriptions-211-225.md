# Highgate causal transcription batch: printed pp. 211–225

The records are in [highgate-transcriptions-211-225.json](highgate-transcriptions-211-225.json). This batch remains incomplete. Seven records from printed pp. 211–215 and two records from printed p. 216 are visually checked against the 2017 diagrams and 2014 edition.

## Source-checked records

| Inventory stable ID | Pattern | 2017 printed page | 2014 printed page | Check |
|---|---|---:|---:|---|
| `highgate-tenclubfourcounttriangle` | Ten club four count triangle | 211 | 148 | diagram and row sequences visually checked |
| `highgate-tenclubthreecounttriangle` | Ten club three count triangle | 211 | 148 | diagram and row sequences visually checked |
| `highgate-tenclubtwocounttriangle` | Ten club two count triangle | 211–212 | 148 | diagram and row sequences visually checked |
| `highgate-tenclubonecounttriangle` | Ten club one count triangle | 213 | 149 | diagram and row sequences visually checked |
| `highgate-tenclubonecounttrianglewithsingleanddoublepasses1` | Ten club one count triangle with single and double passes #1 | 215 | 150 | 18 nodes, 18 edges; visual comparison in both editions |
| `highgate-tenclubonecounttrianglewithsingleanddoublepasses2` | Ten club one count triangle with single and double passes #2 | 215 | 150 | 18 nodes, 18 edges; visual comparison in both editions |
| `highgate-tenclubpasspassselftriangle` | Ten club pass pass self triangle | 215 | 150 | 18 nodes, 18 edges; visual comparison in both editions |

| `highgate-tenclubpasspasshefftriangle` | Ten club pass pass heff triangle | 216 | 151 | 18 nodes, 18 edges; artwork, Prechac heights, endpoints checked in both editions |
| `highgate-tenclubfrenchthreecounttriangle` | Ten club French three count triangle | 216 | 151 | 18 nodes, 18 edges; artwork, Prechac heights, endpoints checked in both editions |

These two graphs were checked against explicit row sequences, Prechac heights, and diagram artwork in both editions.

## Transcription method and limits

For visually checked records, the source row sequence and arrows determine the receiving person and landing event. Self entries stay on the same row, hand labels alternate R/L, and cycle offsets record wraparound. On p. 216 the `3p3p4` and `3p34` Prechac strings determine heights 3 for passes/self throws and 4 for heffs. The graph edges connect to the events at those landing times; the standalone test checks each endpoint and offset directly against the printed action rows.

The editor's causal-edge representation does not separately encode straight/crossing trajectory or double-pass multiplicity. Keep those mechanics in notation and do not infer them from the graph.

## State and offset validation

A permanent standalone NUnit test constructs a `CausalDiagramEditorState` for every JSON record, checks unique record/throw/source IDs, edge counts and resolvable endpoints, calls `SynchronizeHeights()`, checks `IsValidForEditor()`, and compares serialized cycle offsets before and after synchronization. For both p. 216 graphs it also checks all 18 source-row actions against their exact landing endpoints and Prechac-derived heights. Result: **9 records checked, 0 errors**.

## Remaining work

- Printed p. 213 synchronous one-count triangle and synchronous one-count triangle with heffs: paired simultaneous throws and same-time events need supported semantics.
- Printed p. 216 why-not? triangle: zip and fractional phases require careful diagram tracing.
- Printed pp. 217–218 isosceles, isosceles variation, Maybe, and asynchronous pass-pass-self graphs include double-pass multiplicity, which the current editor cannot represent as one causal edge per action.
- Printed pp. 218–222 still have independent single-throw candidates to transcribe: the two one-count variations; two-count/two-count/four-count and Tarim's triangles; pass-pass-self/pass-pass-self/one-count; one-count/one-count/three-count; one-count/one-count/pass-pass-self; and one-count/one-count countdown triangles.
- Printed pp. 224–225 four-person two-count, pass-pass-self, and one-count feeds remain to transcribe from their four lanes.
- Printed p. 223 clockwise one-count runaround changes feeder roles and crossing/pass direction after each walk; represent it as phased transitions rather than flattening it into one static graph.
