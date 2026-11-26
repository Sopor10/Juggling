# Thirteen club runaway: transition review

The named pattern is described on 2017 expanded printed pp. 323–325 (PDF pp. 324–326) and 2014 PDF pp. 232–233. The four source states are in [highgate-runaway-source-states.json](highgate-runaway-source-states.json). The action rows and hand diagrams were checked in both editions; the embedded row diagrams match pixel for pixel.

| State graph | 2017 printed / PDF | 2014 printed / PDF | State description |
|---|---|---|---|
| `highgate-thirteenclubrunaway-initial-urban-terror` | 323 / 324 | 232 / 232 | Initial urban-terror feed, before Daniel walks across. |
| `highgate-thirteenclubrunaway-after-daniel-walk` | 323 / 324 | 232 / 232 | Pass-pass-self feed after Daniel's walk. |
| `highgate-thirteenclubrunaway-before-anne-walk` | 325 / 326 | 233 / 233 | Pass-pass-self feed immediately before Anne walks. |
| `highgate-thirteenclubrunaway-after-anne-walk` | 325 / 326 | 233 / 233 | Urban-terror feed after Anne's walk, with Clare feeding the other three. |

Each state record expands the source's three-action rows to a six-event period so alternating hands return to the same phase. The focused test checks that every printed actor row maps to the correct target juggler and hand, that each periodic state passes editor validation, and that height synchronization preserves the source cycle offsets. Both tests pass.

## Why these are not one selectable preset

The full named pattern is an ordered transition: Daniel walks across after a pass; the group changes from an urban-terror feed to a pass-pass-self feed; later Anne walks across after passing with Daniel and the group changes back to an urban-terror feed. The source also says the walker has 2.5 beats to throw three self throws, requiring a temporary faster rhythm. The current editor stores only one repeating cycle and has no formation or finite-transition state. Selecting any one of the four phase records as `Thirteen club runaway` would omit the transitions and walking. Keep the source states available for research until the editor supports a finite sequence of causal-graph phases and movement cues.
