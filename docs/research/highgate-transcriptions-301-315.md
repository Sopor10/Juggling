# Highgate transcription batch: 2017 printed pages 301–315

Work file: [highgate-transcriptions-301-315.json](highgate-transcriptions-301-315.json). The first accepted graph is `Thirteen club four count box`; the other candidates below remain open and are not implied complete by this batch.

## Coverage and source mapping

The 2017 expanded PDF body pages are PDF pp. 302–316 (printed pp. 301–315). The full text layer has been extracted for that range and the diagrams are present as raster/vector artwork. The 2014 edition is available at [passing.zone/wp-content/uploads/highgate.pdf](https://passing.zone/wp-content/uploads/highgate.pdf); the 2017 expanded edition is [jugglingedge.com/pdf/highgate.pdf](https://jugglingedge.com/pdf/highgate.pdf). The bundled 2014 copy is the same edition noted in the inventory. I have not completed the page-by-page 2014 visual comparison, so no 2014 equivalence is asserted below.

The section contains these named patterns or named pattern variants (one-count countdown's continuation and the headings on adjoining pages are retained as separate candidates where the book names them separately):

| Printed page | Pattern | Inventory match / review note |
|---|---|---|
| 301 | Thirteen club one count feeding countdown | `highgate-thirteenclubonecountfeedingcountdown305thirteenc-9dcb228c44` (inventory title also contains next title due a catalog-heading merge) |
| 301 | Thirteen club pass pass self feeding French three count | `highgate-thirteenclubpasspassselffeedingfrenchthreecount` |
| 301 | Thirteen club pass pass heff feeding three count | `highgate-thirteenclubpasspasshefffeedingthreecount` |
| 302–303 | Thirteen club 972 feeding three count | inventory family `highgate-972feeds4people13clubs`; individual title `highgate-thirteenclub972feedingthreecount` |
| 303 | Thirteen club 972 feeding French three count | `highgate-thirteenclub972feedingfrenchthreecount` |
| 303 | One count feeding 972 | inventory candidate needs exact-title ID reconciliation |
| 303–304 | Thirteen club why not? feeding five count popcorn | `highgate-thirteenclubwhynotfeedingfivecountpopcorn` |
| 304 | Thirteen club maybe feeding five count popcorn | `highgate-thirteenclubmaybefeedingfivecountpopcorn` |
| 305 | Thirteen club funky bookends feeding maybe | `highgate-thirteenclubfunkybookendsfeedingmaybe` |
| 305–306 | Thirteen club Martin's one count feeding five count popcorn | `highgate-thirteenclubmartinsonecountfeedingfivecountpopcorn` |
| 306–307 | Thirteen club Martin's one count feeding funky bookends | `highgate-thirteenclubmartinsonecountfeedingfunkybookends` |
| 307 | Thirteen club one count feeding why not? | `highgate-thirteenclubonecountfeedingwhynot` |
| 307–308 | Thirteen club one count feeding maybe | `highgate-thirteenclubonecountfeedingmaybe` |
| 308–309 | Thirteen club one count feeding Martin's one count | `highgate-thirteenclubonecountfeedingmartinsonecount` |
| 310 | Thirteen club four count box | `highgate-thirteenclubfourcountbox` |
| 310 | Thirteen club three count box | `highgate-thirteenclubthreecountbox` |
| 310–311 | Thirteen club three count box variation | `highgate-thirteenclubthreecountboxvariation` |
| 311 | Thirteen club two count box | `highgate-thirteenclubtwocountbox` |
| 311–312 | Thirteen club chocolate square | `highgate-thirteenclubchocolatesquare` |
| 312 | Weird pass pass self box | `highgate-weirdpasspassselfbox` |
| 313 | Thirteen club two count line feed | `highgate-thirteenclubtwocountlinefeed` |
| 313 | Thirteen club urban terror feed (one count line feed) | `highgate-thirteencluburbanterrorfeedonecountlinefeed` |
| 314 | Thirteen club one count line feed, feeding four count and two count | title/ID reconciliation needed against inventory's combined line-feed candidate |
| 314 | Thirteen club one count line feed, feeding French three count | `highgate-thirteenclubonecountlinefeedfeedingfrenchthreecount` |
| 314–315 | Thirteen club parsnip line feed, feeding five count popcorn | `highgate-thirteenclubparsniplinefeedfeedingfivecountpopcorn` |

This roster follows body headings and the extracted pattern descriptions; it is not yet a completed catalog-ID audit. In particular, the inventory merges some adjacent headings into a single candidate row, so those IDs need to be split or mapped before import.

## Accepted first graph

`Thirteen club four count box` is transcribed as 32 event nodes and 8 pass edges over an 8-beat cycle. The 2017 diagram on printed p. 310 and its four sequences were checked; each single/triple pass is routed to the named recipient's landing event, including cycle wrap. The 2014 PDF p. 221 title and sequence text match; a full visual comparison of that edition's diagram remains open. The state retains all self-event nodes but cannot encode starting club inventories.

## Manipulation classification

The reviewed text and visible examples describe ordinary four-person passing, feeding, boxes, and line feeds. I found no external manipulator role, takeout, hijack, or lo-jack description in the reviewed material. This is a negative finding from this section only; it does not imply that feeding or movement patterns elsewhere in Highgate cannot be manipulative.

## Model questions before graph acceptance

- The pages use `zip` and `heff` as explicit sequence tokens, including on pp. 301–303, 307–309, and 314–315. The current record schema has pass/self/hold nodes and causal edges but no explicit action type for these tokens. They must not be silently converted to empty holds or ordinary passes.
- Several patterns have half-beat or one-and-a-half-beat phase differences and different individual sequence periods. Decimal start times are supported, but each full cycle and hand alternation must be proven from the diagram before a state is accepted.
- Some sequences specify double or triple passes. Their causal arrival node and cycle offset must be read from the drawn arrows; a label alone is insufficient to invent edge endpoints.
- The editor has no spatial formation, crossing/straight path, club-count/color coding, or club inventory fields. These can be preserved as explanatory notes only if the causal graph itself is otherwise exact.
- Some starts state clubs in each hand; these are initial inventories, not throw-node events, and the current state schema cannot encode them.

## Validation status

The temporary validator built the preset and called `Create()`, then `SynchronizeHeights()` and compared serialized offsets. The four-count box created successfully with 32 nodes and 8 edges; offsets were unchanged; `IsValidForEditor()` passed after substituting registered placeholder ID `highgate-three-count`. The actual new ID is not registered centrally yet, so rerun with the intended ID after import. No central preset, inventory, or test file was changed.

The remaining candidates have not yet been transcribed, compared visually with both editions, or validated. The chunk is a verified partial batch, not a claim of completeness for pp. 301–315. A follow-up should include each stable inventory ID, named aliases as separate records even when graphs match, direct state validation and offset checks, and source-specific model limits.
