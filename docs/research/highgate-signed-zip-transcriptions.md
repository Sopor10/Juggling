# Highgate signed zip/gap audit checkpoint

Source checked: Aidan Burns, [The New Highgate Collection (2017 expanded PDF)](https://jugglingedge.com/pdf/highgate.pdf). Page numbers below distinguish printed page from PDF page (PDF includes cover/front matter).

This is a partial transcription batch. `highgate-signed-zip-transcriptions.json` contains a source-checked *Pass self zip* graph. Other candidate rows below remain audit notes, not graph records. Signed offsets make backward arrows storable, but do not establish which event nodes those arrows connect.

The *Pass self zip* graph is imported into the central editor catalog as `highgate-passselfzip`, and its inventory row is marked translated. The remaining zip/gap candidates are still untranscribed.

| Pattern | Source | Confirmed by source text | Remaining work |
|---|---|---|---|
| Four four one | printed 22 / PDF 23 | Ben's sequence is `pass heff double zip self self`; zip edge goes from the right-hand beat to the left-hand beat, one beat backward. | Enumerate every unchanged and modified edge in the complete diagram. |
| Four four four zero | printed 23 / PDF 24 | Ben's sequence is `pass heff double heff gap self`; gap edge goes from the second left-hand beat to the first, two beats backward. | Enumerate every edge in the complete diagram. |
| A self triple | printed 24 / PDF 25 | Ben uses `pass self triple-self pass zip self`; the triple-self crosses hands. | Confirm every endpoint, including zip and overlapping incoming arrivals, from diagram. |
| Double triple zip (561) | printed 28 / PDF 29 | Entry, two repeats of the 561 excited section, then exit. | Requires finite prefix/repeat/suffix semantics; avoid encoding as one repeating loop. |
| Double zap zip (531) | printed 31 / PDF 32 | Ordinary repeating zap/zip pattern. | Read endpoints from diagram; preserve action meaning. |
| Double zip (51) | printed 31 / PDF 32 | Ordinary repeating pattern with backward zip. | Read endpoints from diagram. |
| Pass self zip | printed 40 / PDF 41 (text continues p.41 / PDF 42) | Siteswap 726 / Prechac 3.5p31; Ben and Anne both use `pass self zip`, with Anne starting 1.5 beats after Ben. Raster loops agree with the generated one-beat-back zip edges. | Complete: six events per juggler over the doubled six-beat cycle. |
| The Recipe | printed 32 / PDF 33 | Source identifies a zip arrow two beats backward. | Read all endpoints from diagram. |

## Separate model limitations

- Simultaneous events for one juggler (including *Simultaneous heffs* and synchronous recipe variants) can be represented as separate state nodes, but the current SVG lane and table projection overlap or collapse those nodes. Keep these separate from ordinary asynchronous zip transcription until the display model is addressed.
- *Double triple zip (561)* is a finite entry/excited-section/exit pattern, so a repeating-only preset would change its structure.

The NUnit test `HighgateSignedZipStandaloneRecordValidationTests` validates every graph record in this separate JSON file with `IsValidForEditor()`, checks that `SynchronizeHeights()` retains signed offsets, and requires exactly one outgoing edge per throw. It does not claim completeness for unresolved candidates; each future record still needs source-arrow review.
