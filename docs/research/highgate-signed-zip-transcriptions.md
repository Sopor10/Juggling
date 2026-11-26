# Highgate signed zip/gap audit checkpoint

Sources checked: Aidan Burns, [The New Highgate Collection (2017 expanded PDF)](https://jugglingedge.com/pdf/highgate.pdf) and the 2014 edition bundled at `Siteswaps.Mcp.Server/Resources/highgate.pdf`. Printed and PDF page numbers are distinguished below.

This separate transcription batch contains source-checked graphs for *Pass self zip* and *The Recipe*. The first is already in the central catalog. The Recipe record remains separate and does not edit the catalog or inventory.

| Pattern | Source | Source-verified fact | Disposition |
|---|---|---|---|
| Four four one | 2017 printed 22 / PDF 23; 2014 PDF 8 | Ben's sequence is `pass heff double zip self self`; the zip points one beat backward. | Transcribed: B-R1→B-L3 at offset −1, with positive source height 1. |
| Four four four zero | 2017 printed 23 / PDF 24; 2014 PDF 9 | Ben's sequence is `pass heff double heff gap self`; the gap points two beats backward. | Transcribed: B-L1→B-L3 at offset −1, with explicit source height 0 for the empty-hand gap. |
| A self triple | 2017 printed 24 / PDF 25; 2014 PDF 10 | Ben uses `pass self triple-self pass zip self`; triple-self crosses hands. | Transcribed: B-L2→B-R1 at offset +1 spans three beats; zip is B-L3→B-R3 at −1 beat. |
| Double triple zip (561) | 2017 printed 28 / PDF 29; 2014 PDF 14 | The source describes an entry throw, two repeats of excited state 561, then an exit. | True finite-transition limit; repeating-only cycle would alter the trick. |
| Double zap zip (531) | 2017 printed 31 / PDF 32; 2014 PDF 17 | Ordinary repeating pattern, with a double pass, zap, and zip. | Transcribed as a six-beat cycle: double-pass routes span 2.5 beats, straight zaps 0.5 beat, and zip arrows point back one beat at positive source height 1. |
| Double zip (51) | 2017 printed 31 / PDF 32; 2014 PDF 17 | Entry 4, two repeats of excited 51, and exit 41. | True finite-transition limit. |
| The Recipe | 2017 printed 32–33 / PDF 33–34; 2014 PDF 18–19 | Derived sequence `5340-222`; the source gives first-period arrows 1→6, 2→5, 3→7, 4→4 (optional), 5→3, 6→8, 7→9. | Transcribed below in the separate JSON chunk. |
| 58552 | 2014 PDF 25 only | Siteswap 58552; Prechac `2.5p2.5p142.5p`; Anne starts with two clubs in right and one in left; Ben starts half a beat later. | Transcribed as a 20-edge, 10-beat cycle. No corresponding 2017 body diagram located. |

## The Recipe graph

The recipe is a repeating graph, not a finite transition. Its seven-token derived siteswap is expanded twice across the 14 alternating half-beat nodes for Anne and Ben. Each edge follows the numbered instructions in the 2017 source; the 2014 diagram and construction text on p. 18 match. The optional 4→4 self-loop is retained. The −2 derived value is an edge back to an earlier node in the same cycle, represented with an explicit nonnegative display height because the editor's height field is constrained to nonnegative values.

## True display/model limits kept separate

- *Simultaneous heffs*, *Techno (seven-club three-count on singles)*, *Scratch your head*, *Oddz godz*, and synchronous Brolly variants contain multiple same-time events for one juggler. The editor can store distinct nodes, but the current SVG lane and table projection overlap or collapse them.
- *Double triple zip (561)* and *Double zip (51)* are finite entry/excited-repeat/exit diagrams; the repeating-only model cannot preserve their finite transition structure.
- Signed offsets and explicit heights remove the old backward-arrow storage limitation. The ordinary periodic candidates above should not remain classified as model-blocked on that basis; several still need complete graph transcription.

The NUnit test `HighgateSignedZipStandaloneRecordValidationTests` validates each graph record using `IsValidForEditor()`, checks signed offsets survive `SynchronizeHeights()`, requires exactly one outgoing edge per throw, and asserts the Recipe's 14 edges, 58552's 20 edges, and the early zip/gap endpoints above. The finite-transition cases remain 561 and 51.
