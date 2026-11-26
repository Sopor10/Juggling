# Highgate causal transcription batch: printed pp. 271–285

The included graph is in [highgate-transcriptions-271-285.json](highgate-transcriptions-271-285.json). I read and visually scanned every page in the span (2017 expanded PDF pp. 272–286; printed footers 271–285), including the causal diagrams and movement panels.

## Included graph

| Inventory stable ID | Pattern | 2017 printed page | 2014 printed page | Graph/check |
|---|---|---:|---:|---|
| `highgate-londonsburning` | London's burning | 272 | 192 | 96 nodes, 96 edges; visual comparison in both editions |

The row sequences were expanded from the four song-line patterns and each juggler's offset through the round. The four-person box relation used by the diagram is encoded in the edge recipients: Anne opposite Clare, Ben opposite Daniel, and the adjacent left/right recipients follow the clockwise box order shown in the diagram. The printed starting state is two clubs in the right hand and one in the left hand for each juggler.

## State and offset validation

A temporary C# checker created a `CausalDiagramEditorState` for the record, called `SynchronizeHeights()`, verified `IsValidForEditor()`, and compared every edge's serialized `cycleOffset` before and after synchronization. Result: **1 record checked, 0 errors**; all 96 offsets were preserved.

## Other material reviewed in this span

These pages contain no other clear, stationary ordinary pattern with a complete causal graph suitable for this chunk. The following are recorded as open/model-boundary items rather than flattened into misleading stationary graphs:

- Printed p. 271: a note suggesting an ambidextrous *Frère Jacques* variant by reusing another pattern; no separate causal diagram.
- Printed p. 273: Four person runarounds; the diagram changes partner positions and alternates box and line feeds while jugglers walk.
- Printed pp. 274–275: The walk away; the feed topology changes after Daniel and Clare walk across.
- Printed p. 276: Tarim's runaway; transitions between urban terror feed and pass-pass-self box with two walks.
- Printed p. 278: Shooting star; the diagram is a partial spatial route and jugglers stop juggling to move to an empty star point.
- Printed p. 279: Rotator and Three leaf clover; a rotating central role and successive role swaps change the spatial arrangement.
- Printed p. 281: Pistons and Flying trapeze; both require repeated walking around squares.
- Printed p. 282: Four person benzene ring and Dosado; participants walk around an oval and exchange moving roles.
- Printed p. 283: Sticky triangle; movement depends on corner/midpoint occupancy and the empty position.
- Printed pp. 284–285: The weave and Dresser drawer weave; three walkers move through paths around a feeder while their pass targets track those positions.

The causal editor does not store or animate spatial position, role movement, or walking-triggered phase changes as a separate state. Those patterns need an explicit spatial/phase model before they can be represented faithfully.
