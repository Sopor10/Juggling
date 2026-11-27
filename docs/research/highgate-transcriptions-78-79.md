# Highgate slow-fast source graphs: printed pp. 78–79

The selected records are in [highgate-transcriptions-78-79.json](highgate-transcriptions-78-79.json) and [the follow-up source records](highgate-transcriptions-78-79-follow-up.json). Five static, non-manipulation patterns are checked against the 2017 expanded book and the matching 2014 edition. The sources print the action rows beside the causal diagrams; their different horizontal event spacing encodes Anne's faster tempo.

## Source-checked patterns

| Stable ID | Pattern | 2017 source | 2014 source | Graph |
|---|---|---|---|---|
| `highgate-fourcountvsthreecount` | Four count vs three count | printed p. 78 / PDF p. 79 | printed p. 50 / PDF p. 50 | 14 events / 14 edges, six-beat cycle |
| `highgate-threecountvstwocount` | Three count vs two count | printed p. 78 / PDF p. 79 | printed p. 50 / PDF p. 50 | 10 events / 10 edges, four-beat cycle |
| `highgate-twocountvsonecount` | Two count vs one count | printed p. 79 / PDF p. 80 | printed p. 50 / PDF p. 50 | 6 events / 6 edges, two-beat cycle |
| `highgate-twocountvspasspassself` | Two count vs pass pass self | printed p. 78 / PDF p. 79 | printed p. 50 / PDF p. 50 | 14 events / 14 edges, six-beat cycle |
| `highgate-passpassselfvsonecount` | Pass pass self vs one count | printed p. 79 / PDF p. 80 | printed p. 50 / PDF p. 50 | 10 events / 10 edges, four-beat cycle |

Both editions show the same receiver arrows and action sequences. For four count versus three count, Anne's eight-event lane advances every 3/4 beat and Ben's six-event lane every beat; their common cycle is six beats. For three count versus two count, Anne's six-event lane advances every 2/3 beat and Ben's four-event lane every beat; their common cycle is four beats. For two count versus one count, the four-event Anne lane advances every 1/2 beat and the two-event Ben lane every beat; their common cycle is two beats. The diagram endpoints determine which event catches each pass, including the arrows that cross the end of a lane cycle.

Straight and crossing trajectories are retained in the notation. The causal editor stores event time, receiving event, and cycle wrap; it does not encode trajectory or starting club inventory.

## Import and validation

All five patterns are registered through the embedded `highgate-causal-patterns.json` resource and appear in the Highgate preset list. Their standalone NUnit tests check the per-lane event times, every source-row receiver, each diagram-derived lane index and cycle offset, editor validity, offset preservation through `SynchronizeHeights()`, and equality with the imported runtime presets. The focused tests pass: **5 source records, 54 events, 54 edges, 0 failures**.

## Remaining candidates in this section

The remaining source candidates on printed pp. 77–78 are not included: interactive Passing games are stateful rules rather than fixed repeating graphs, while colour-coded patterns depend on club identity or leader-dependent count changes.
