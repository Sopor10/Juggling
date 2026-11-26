# Highgate transcription batch: 2017 printed pages 256–270

The first checked batch is in [highgate-transcriptions-256-270.json](highgate-transcriptions-256-270.json). It contains four named presets, including a separate catalog entry for the drop-back-line variant even though its causal graph matches Four count y.

## Included graph records

| Stable/catalog ID | Pattern | 2017 body page | 2014 body page | Graph |
|---|---|---|---|---|
| `highgate-fourpersonfourcountdropbackline1` | Four person four count drop-back line #1 | printed p. 257 / PDF p. 258 | PDF p. 182 | 16 nodes / 16 edges |
| `highgate-fourcounty` | Four count y | printed p. 258 / PDF p. 259 | PDF p. 183 | 16 / 16 |
| `highgate-fourpersonfourcountdropbackline2` | Four person four count drop-back line #2 | printed p. 258 / PDF p. 259 | PDF p. 183 | same edge graph as Four count y |
| `highgate-fourcountdropbacky` | Four count drop-back y | printed p. 259 / PDF p. 260 | PDF p. 184 | 16 / 16 |

All four IDs match the union inventory and are unique. A scan of central preset data found no existing records for these IDs. The source links are recorded per pattern: [2017 expanded PDF](https://jugglingedge.com/pdf/highgate.pdf), [2014 PDF](https://passing.zone/wp-content/uploads/highgate.pdf).

`Four person four count drop-back line #2` (2017 printed p. 258 / PDF p. 259; 2014 PDF p. 183) is included under its own inventory ID and label. Its throw-target graph is identical to `Four count y`; its line formation and drop-back-over-the-head details differ, but the current editor state has no position or pass-style property to distinguish those details.

## Visual and semantic review

All pages in printed pp. 256–270 (PDF pp. 257–271) were rendered and reviewed. For each included preset, I compared its 4-event row sequence and every arrow in the source diagram. The source says everyone starts with two clubs in the right hand and one in the left; this is included in the notation. Each four-event period returns to the same hand assignment, so a four-beat cycle suffices.

The drop-back lines and Y formation are described in the notation. The graph captures the complete pass/self routing, but cannot encode the jugglers' positions or the visual over-the-head path. No holds, zips, or multi-club edges occur in these records.

## State validation

A temporary validator built a `CausalDiagramEditorPreset` for each JSON record and called `Create()`, then `SynchronizeHeights()` and compared every serialized cycle offset. All `Create()` calls succeeded; each graph has 16 nodes and 16 edges; offsets were unchanged. `IsValidForEditor()` is false with the new IDs until the central known-preset list contains them. Temporarily setting the existing registered ID `highgate-three-count` made each graph pass editor validation. Rerun with intended IDs after import.

## Other patterns reviewed in this span

The section contains more independent structures that remain for follow-up: four-person Rick's picnics (three-, two-, and one-count); Three count y and Jim's three count y; four-count trapeze and all-out civil war feed; bookends/inverted bookends box; double urban terror feed; accommodation/chocolate boxes/bookcases; countdown accommodation patterns; and the sung round patterns. Several have hurried half-beat timing, holds, or special formations. They have not been labeled unrepresentable merely because they are not in this first batch. The generalised Y explanation is a construction recipe rather than a single pattern. Printed pp. 267–270 contain the Row, row, row your boat patterns, its ambidextrous variation, and Frère Jacques; their explicit diagrams remain for a follow-up batch.

No central preset, inventory, or test files were edited for this chunk.
