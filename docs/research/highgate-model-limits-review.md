# Highgate causal-diagram model-limit review

This note audits the 24 inventory rows currently marked `model extension required; source retained` against the editor state, SVG renderer, table adapter, and Passist/JIF adapter. It distinguishes limitations of the static causal editor from limitations of playback/export. A row marked as a model limit is not evidence that its source diagram has been fully transcribed.

## Findings and priority

### P0 — Preserve signed causal-cycle offsets

The JSON and token state can store `CycleOffset` values from −1000 to 1000, and the SVG path renderer places the endpoint at `target.StartTime + (sourceCycle + edge.CycleOffset) * CycleLength`. However, `SynchronizeHeights()` increments an offset while the height derived from it is negative. The loader already detects this mutation and rejects a preset, but the editor's normalization still changes the source arrow. For Highgate zips and gaps, this can move a backward arrow into a later cycle and turn the diagram into a different graph.

Inventory entries whose notes explicitly require backward arrows are:

| IDs | Source pages (2017 printed / 2014 PDF) | Evidence |
|---|---|---|
| `highgate-58552` | 2014 p. 25 only | 2014-only 58552 diagram contains backward zip arrows. |
| `highgate-aselftriple`, `highgate-doubletriplezip561` | 24, 28 / 10, 14 | The follow-up zip requires a backward causal edge. |
| `highgate-doublezip51`, `highgate-doublezapzip531`, `highgate-fourfourone`, `highgate-fourfourfourzero` | 22–23, 31 / 8–9, 17 | Zip requires a one-beat backward edge; Four four four zero's gap points two beats backward. |
| `highgate-passselfzip`, `highgate-therecipe` | 32–33, 40 / 18–19, 23 | Source notes require backward zip arrows; The Recipe uses a −2 zip. |
| `highgate-parsnip`, `highgate-fiveclubwhynot`, `highgate-fiveclubnotwhy`, `highgate-fiveclubnotwhyalternativestart` | 41–43 / 24–25 | Why-not/Parsnip graphs include backward zip routes. |
| `highgate-heffzipzap`, `highgate-zapzipheff`, `highgate-zapzipheffalternativestart` | 43 / 25 | Backward zip routes in heff/zap sequences. |
| `highgate-convertingsymmetricasynchronousfourhandedsiteswa-0987b5f53e`, `highgate-convertingsymmetricsynchronousfourhandedsiteswap-d9ed3bf63a` | 39 / 22 | Conversion examples contain backward zip arrows; these are explanatory examples and may not be independent catalog patterns. |

`highgate-thesynchronousrecipe` and `highgate-triplepassholdinsynchonecount6424` also need signed offsets; they additionally need simultaneous-event layout described below.

**Implementation status:** the editor state now uses compact link format `j4` and stores an explicit-height flag per throw. `SynchronizeHeights()` preserves signed edge offsets; graph-derived heights are recalculated only for non-explicit throws. Older `j3`, `j2`, `j1`, and JSON links migrate through the previous positive-height normalization. Manual node drags clamp to timing positions that keep derived heights valid; arrow retargeting remains the operation that changes the causal destination/offset.

State tests now cover negative-offset round trips, explicit-height round trips, legacy-link migration, and drag clamping. The Highgate JSON importer accepts an explicit nonnegative `height` plus `heightIsExplicit: true` on source throw nodes, and rejects a missing/negative explicit height. The 141 `Siteswaps.Test` tests and the four `CausalDiagramArrowDragTests` pass. **This does not yet make the Highgate zip/gap rows translated:** no source-reviewed explicit-height graph has been imported yet, and source diagrams must be transcribed and visually checked before those rows can leave the retained/model-review status. A rendered SVG assertion for the leftward target is also still needed.

### P1 — Allow distinct events at the same juggler and time

The current data model has a `Hand` on each node, but the SVG assigns every node for a juggler to `RowY(JugglerIndex(...))`; its x coordinate is derived from `StartTime`. Two same-time events by that juggler therefore occupy the same position. The table adapter also selects the first event in a juggler/time cell, silently hiding any additional event.

| IDs | Source pages (2017 printed / 2014 PDF) | Evidence |
|---|---|---|
| `highgate-simultaneousheffs` | 23 / 9 | Two heffs happen at the same time for one juggler. |
| `highgate-technosevenclubthreecountonsingles`, `highgate-scratchyourhead` | 26 / 12 | Same-time two-hand actions (pass/self or pass/heff) are shown. |
| `highgate-oddzgodz` | 27 / 13 | Simultaneous two-hand events and phase shift. |
| `highgate-thesynchronousrecipe` | 35–36 / 20–21 | Simultaneous events plus backward zips. |
| `highgate-triplepassholdinsynchonecount6424` | 29 / 15 | Synchronous two-hand throws plus zip variant. |

**Code work:** define whether a juggler row is one track or two hand tracks. For fidelity with the source diagrams, a hand sub-lane or a deterministic same-time node offset is needed in the SVG; edge endpoints must use that same position. The event table must represent multiple events in one time column, for example with hand-specific subcells, instead of taking `FirstOrDefault()`. Preserve keyboard/touch hit targets and test an actual phone-width view because two-hand nodes will be closer together.

### P2 — Add finite entry/exit or transition events where the source is not periodic

`CausalDiagramEditorState` stores a repeating `CycleLength` and the renderer repeats each node across `Repetitions`. This fits a steady-state cycle. It cannot faithfully express a finite lead-in, excited section, and exit whose events occur only once. The inventory has a related candidate, `highgate-tripleself73` (printed p. 30), still open for transcription: source review describes it as entry → excited cycle → exit, so it needs a finite-transition representation, not just a periodic preset. The separate `highgate-aselftriple` is periodic but has the backward zip described in P0.

**Code work:** model a pattern as a steady-state cycle plus optional finite prefix/suffix (or as a finite event graph with explicit repeat segments). Serialization, link state, rendering, table projection, and generation/export must preserve which nodes repeat. Do not encode a finite transition by duplicating it into the cycle; that changes the pattern.

### P3 — Keep event/action semantics explicit across editor and playback

Static causal graphs can show holds as disconnected event nodes: several existing presets contain explicit hold nodes and remain valid. This is distinct from the missing-edge fallback in `CausalDiagramEditorJifAdapter`: it infers a self target for an unconnected node, then derives duration from its height. A hold has height zero, which fails the adapter’s positive-duration check. Thus a static hold can be represented today, but it cannot yet be exported as a faithful Passist animation.

The reviewed set above is classified as ordinary patterns in the inventory; the separate manipulator research has additional candidates. Before importing zaps, heffs, steals, or hijacks, the model needs a declared event vocabulary (for example pass, self, hold, zip, heff, manipulation/transition), with a documented meaning for each edge and playback behavior. Preserve the actor/recipient and event kind rather than turning an unfamiliar action into a normal self edge. This is especially relevant to the `why not` families, whose inventory classification currently says ordinary but whose transition semantics still need source-level review.

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

This audit covers the 24 rows currently labeled model-limited. It is not a full audit of all 919 inventory candidates, and it does not establish that the other open candidates are representable or fully transcribed.
