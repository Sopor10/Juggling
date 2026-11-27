# Highgate causal transcription batch: printed pp. 211–225

The records are in [highgate-transcriptions-211-225.json](highgate-transcriptions-211-225.json). This batch remains incomplete. Seven records from printed pp. 211–215, two from printed p. 216, two one-count variations from pp. 218–219, and Tarim's triangle from pp. 219–220 are visually checked against the 2017 diagrams and 2014 edition.

## Source-checked records

| Inventory stable ID | Pattern | 2017 printed page | 2014 printed page | Check |
|---|---|---:|---:|---|
| `highgate-tenclubfourcounttriangle` | Ten club four count triangle | 211 | 148 | diagram and row sequences visually checked; B-L4 self endpoint uses cycle offset 0, yielding the printed 9 height |
| `highgate-tenclubthreecounttriangle` | Ten club three count triangle | 211 | 148 | diagram and row sequences visually checked |
| `highgate-tenclubtwocounttriangle` | Ten club two count triangle | 211–212 | 148 | diagram and row sequences visually checked |
| `highgate-tenclubonecounttriangle` | Ten club one count triangle | 213 | 149 | diagram and row sequences visually checked |
| `highgate-tenclubonecounttrianglewithsingleanddoublepasses1` | Ten club one count triangle with single and double passes #1 | 215 | 150 | 18 nodes, 18 edges; visual comparison in both editions |
| `highgate-tenclubonecounttrianglewithsingleanddoublepasses2` | Ten club one count triangle with single and double passes #2 | 215 | 150 | 18 nodes, 18 edges; visual comparison in both editions |
| `highgate-tenclubpasspassselftriangle` | Ten club pass pass self triangle | 215 | 150 | 18 nodes, 18 edges; visual comparison in both editions |

| `highgate-tenclubpasspasshefftriangle` | Ten club pass pass heff triangle | 216 | 151 | 18 nodes, 18 edges; artwork, Prechac heights, endpoints checked in both editions |
| `highgate-tenclubfrenchthreecounttriangle` | Ten club French three count triangle | 216 | 151 | 18 nodes, 18 edges; artwork, Prechac heights, endpoints checked in both editions |
| `highgate-tenclubonecountvariation1` | Ten club one count variation #1 | 218 | 153 | 6 nodes / 6 edges; arrows and two-action source rows checked in both editions |
| `highgate-tenclubonecountvariation2` | Ten club one count variation #2 | 219 | 154 | 12 nodes / 12 edges; arrows and four-action source rows checked in both editions |
| `highgate-tenclubtarimstriangle` | Ten club Tarim's triangle | 219–220 | 154 | 18 nodes / 18 edges; lane offsets and all arrow endpoints checked in both editions |

These two graphs were checked against explicit row sequences, Prechac heights, and diagram artwork in both editions.

The four-count triangle stays WIP: correcting B-L4's self-wrap from one full cycle to the next event in the same cycle restores its 9-height, but the current prop mapper still cannot close the full graph into a periodic prop assignment. The source supports the edge height correction; the overall mapping needs further review.

## Transcription method and limits

For visually checked records, the source row sequence and arrows determine the receiving person and landing event. Self entries stay on the same row, hand labels alternate R/L, and cycle offsets record wraparound. On p. 216 the `3p3p4` and `3p34` Prechac strings determine heights 3 for passes/self throws and 4 for heffs. Tarim's triangle diagram is on 2017 printed p. 219 and its action text continues on p. 220; the matching 2014 diagram is on p. 154. Its phase starts are Ben 0, Anne 1/3, Clare 2/3; the arrows distinguish the lane event index as well as receiver. The test checks each of its 18 endpoints, lane-index shifts, and fractional phase deltas against the printed rows and arrows.

The editor's causal-edge representation does not separately encode straight/crossing trajectory or double-pass multiplicity. Keep those mechanics in notation and do not infer them from the graph.

## State and offset validation

A permanent standalone NUnit test constructs a `CausalDiagramEditorState` for every JSON record, checks unique record/throw/source IDs, edge counts and resolvable endpoints, calls `SynchronizeHeights()`, checks `IsValidForEditor()`, and compares serialized cycle offsets before and after synchronization. For both p. 216 graphs it checks all 18 source-row actions against exact landing endpoints and Prechac-derived heights. For the two one-count variations it checks every printed recipient against its arrow endpoint and phase-derived landing time. For Tarim's triangle it checks all 18 receiver/index endpoints, phase deltas, and cycle wraps. Result: **12 records checked**.

## Remaining work

- Printed p. 213 synchronous one-count triangle and synchronous one-count triangle with heffs: paired simultaneous throws and same-time events need supported semantics.
- Printed p. 216 why-not? triangle: zip and fractional phases require careful diagram tracing.
- Printed pp. 217–218 isosceles, isosceles variation, Maybe, and asynchronous pass-pass-self graphs include double-pass multiplicity, which the current editor cannot represent as one causal edge per action.
- Printed pp. 219–222 still have independent single-throw candidates to transcribe: two-count/two-count/four-count; pass-pass-self/pass-pass-self/one-count; one-count/one-count/three-count; one-count/one-count/pass-pass-self; and one-count/one-count countdown triangles. Their printed actions are available, but exact arrow landing offsets need independent audit; none is inferred from Tarim's triangle or the one-count variations. Countdown also has changing pass heights across the sequence, so the action label alone does not establish landing events.
- Printed pp. 224–225 four-person feeds are transcribed and validated in the adjacent [226–240 batch](highgate-transcriptions-226-240.md).
- Printed p. 223 clockwise one-count runaround changes feeder roles and crossing/pass direction after each walk; represent it as phased transitions rather than flattening it into one static graph.
