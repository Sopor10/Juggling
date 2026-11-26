# Highgate causal transcription batch: printed pp. 211–225

The records are in [highgate-transcriptions-211-225.json](highgate-transcriptions-211-225.json). This batch is incomplete. Seven records from printed pp. 211–215 are visually checked against the 2017 diagrams and 2014 edition. Two further p. 216 records have matching explicit row-sequence prose in the 2017 and 2014 editions and pass editor state validation; their causal edges still need an edge-by-edge visual audit against both diagrams.

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

## Candidate records: source prose checked, diagrams not yet fully verified

| Inventory stable ID | Pattern | 2017 printed page | State check | Remaining review |
|---|---|---:|---|---|
| `highgate-tenclubpasspasshefftriangle` | Ten club pass pass heff triangle | 216 | valid; offsets preserved | visually compare every edge against both editions |
| `highgate-tenclubfrenchthreecounttriangle` | Ten club French three count triangle | 216 | valid; offsets preserved | visually compare every edge against both editions |

These two graphs are checked against explicit row-sequence prose in both editions, but have not yet been confirmed edge by edge against diagram artwork. Treat them as text-verified candidates pending visual graph audit.

## Transcription method and limits

For visually checked records, the source row sequence determines which person receives each event. Self entries stay on the same row, hand labels alternate R/L, and cycle offsets record wraparound. The p. 216 candidates use their explicit Prechac heights to determine landing events; their artwork comparison remains open.

The editor's causal-edge representation does not separately encode straight/crossing trajectory or double-pass multiplicity. Keep those mechanics in notation and do not infer them from the graph.

## State and offset validation

A focused NUnit test constructed a `CausalDiagramEditorState` for every JSON record, called `SynchronizeHeights()`, checked `IsValidForEditor()`, and compared serialized cycle offsets before and after synchronization. Result: **9 records checked, 0 errors**. This verifies editor-state validity, not semantic correctness of unverified source edges.

## Remaining work

- Printed p. 213 synchronous one-count triangle and synchronous one-count triangle with heffs: paired simultaneous throws and same-time events need supported semantics.
- Printed p. 216 why-not? triangle: zip and fractional phases require careful diagram tracing.
- Printed pp. 217–225: maybe, isosceles, isosceles variation, asynchronous pass-pass-self, feeding variations, mixed triangles, Tarim's triangle and remaining listed patterns need source comparison and transcription.
- Confirm the two p. 216 candidate graphs visually in both editions before import.
