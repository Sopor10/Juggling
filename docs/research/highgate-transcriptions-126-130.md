# Highgate transcription batch: 2017 printed pages 126–130

This separate batch contains four independent, non-manipulation period-five feeds (120 event nodes and 120 causal edges). The four transcribed patterns are imported into the runtime causal-editor catalog and marked as translated in the inventory; the JSON remains the independent source transcription. Body-page locations below use the printed page number shown in the PDF footer; the 2017 expanded PDF page is one higher.

## Included records and source comparisons

| Stable ID | Pattern | 2017 source | Matching 2014 source | Cycle |
|---|---|---|---|---:|
| `highgate-martinsonecountfeedingmaybe` | Martin's one count feeding Maybe | [PDF 128](https://jugglingedge.com/pdf/highgate.pdf#page=128), printed 127 | [PDF 84](https://passing.zone/wp-content/uploads/highgate.pdf#page=84), printed 84 | 10 |
| `highgate-martinsonecountfeedingfiveclubwhynotandfunkybookends` | Martin's one count feeding five club Why Not? and funky bookends | [PDF 129](https://jugglingedge.com/pdf/highgate.pdf#page=129), printed 128 | [PDF 85](https://passing.zone/wp-content/uploads/highgate.pdf#page=85), printed 85 | 10 |
| `highgate-onecountfeedingwhynotandparsnip` | One count feeding Why Not? and parsnip | [PDF 129](https://jugglingedge.com/pdf/highgate.pdf#page=129), printed 128 | [PDF 85](https://passing.zone/wp-content/uploads/highgate.pdf#page=85), printed 85 | 10 |
| `highgate-onecountfeedingfiveclubwhynotandmartinsonecount` | One count feeding five club Why Not? and Martin's one count | [PDF 129–130](https://jugglingedge.com/pdf/highgate.pdf#page=129), printed 128–129 | [PDF 85](https://passing.zone/wp-content/uploads/highgate.pdf#page=85), printed 85 | 10 |

## Transcription and validity audit

I visually compared the 2017 body diagrams and their action rows with the 2014 diagrams. The 2014 pages reproduce each included title, siteswap, start phase, throw sequence, and endpoint arrangement. Each lane has a five-action period, repeated twice so the ten-beat graph preserves alternating R/L hands. Ben's events begin on integer beats; Anne and Clare begin half a beat later, as specified in the source. The start-hand descriptions change club counts but do not change that R/L event order.

Every named pass targets the named receiver's event 1.5 beats after the source event, giving height 3.5; the drawn arrows confirm the receiver. `self` targets the next event on that lane (height 3), `heff` the second-next event (height 4), and `zip` the prior event on that lane (height 1). These produce one outgoing edge per throw and one incoming edge per event. Every cycle offset was chosen to preserve the source height using `target time + cycleOffset × 10 - source time + 2 = height`. The explicit event heights retain the source throw meaning even where source diagram columns are layout positions.

`Highgate126To130StandaloneRecordValidationTests` permanently constructs all four records as `CausalDiagramEditorState`, calls `SynchronizeHeights()`, checks that cycle offsets are unchanged, and requires `IsValidForEditor()` for every record. The focused test passes. The catalog-wide validity test and reviewed-ID allowlist also cover all four imported patterns.

## Excluded material and scope

- Printed p. 126 / PDF p. 127 contains only the tail of the already transcribed “Martin's one count feeding Why Not?” from printed p. 125; I did not create a duplicate.
- “Martin's one count feeding maybe and why not?” and “Martin's one count feeding five count popcorn and parsnip” were excluded because the existing pattern inventory classifies them as manipulation cases. Their diagrams are on printed pp. 127–128 / PDF pp. 128–129.
- The “Hijacking in period five feeds” discussion begins on printed p. 130 / PDF p. 131. It is transition/manipulation guidance, not an independent static pattern. Entries whose body diagrams are outside pp. 126–130 were not included even if an index or contents entry falls in the range.
- No included graph has an uncertain endpoint. All four included patterns have a matching 2014 body diagram on pp. 84–85.
