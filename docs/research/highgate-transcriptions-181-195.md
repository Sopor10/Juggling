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

`IsValidForEditor()` is false on the actual IDs until they are added to the central known-preset list. Temporarily using the existing registered ID `highgate-three-count` made each graph pass validation. Rerun validation with the real IDs after central import.

## Reviewed patterns not in this chunk

The general Bruno's Nightmare introduction on printed p. 181 is explanatory. Its ten named variants on printed pp. 182–191 all include people walking around a giant's two hands, changing who is the feeder and which role each person takes. The current editor graph has no position or walking transition data, so these variants cannot be represented as complete patterns without dropping source behavior.

Five static ten-club patterns on printed pp. 192–193 were not added because their source says a throw is a double pass or contains double-pass actions: Ten club two count feed on doubles; Asynchronous ten club pass pass self feed (feeder throws single passes); Synchronous ten club pass pass self feed (feeder throws double passes); Synchronous ten club pass pass self feed (feeder throws single double self); and Asynchronous ten club pass pass self feed (feeder throws single double self). The editor permits only one outgoing causal edge per throw node and has no multiplicity field, so representing these as single arrows would lose the double-club action. Printed p. 195 adds color-coding instructions only, no new pattern.

No central runtime, inventory, or test files were edited for this chunk.
