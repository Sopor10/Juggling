# Highgate transcription batch: 2017 printed pages 241–255

This first importable batch is in [highgate-transcriptions-241-255.json](highgate-transcriptions-241-255.json). It covers four static four-person feasts from the beginning of the span. The remaining patterns in the span are listed below as reviewed but not yet transcribed; this is not a completeness claim for pp. 241–255.

## Included patterns

| Inventory/catalog ID | Pattern | 2017 body page | 2014 body page | State |
|---|---|---|---|---|
| `highgate-fourpersonfourcountfeast` | Four person four count feast | printed p. 245 / PDF p. 246 | PDF p. 173 | 64 nodes / 64 edges |
| `highgate-fourpersonthreecountfeast` | Four person three count feast | printed p. 245 / PDF p. 246 | PDF p. 173 | 48 / 48 |
| `highgate-fourpersontwocountfeast` | Four person two count feast | printed p. 245 / PDF p. 246 | PDF p. 173 | 32 / 32 |
| `highgate-fourpersononecountfeast` | Four person one count feast | printed p. 246 / PDF p. 247 | PDF p. 174 | 16 / 16 |

The data uses the stable inventory IDs. I checked those IDs against the central source and preset list; none is already present there. Sources are [the 2017 expanded PDF](https://jugglingedge.com/pdf/highgate.pdf) and [the 2014 PDF](https://passing.zone/wp-content/uploads/highgate.pdf), also available locally as `Siteswaps.Mcp.Server/Resources/highgate.pdf`.

## Transcription and source review

I rendered all requested source pages. For each included feast, I checked the book's four-person sequences against the causal diagram: every pass routes to the named person, every `self` stays on the same row, hands alternate, and the final edge wraps to the next cycle. The shortest full action period is used (16, 12, 8, or 4 beats), which preserves every source event without repeating the diagram's display-only extra cycles. The corresponding 2014 diagrams on PDF pp. 173–174 were also visually compared; titles, sequences, initial hand inventories, and causal edges match.

The source gives the initial inventory as two clubs in the right hand and one in the left hand for each juggler. That inventory, plus the 2014 three-count feast's red/white color key, is retained in the record notes; the editor has no club-inventory or color fields.

## State validation

A temporary validator constructed a `CausalDiagramEditorPreset` from each JSON record and called `Create()`, `SynchronizeHeights()`, then compared serialized cycle offsets. All four `Create()` calls succeeded, all offsets were preserved, and the graph passed `IsValidForEditor()` after temporarily replacing the new ID with the registered `highgate-three-count` ID. The actual IDs are not yet in the central catalog, so `IsValidForEditor()` currently returns false for the raw records; rerun after import with the intended IDs. No central runtime, inventory, or test files were changed.

## Remaining reviewed material

The rest of the requested range contains more candidates that still need individual transcription and state checks:

- Printed pp. 241–244: Bookends line feed; Mob neurosis/psychosis; Parsnip line feed; Funky bookends line feed; One count feeding why not?/five-club why not?/funky bookends/maybe; these include holds, zips, heffs, or more involved line-feed routes.
- Printed pp. 246–247: pass pass self feast; bookends feast; two- and one-count revolving feeds; quasar.
- Printed pp. 248–251: countdown feast; fizz buzz feast; selfless fizz buzz; Aidan's feasts #1–#4 and extended feast #1.
- Printed pp. 251–255: three-, two-, and one-count lunch boxes.

Some of these patterns may be representable with current nodes and edges; those without negative zip/gap arrows or other missing action types should be handled in follow-up batches. I have not marked them unrepresentable merely because they are not in this first batch. Printed p. 255 begins the four-person lunch boxes section; the next page's continuation is outside this requested span.
