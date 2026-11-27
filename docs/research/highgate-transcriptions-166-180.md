# Highgate transcription batch: 2017 printed pages 166–180

The graph records are in [highgate-transcriptions-166-180.json](highgate-transcriptions-166-180.json). The four two-count/pass-pass-self runarounds already have catalog coverage. The clockwise one-count runaround is newly transcribed and has standalone validity coverage; the new pattern is imported into the runtime catalog and marked translated in the inventory.

## Included record

| Inventory ID | Pattern | 2017 body page | 2014 body page | Graph |
|---|---|---:|---:|---:|
| `highgate-twocountfeastrunaround` | Two count feast runaround | printed p. 171 / PDF p. 172 | PDF p. 116 | 36 nodes, 36 edges |
| `highgate-anticlockwisetwocountrunaround` | Anticlockwise two count runaround | printed p. 167 / PDF p. 168 | PDF p. 114 | 24 nodes, 24 edges |
| `highgate-clockwisetwocountrunaround` | Clockwise two count runaround | printed p. 172 / PDF p. 173 | PDF p. 114 | Same 24-node throw graph; only the spatial walking direction changes |
| `highgate-anticlockwisepasspassselfrunaround` | Anticlockwise pass pass self runaround | printed p. 174 / PDF p. 175 | PDF p. 117 | Six-beat cycle, 18 nodes and 18 edges |
| `highgate-clockwisepasspassselfrunaround` | Clockwise pass pass self runaround | printed p. 176 / PDF p. 177 | PDF p. 118 | Six-beat cycle, 18 nodes and 18 edges |
| `highgate-clockwiseonecountrunaround` | Clockwise one count runaround | printed p. 178 / PDF p. 179 | PDF p. 116 | 18-beat hand cycle, 54 events and 54 edges |

Sources: [2017 expanded PDF](https://jugglingedge.com/pdf/highgate.pdf), Aidan Burns; [2014 PDF](https://passing.zone/wp-content/uploads/highgate.pdf), also bundled locally at `Siteswaps.Mcp.Server/Resources/highgate.pdf`.

## Visual transcription

I checked both source pages visually. They show the same three-person causal diagram and the same six two-beat phases: the starting phase, after Anne walks, and after Ben walks, repeated once with the opposite hand; after Clare walks the pattern returns to the start. The node rows in the diagram contain twelve alternating-hand events per juggler. In each phase, each source event connects to the next event on the receiving juggler's row; self throws continue on the same row. All 36 edges and the changing target rows were checked against the source diagram and the accompanying phase descriptions.

The graph's phases encode the changing feed/pass network. The editor data model has no separate spatial position or walking-animation field, so the physical route shown in the lower source diagram is described in `notation` but is not animated.

The anticlockwise two-count runaround has two four-beat phases: Ben feeds Anne and Clare, then after Anne walks Clare feeds Anne and Ben. The diagram repeats these phases after Clare walks. I represented them as an eight-beat cycle with eight alternating-hand events per juggler. The printed starting and after-Anne sequences name the receiving juggler for every pass; each edge lands on the next event of that named juggler, and self throws continue on the same row. All 24 edges derive height 3 and have one incoming and one outgoing edge per event.

The two pass pass self runarounds have two three-beat phases. The starting and after-walk sequences on printed pp. 174 and 176 specify each source event's receiver, so the event cycle is six beats per juggler. Each pass or self connects to the next event on its named receiver's lane. The 18 edges in each graph derive height 3; hand labels alternate across the six-event cycle.

The clockwise one-count runaround on printed p. 178 has three phases of three actions per juggler. Those nine-action rows repeat with opposite-hand labels, producing an 18-beat cycle with 18 events per person. I compared the action rows and diagram with the 2014 body page 116. Each throw connects to the next event on its named receiver's row; the resulting 54 edges derive height 3 and preserve hand parity.

## Validation

A focused NUnit test builds editor states from every record in this chunk using a registered ID for the known-ID guard, calls `SynchronizeHeights()`, checks that cycle offsets are unchanged, and requires `IsValidForEditor()` to pass. The central catalog-wide validity test also checks the four newly imported presets under their own IDs. The two-count feast runaround has 36 nodes and 36 edges; the anticlockwise two-count runaround has 24 nodes and 24 edges.

## Other patterns in this page span

Traffic lights, anti-quasar, quasar, and the anticlockwise pulsar patterns are already in the central catalog. This chunk also carries the two-count runarounds and clockwise/anticlockwise pass-pass-self runarounds, plus the newly transcribed clockwise one-count runaround. The clockwise pulsar runaround remains open: flattening its described stage rows creates duplicate incoming targets, so its arrows and phase boundaries need a direct diagram audit before its graph is trustworthy. “Rotating confusion” (2017 printed p. 179) and “Confusing rotation” (printed p. 180) are also open; both use crossing passes and variable slow/fast phase lengths and need detailed source-to-event mapping. These are non-manipulation passing candidates. No central preset or inventory files were edited for this chunk.
