# Highgate transcription batch: 2017 printed pages 76–90

This is a first small, visually reviewed import batch for the 2017 expanded Highgate PDF. Pattern data is kept in [highgate-transcriptions-76-90.json](highgate-transcriptions-76-90.json); it has not been added to the central catalog by this batch.

## Batch contents and sources

| Stable/catalog ID | Pattern | 2017 source | 2014 comparison | State size |
|---|---|---|---|---|
| `highgate-jimsfizzbuzzasynchronousversion` | Jim's fizz buzz (asynchronous version) | printed p. 76, PDF p. 77 | PDF p. 47 has Fizz buzz and Jim's synchronous version; not this asynchronous pattern | 60 nodes, 48 edges |
| `highgate-jimsbookendsakabrainstormasynchronousversion` | Jim's bookends aka brainstorm (asynchronous version) | printed p. 76, PDF p. 77 | No exact counterpart found; 2014 Funky Bookends is a different pattern | 52 nodes, 40 edges |
| `highgate-sevenclubonecountonsingles` | Seven club one count on singles | printed p. 80, PDF p. 81 | PDF p. 16 has related seven-club one-count material, not the exact singles-only variant | 4 nodes, 4 edges |

The source is Aidan Burns, [expanded Highgate PDF (2017 file)](https://jugglingedge.com/pdf/highgate.pdf). The locally bundled 2014 PDF is the same file served at [passing.zone](https://passing.zone/wp-content/uploads/highgate.pdf). The printed-page/PDF-page offset is recorded per record in the JSON.

## Review and transcription method

I rendered and visually checked the body pages covering printed pp. 76–90 (PDF pp. 77–91). This batch contains only three selected patterns, not every pattern on those pages. For each included item, I checked its title, printed notation or sequence, hand order, pass direction, and every arrow in the source diagram against its graph. Holds in the two asynchronous patterns are represented as explicit nodes without outgoing edges, matching the source's “hold” tokens. The visual review found no third-person manipulation in these three ordinary two-person patterns.

The fizz-buzz diagram shows one half of each pattern and says it repeats with the other hand; the graph therefore repeats each 15-token row with opposite-hand events to make a 30-beat cycle. The asynchronous brainstorm similarly doubles its 13-token rows to preserve alternating hands. Ben's half-beat phase is present in both patterns. The seven-club singles diagram has a two-beat repeating graph with Anne at integer beats and Ben offset by half a beat. The selected patterns use no unsupported negative zip/gap arrows.

The first draft cited “Seven club one count on singles” at printed p. 79. Visual page review caught that error: printed p. 79 is “Pass pass self vs one count”; the seven-club section starts at printed p. 80. Both the JSON source citation and this note use the corrected page.

## Validation

A temporary validator constructed a `CausalDiagramEditorPreset` from each JSON record, called `Create()`, called `SynchronizeHeights()`, and compared the serialized cycle offsets before and after. Results:

- Jim's fizz buzz async: `Create()` succeeded; 60 nodes / 48 edges; offsets unchanged.
- Jim's bookends/brainstorm async: `Create()` succeeded; 52 nodes / 40 edges; offsets unchanged.
- Seven club one count on singles: `Create()` succeeded; 4 nodes / 4 edges; offsets unchanged.

`IsValidForEditor()` returns false on each raw record's new ID because the ID is not yet in the central known-preset catalog. To check the graph invariants independently, the validator temporarily assigned the existing registered ID `highgate-three-count`; all three graph states then returned true. After central import, rerun the catalog's actual `Create()` / `IsValidForEditor()` checks with the intended IDs. No central files were changed in this batch.

## Known scope and limitations

This is a checked first batch, not completion of printed pp. 76–90. Other patterns on the reviewed pages remain for later batches. The editor graph stores causal nodes and edges; it does not store the club-count starting inventory or the book's color-coded club identity. Those annotations are therefore not reproduced in these records. No included pattern required a model extension.
