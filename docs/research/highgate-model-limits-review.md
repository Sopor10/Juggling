# Highgate causal-diagram model-limit review

This note audits the 3 inventory rows still marked `model extension required; source retained` against the editor state, SVG renderer, table adapter, and Passist/JIF adapter. It distinguishes limitations of the static causal editor from limitations of playback/export. A row marked as a model limit is not evidence that its source diagram has been fully transcribed. Since the first audit, the signed-offset editor work has landed; read the implementation-status updates below before interpreting the original inventory labels.

## Findings and priority

### P0 — Preserve signed causal-cycle offsets (implemented)

The JSON and token state can store `CycleOffset` values from −1000 to 1000, and the SVG path renderer places the endpoint at `target.StartTime + (sourceCycle + edge.CycleOffset) * CycleLength`. The earlier implementation incremented an offset when its derived height was negative, which changed backward arrows. That behavior is fixed: `SynchronizeHeights()` now preserves the signed offset and recalculates only non-explicit heights. Zip/gap rows still require exact source transcription, but backward timing is no longer an editor-model blocker.

The backward-arrow support is now implemented. Six source-checked ordinary graphs that were previously held back by signed offsets are now translated; seven other periodic patterns have moved to graph-transcription-pending status. The remaining true model limit in the current inventory is finite transitions; simultaneous-event layout is now implemented.

| IDs | Source pages (2017 printed / 2014 PDF) | Evidence |
|---|---|---|
| `highgate-58552` | 2014 p. 25 only | 2014-only 58552 diagram contains backward zip arrows. |
| `highgate-aselftriple`, `highgate-doubletriplezip561` | 24, 28 / 10, 14 | The follow-up zip requires a backward causal edge. |
| `highgate-doublezip51`, `highgate-doublezapzip531`, `highgate-fourfourone`, `highgate-fourfourfourzero` | 22–23, 31 / 8–9, 17 | Zip requires a one-beat backward edge; Four four four zero's gap points two beats backward. |
| `highgate-therecipe` | 32–33 / 18–19 | Source notes require backward zip arrows; The Recipe uses a −2 zip. |
| `highgate-parsnip`, `highgate-fiveclubwhynot`, `highgate-fiveclubnotwhy`, `highgate-fiveclubnotwhyalternativestart` | 41–43 / 24–25 | Why-not/Parsnip graphs include backward zip routes. |
| `highgate-heffzipzap`, `highgate-zapzipheff`, `highgate-zapzipheffalternativestart` | 43 / 25 | Backward zip routes in heff/zap sequences. |
| `highgate-convertingsymmetricasynchronousfourhandedsiteswa-0987b5f53e`, `highgate-convertingsymmetricsynchronousfourhandedsiteswap-d9ed3bf63a` | 39 / 22 | Conversion examples contain backward zip arrows; these are explanatory examples and may not be independent catalog patterns. |

`highgate-thesynchronousrecipe` and `highgate-triplepassholdinsynchonecount6424` previously also needed simultaneous-event layout. Signed offsets and the same-time layout are now supported; only source transcription remains.

**Early-page audit update:** `highgate-fourfourone`, `highgate-fourfourfourzero`, `highgate-aselftriple`, `highgate-doublezapzip531`, `highgate-therecipe`, and the 2014-only `highgate-58552` now have source-checked graph records in [highgate-signed-zip-transcriptions.json](highgate-signed-zip-transcriptions.json) and are imported in the central catalog. NUnit validates the source graphs and preset editor states. `highgate-doubletriplezip561` and `highgate-doublezip51` remain finite-transition cases, separately described below.

**Implementation status:** the editor state now uses compact link format `j4` and stores an explicit-height flag per throw. `SynchronizeHeights()` preserves signed edge offsets; graph-derived heights are recalculated only for non-explicit throws. Older `j3`, `j2`, `j1`, and JSON links migrate through the previous positive-height normalization. Manual node drags clamp to timing positions that keep derived heights valid; arrow retargeting remains the operation that changes the causal destination/offset.

State tests cover negative-offset round trips, explicit-height round trips, legacy-link migration, and drag clamping. The Highgate JSON importer accepts an explicit nonnegative `height` plus `heightIsExplicit: true` on source throw nodes, and rejects a missing/negative explicit height. Current unit tests exercise every imported preset's editor validity; the three-person *Pass pass zip feeding* diagrams also exercise backward zip edges with source heights. The signed-offset blocker is resolved, while untranscribed zip/gap rows still need source diagrams checked edge by edge.

`highgate-passselfzip` (siteswap 726) is now translated and imported with twelve events, twelve edges, and a source-matched −1 cycle offset on Anne's final zip. This verifies a backward zip loop through the runtime catalog and its validity test.

### P1 — Implemented: distinct same-time events on separate hand tracks

The earlier renderer put every event for one juggler on the same y-coordinate, and the table kept only the first event in a juggler/time cell. The editor now detects simultaneous events and places left- and right-hand nodes on separate tracks at ±30 SVG units. It raises the effective spacing between jugglers to 120 units when those tracks are needed. Causal paths, drag targets, and edit controls all use the same event coordinate. The table splits that juggler into L/R rows and retains multiple chips in every cell.

| IDs | Source pages (2017 printed / 2014 PDF) | Evidence |
|---|---|---|
| `highgate-simultaneousheffs` | 23 / 9 | Two heffs happen at the same time for one juggler. |
| `highgate-technosevenclubthreecountonsingles`, `highgate-scratchyourhead` | 26 / 12 | Same-time two-hand actions (pass/self or pass/heff) are shown. |
| `highgate-oddzgodz` | 27 / 13 | Simultaneous two-hand events and phase shift. |
| `highgate-thesynchronousrecipe` | 35–36 / 20–21 | Simultaneous events plus backward zips. |
| `highgate-triplepassholdinsynchonecount6424` | 29 / 15 | Synchronous two-hand throws plus zip variant. |

The unit test checks distinct hand tracks and target separation. The Playwright test renders a same-time R/L state at 390×844 with touch enabled, verifies both 56×56 targets and separate table rows, and performs a touch drag. The focused E2E passes.

### P2 — Partial implementation: finite sequence phases and transitions

`CausalDiagramEditorState` stores a repeating `CycleLength` and the renderer repeats each node across `Repetitions`. This fits a steady-state cycle. It cannot faithfully express a finite lead-in, excited section, and exit whose events occur only once. The inventory has a related candidate, `highgate-tripleself73` (printed p. 30), still open for transcription: source review describes it as entry → excited cycle → exit, so it needs a finite-transition representation, not just a periodic preset. `highgate-aselftriple` is a periodic pattern and its signed zip is now represented in the imported graph.

The editor now has a separate finite-sequence catalog and `j5.` state links. Thirteen-club Runaway exposes its four source-backed periodic phases in the Highgate preset list, supports phase navigation, and describes the two walk transitions. Existing periodic `j4.` links remain unchanged. Its intermediate three-self-throw intervals and movement paths are not drawn as event graphs because the source does not specify a full event-by-event transition chart.

**Remaining code work:** represent finite entry/exit actions as causal events and connect the transition graph to rendering, table projection, and animation. Do not encode a finite transition by duplicating it into the repeating cycle; that changes the pattern. Runaway's transition summary is useful but does not complete this requirement; the triple-self entry/exit candidate and the finite zip patterns still need source transcription and full transition graph support.

### P3 — Keep event/action semantics explicit across editor and playback

Static causal graphs can show holds as disconnected event nodes: several existing presets contain explicit hold nodes and remain valid. This is distinct from the missing-edge fallback in `CausalDiagramEditorJifAdapter`: it infers a self target for an unconnected node, then derives duration from its height. A hold has height zero, which fails the adapter’s positive-duration check. Thus a static hold can be represented today, but it cannot yet be exported as a faithful Passist animation.

The reviewed set above is classified as ordinary patterns in the inventory; the separate manipulator research has additional candidates. For static diagrams, the imported records keep unfamiliar action names, explicit heights, hand routes, and source references in notation and source notes; the 11 audited periodic patterns now pass edge-by-edge checks, including the three 2.5-beat half-spin zap patterns. Passist export still lacks a full action vocabulary for zaps, holds, steals, and hijacks. Do not infer a playback action from a causal edge alone.

## Representable today, with limits stated accurately

- Explicit holds can be shown as zero-edge nodes in the static graph. The newer early/late triple presets demonstrate this. That does not yet prove correct Passist playback.
- A disconnected node is accepted by `IsValidForEditor`; this validator checks structural invariants, not that the source diagram's entire sequence was transcribed. The catalog test catches structurally invalid presets before serialization, while per-batch arrow audits are still needed for semantic completeness.
- More than one incoming edge is allowed by editor-state validation and can be drawn, but Passist export rejects duplicate `(target, cycle)` arrivals. This is a playback/export limitation, not a reason to discard a valid static causal diagram.
- Simultaneous nodes have valid state data if they use different IDs; their current overlap is a rendering/table issue, not a prohibition in `IsValidForEditor`.

## Source and implementation references

- Inventory evidence: [highgate-pattern-inventory.csv](highgate-pattern-inventory.csv), rows with `processing_status=model extension required; source retained`.
- Current model behavior: `Siteswaps.Components/CausalDiagram/CausalDiagramEditorState.cs` (`IsValid`, `SynchronizeHeights`, `DerivedHeight`).
- SVG row/time projection: `Siteswaps.Components/CausalDiagram/CausalDiagramEditorView.razor` (`RowY`, `CausalPath`).
- Timeline table projection: `Siteswaps.Components/CausalDiagram/CausalDiagramEditorTableAdapter.cs`.
- Passist playback projection and duplicate-arrival limit: `Siteswaps.Components/CausalDiagram/CausalDiagramEditorJifAdapter.cs`.
- Existing source-specific evidence and model notes: [highgate-causal-translation.md](highgate-causal-translation.md), [highgate-transcriptions-30-45.md](highgate-transcriptions-30-45.md), and [highgate-transcriptions-91-105.md](highgate-transcriptions-91-105.md).

## Scope note

This audit covers the 3 rows currently labeled model-limited. It is not a full audit of all 919 inventory candidates, and it does not establish that the other open candidates are representable or fully transcribed.
