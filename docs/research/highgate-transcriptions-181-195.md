# Highgate transcription batch: 2017 printed pages 181–195

Importable graph data is in [highgate-transcriptions-181-195.json](highgate-transcriptions-181-195.json). It contains four verified patterns that have one causal edge per throw event and no unrepresented double-pass action.

## Included patterns and inventory IDs

| Stable/catalog ID | Pattern | 2017 body source | 2014 comparison |
|---|---|---|---|
| `highgate-tenclubtwocountfeedonsingles` | Ten club two count feed on singles | printed p. 192 / PDF p. 193 | PDF p. 134 |
| `highgate-tenclubonecountfeedingpasspassselfandthreecount` | Ten club one count feeding pass pass self and three count | printed p. 193 / PDF p. 194 | PDF p. 136 |
| `highgate-tenclubonecountfeedingbookendsandinvertedbookends` | Ten club one count feeding bookends and inverted bookends | printed p. 193 / PDF p. 194 | PDF p. 136 |
| `highgate-tenclubsuburbanterrorfeed` | Ten club suburban terror feed | printed p. 193 / PDF p. 194 | PDF p. 136 |

Source links are stored on each record: [2017 expanded PDF](https://jugglingedge.com/pdf/highgate.pdf) and [2014 PDF](https://passing.zone/wp-content/uploads/highgate.pdf), also bundled as `Siteswaps.Mcp.Server/Resources/highgate.pdf`.

## Visual and semantic checks

I rendered and inspected all source pages in the requested span. For each included record, the named sequence, hand alternation, role offset, and every destination row in the causal diagram were checked against the pattern text. Each exact 2014 counterpart was also visually inspected; the titles, sequences, start phases, and causal lines match. Cycle lengths are the shortest even beat period that repeats the source rows and hand assignment: 4, 6, 10, and 4 beats. This keeps each record compact while preserving the diagram's repeating graph.

The source gives starting club counts and, on some pages, color-coded club identities. Those are recorded in `notation`; the editor graph has no fields for inventory or club color. These records only claim the single-club causal arrows shown in the diagrams.

## Validation

A temporary validator constructed a `CausalDiagramEditorPreset` from every JSON record, called `Create()` and `SynchronizeHeights()`, and checked offsets before and after. Results:

- Two count feed on singles: 12 nodes / 12 edges; `Create()` succeeded; offsets unchanged.
- One count feeding pass pass self and three count: 18 / 18; `Create()` succeeded; offsets unchanged.
- One count feeding bookends and inverted bookends: 30 / 30; `Create()` succeeded; offsets unchanged.
- Suburban terror feed: 12 / 12; `Create()` succeeded; offsets unchanged.

All four records are already imported into the central Highgate select list and are covered by `PresetCatalog_EveryPresetCreatesAValidEditorState`. The loader checks that each serialized edge offset is preserved through `SynchronizeHeights()`.

## Reviewed patterns not in this chunk

The general Bruno's Nightmare introduction on printed p. 181 is explanatory and its diagram emphasizes people walking around a giant's two hands. The ten named variants on printed pp. 182–191 change who feeds and which role each person takes. Their per-phase sequences are present in the text, but they do not supply a checked repeating causal-edge list; the changing role and hand phases still need a full-cycle derivation before a graph can be called complete. The editor does not animate the walking route, so that spatial behavior would remain in the source notes.

## Index and body-page mismatch in the preceding runarounds

The 2017 index candidates immediately before Bruno's Nightmare point four printed pages later than the body text. This was checked directly against the rendered body pages; the matching 2014 text is noted where present.

| Inventory ID | Index page (2017) | Actual 2017 body page | 2014 body page | Finding |
|---|---:|---:|---:|---|
| `highgate-clockwiseonecountrunaround` | 181 | 177 | No matching 2014 index entry found | The text gives three role phases and the named destination sequences. It remains untranslated until those phases are mapped into one complete hand-parity-preserving edge cycle. |
| `highgate-rotatingconfusion` | 182 | 178 | 121 | The text gives two slow/fast feeds, two role rotations, and per-juggler phase sequences. No complete repeating cycle has been validated. |
| `highgate-confusingrotation` | 183 | 179 | 122 | Same rotating feed structure with different count periods; the phase sequence and final role rotation still need to be expanded into a full cycle. |

The index/body discrepancy does not change the four translated ten-club feeds above. Their body diagrams are on 2017 printed pp. 192–193 and their checked 2014 counterparts are pp. 134 and 136. The five unimported static feed variants in this range involve double-pass multiplicity or simultaneous actions, which the current one-edge-per-event graph does not preserve.

Five static ten-club patterns on printed pp. 192–193 were not added because their source says a throw is a double pass or contains double-pass actions: Ten club two count feed on doubles; Asynchronous ten club pass pass self feed (feeder throws single passes); Synchronous ten club pass pass self feed (feeder throws double passes); Synchronous ten club pass pass self feed (feeder throws single double self); and Asynchronous ten club pass pass self feed (feeder throws single double self). The editor permits only one outgoing causal edge per throw node and has no multiplicity field, so representing these as single arrows would lose the double-club action. Printed p. 195 adds color-coding instructions only, no new pattern.

No additional graph was added in this audit: the four source-checked ten-club feed graphs were already present in both the separate JSON chunk and the central list. The remaining ordinary patterns above are recorded as open pending full-cycle derivation; no endpoints were guessed.
