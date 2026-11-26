# Highgate causal transcription audit: printed pp. 271–285

Sources: Aidan Burns, *The New Highgate Collection* (2017 expanded PDF, [source](https://jugglingedge.com/pdf/highgate.pdf)) and *The Highgate Collection* (2014 PDF, [source](https://passing.zone/wp-content/uploads/highgate.pdf)). Each included diagram was read from the page image in both editions. The 2017 printed footer is one less than its PDF page number in this range.

## Included causal graphs

| Stable ID | Pattern | 2017 printed / PDF page | 2014 PDF page | Graph |
|---|---|---:|---:|---|
| `highgate-londonsburning` | London's burning | 272 / 273 | 192 | Four song lines expanded into a 24-position cycle per person; source diagram and row order checked in both editions. |
| `highgate-rotator` | Rotator | 279 / 280 | 197 | Twelve positions; Ben's passes rotate through Anne, Clare, Daniel while the other pairs pass simultaneously. |
| `highgate-threeleafclover` | Three leaf clover | 279 / 280 | 197 | The printed 16-position section changes role assignments. The role permutation is repeated four times to close its 64-position causal cycle. |
| `highgate-pistons` | Pistons | 281 / 282 | 198 | Eight positions; the two pass rounds are Anne–Ben/Clare–Daniel, then Anne–Daniel/Ben–Clare. |
| `highgate-flyingtrapeze` | Flying trapeze | 281 / 282 | 198 | Eight positions; the two pass rounds are Anne–Clare/Ben–Daniel, then Anne–Ben/Clare–Daniel. |
| `highgate-benzenering` | Four person benzene ring | 282 / 283 | 199 | Sixteen positions; Ben and Clare pass twice to each of the stationary Anne and Daniel. |
| `highgate-dosado` | Dosado | 282 / 283 | 199 | Sixteen positions; the same throw graph as the benzene ring with different walking phases. |
| `highgate-stickytriangle` | Sticky triangle | 283 / 284 | 200 | Eight positions; corner/midpoint movement changes the pass partners between the two four-count rounds. |
| `highgate-theweave` | The weave | 284 / 285 | 201 | Six positions; Ben's passes go to Anne, Clare, then Daniel; each walker returns passes to Ben. |
| `highgate-dresserdrawerweave` | Dresser drawer weave | 285 / 286 | 202 | Six positions; same throw graph as The Weave with a different walking route. |

For the walking patterns, fixed people remain the graph lanes. The graph records each throw's recipient over time; the notation retains the movement route and formation. The source causal edges and per-juggler row sequences were compared in both editions. Every JSON record has one outgoing edge per event, valid endpoints, and a cycle that closes. The Clover role change is explicitly included in the longer cycle instead of treating its partial printed diagram as a complete period.

## Reviewed but not represented as a single repeating graph

- **Frère Jacques** (printed p. 270, PDF p. 271; immediately before this audit range) is only an optional ambidextrous variation that refers to Row, Row, Row Your Boat. This page gives no separate event sequence or causal diagram to transcribe.
- **Four person runarounds** (printed p. 273) includes 40 numbered spatial frames for one clockwise example, then says that in general the crossing can happen after any pass. The example fixes crossing points, but the frames show changing positions and pass connections rather than a labeled throw-event schedule; I have not inferred event endpoints or timing from those unlabeled connections.
- **The walk away** (printed pp. 274–275) gives starting, intermediate, and final sequences as walkers change between a two-count line feed and a mixed feed. The described segment ends in a different feeder state and does not give a closing transition back to its start.
- **Tarim's runaway** (printed pp. 276–277) alternates urban-terror and pass-pass-self feeds through two walks. The printed sequence ends with Anne feeding and does not specify a repeated return to the initial Ben-feeder state.
- **Shooting star (11 clubs)** (printed p. 278) explicitly says its diagram is incomplete. A passer who receives no club stops juggling and walks to the empty star point; the source does not give the missing causal/event sequence needed to close the cycle.

These exclusions concern a missing fixed repeating event cycle in the cited pages; walking by itself is not a reason to omit a causal graph.

## Validation

`Highgate271To285StandaloneRecordValidationTests` reads all ten records, checks the expected stable IDs, constructs editor states, calls `SynchronizeHeights()`, verifies that every serialized `CycleOffset` is preserved, and requires `IsValidForEditor()` for each record.

```text
dotnet test Siteswaps.Test/Siteswaps.Test.csproj --no-restore --filter FullyQualifiedName~Highgate271To285StandaloneRecordValidationTests
Passed: 1, Failed: 0
```
