# Highgate transcription batch: 2017 printed pages 166–180

The importable graph record is in [highgate-transcriptions-166-180.json](highgate-transcriptions-166-180.json). This is a first checked candidate from the span, not a claim that every pattern on these pages has been transcribed.

## Included record

| Inventory ID | Pattern | 2017 body page | 2014 body page | Graph |
|---|---|---:|---:|---:|
| `highgate-twocountfeastrunaround` | Two count feast runaround | printed p. 171 / PDF p. 172 | PDF p. 116 | 36 nodes, 36 edges |

Sources: [2017 expanded PDF](https://jugglingedge.com/pdf/highgate.pdf), Aidan Burns; [2014 PDF](https://passing.zone/wp-content/uploads/highgate.pdf), also bundled locally at `Siteswaps.Mcp.Server/Resources/highgate.pdf`.

## Visual transcription

I checked both source pages visually. They show the same three-person causal diagram and the same six two-beat phases: the starting phase, after Anne walks, and after Ben walks, repeated once with the opposite hand; after Clare walks the pattern returns to the start. The node rows in the diagram contain twelve alternating-hand events per juggler. In each phase, each source event connects to the next event on the receiving juggler's row; self throws continue on the same row. All 36 edges and the changing target rows were checked against the source diagram and the accompanying phase descriptions.

The graph's phases encode the changing feed/pass network. The editor data model has no separate spatial position or walking-animation field, so the physical route shown in the lower source diagram is described in `notation` but is not animated.

## Validation

A temporary validator built a `CausalDiagramEditorPreset` from the JSON record and called `Create()`, then `SynchronizeHeights()` and compared offsets. `Create()` succeeded; the state has 36 nodes and 36 edges, and all cycle offsets were unchanged. `IsValidForEditor()` was false before registering the new preset ID; after import the catalog-wide validity test verifies the actual ID.

## Other patterns in this page span

The reviewed pages also include traffic lights, pulsar runarounds, pass-pass-self runarounds, one-count runarounds, and rotating/confusing rotations. They include additional feeder phases and/or more complex walk sequences. They are not included in this first chunk; each needs a separate full phase-to-diagram audit before import. No central preset, inventory, or test files were edited for this chunk.
