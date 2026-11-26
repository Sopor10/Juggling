# Highgate causal transcription batch: printed pp. 211–225

The current graph records are in [highgate-transcriptions-211-225.json](highgate-transcriptions-211-225.json). This is a partial, checked batch for pp. 211–215, not a claim that the full pp. 211–225 span has been transcribed.

## Included patterns

| Inventory stable ID | Pattern | 2017 printed page | 2014 printed page | Check |
|---|---|---:|---:|---|
| `highgate-tenclubfourcounttriangle` | Ten club four count triangle | 211 | 148 | diagram and row sequences visually checked |
| `highgate-tenclubthreecounttriangle` | Ten club three count triangle | 211 | 148 | diagram and row sequences visually checked |
| `highgate-tenclubtwocounttriangle` | Ten club two count triangle | 211–212 | 148 | diagram and row sequences visually checked |
| `highgate-tenclubonecounttriangle` | Ten club one count triangle | 213 | 149 | diagram and row sequences visually checked |
| `highgate-tenclubonecounttrianglewithsingleanddoublepasses1` | Ten club one count triangle with single and double passes #1 | 215 | 150 | 18 nodes, 18 edges; visual comparison in both editions |
| `highgate-tenclubonecounttrianglewithsingleanddoublepasses2` | Ten club one count triangle with single and double passes #2 | 215 | 150 | 18 nodes, 18 edges; visual comparison in both editions |
| `highgate-tenclubpasspassselftriangle` | Ten club pass pass self triangle | 215 | 150 | 18 nodes, 18 edges; visual comparison in both editions |

The 2017 pages above are the printed footer numbers in the expanded PDF. I rendered and inspected the 2017 diagrams and corresponding 2014 diagrams. The inventory stable IDs were checked before writing; none was already present in an earlier transcription chunk.

## Transcription method and limits

Each record uses the source row sequence to connect a throw node to the next event on the named recipient's row. Self entries remain on the same juggler row. Hand labels alternate R/L. Where the book gives a fractional phase, it is encoded in `startTime`; the edge cycle offset accounts for wraparound at the displayed cycle boundary. The one-count triangle is asynchronous, with the printed phases 0, 4/3, and 2/3 beats. The preceding synchronous one-count patterns are excluded from this batch.

The editor's causal-edge representation does not separately encode whether a pass is straight or crossing, or the number of clubs in a double throw. Those mechanics are documented in each record's notation/limitations while the graph captures the displayed row-to-row causal connections. This chunk does not claim that multiplicity or spatial path has become an editor feature.

## State and offset validation

A temporary C# checker constructed one `CausalDiagramEditorState` per JSON record, called `SynchronizeHeights()`, checked `IsValidForEditor()`, and compared every serialized `cycleOffset` before and after synchronization. Result: **7 records checked, 0 errors**. The phase-shifted four-count and two-count graphs contain wrap edges with offset 1; all offsets remained unchanged after synchronization.

## Remaining pp. 211–215 work

The following patterns remain open and are not import-ready:

- Printed p. 213: synchronous one-count triangle; synchronous one-count triangle with heffs. These use paired simultaneous throws and need explicit support/semantics for same-time events and heff/hold links.
- Printed p. 216 and later: pass-pass-heff, French three-count, why-not/maybe, isosceles variants and the rest of pp. 211–225 are outside this partial batch and remain open.

The three printed p. 215 records are now transcribed and state-checked. Their graph row destinations match the diagrams, but the editor does not represent double-pass multiplicity or straight/crossing trajectory as separate edge properties; those mechanics are documented in `notation` and `modelLimitations`.
