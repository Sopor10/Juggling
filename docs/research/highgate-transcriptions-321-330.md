# Highgate causal transcription batch: printed pp. 321–330

This chunk contains every clear stationary pattern from printed pp. 321–323, plus the Double Urban Terror Feed whose diagram starts on p. 320 and whose row descriptions continue on p. 321. Patterns requiring walking or simultaneous throws are inventoried below as model-boundary items and are not represented as stationary graphs.

## Included, visually verified graphs

| Inventory stable ID | Pattern | 2017 printed page | 2014 printed page | Nodes / edges |
|---|---|---:|---:|---:|
| `highgate-thirteenclubdoubleurbanterrorfeed` | Thirteen club double urban terror feed | 320–321 | 229 | 24 / 24 |
| `highgate-thirteenclubthreecountaccommodation` | Thirteen club three count accommodation | 321 | 230 | 24 / 24 |
| `highgate-thirteenclublowfatchocolatebox` | Thirteen club low fat chocolate box | 321 | 230 | 48 / 48 |
| `highgate-thirteenclubbookshelf` | Thirteen club bookshelf | 322 | 231 | 120 / 120 |
| `highgate-thirteenclubchocolatebookshelf` | Thirteen club chocolate bookshelf | 322 | 231 | 80 / 80 |
| `highgate-thirteenclubcountdownaccommodation` | Thirteen club countdown accommodation | 323 | 232 | 32 / 32 |

I visually checked the complete causal diagrams in both editions and compared each row's recipient sequence with the explanatory text. The inventory stable IDs were checked against all existing transcription chunks before writing.

## Transcription and model boundaries

Each event row has alternating R/L hand labels. The printed row sequence determines the receiving juggler for each event; each edge continues to the next event on that recipient's row. Fractional starts for Anne and Clare are encoded as 0.5-beat offsets. For *Thirteen club bookshelf*, the book says the pattern repeats with the other hand and its diagram shows half the pattern, so the 15-event source sequence is doubled into a 30-event full cycle.

The stationary patterns are represented as complete event/recipient graphs. The editor does not separately encode starting club counts, pass style (straight/crossing), or feed-family names. Those details remain in the record's notation and limitations. The 2017 chunk is complete for the clear stationary diagrams in pp. 321–323, not for every named pattern in pp. 321–330.

These named patterns in the remainder of the section require movement and/or synchronized throws, so they remain outside the graph batch:

- Printed pp. 323–325: Thirteen club runaway (walking switches between urban terror and pass-pass-self feed).
- Printed p. 326: Thirteen club weave (walks a figure eight).
- Printed p. 327: Thirteen club dresser drawer weave (walks along a line).
- Printed p. 328: Thirteen club it's a good one (countdown weave; figure-eight movement and left-hand passes).
- Printed p. 329: Thirteen club gorilla weave (figure-eight walking).
- Printed p. 330: Thirteen club synchronous gorilla weave (synchronous feeder plus figure-eight walking).

The preceding Thirteen club two count and one count patterns start and finish on printed p. 320, before this range. The Double Urban Terror Feed is included because its sequence text continues on p. 321; its diagram is on p. 320.

## Direct C# state and offset check

A temporary C# checker created one `CausalDiagramEditorState` per included record, called `SynchronizeHeights()`, checked `IsValidForEditor()`, and compared every edge's `cycleOffset` before and after synchronization. Result: **6 records checked, 0 errors**; all serialized offsets were preserved. I also audited every source row token against the graph destination. The six graphs contain 157 `self` entries, and all 157 have an edge continuing to the next event on the same juggler row; there were zero sequence-to-edge mismatches. None of these six source patterns contains an explicit `hold` beat, so there are no omitted hold nodes.
