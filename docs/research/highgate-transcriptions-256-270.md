# Highgate transcription audit: 2017 printed pp. 256–270

Sources: Aidan Burns, *The New Highgate Collection* (2017 expanded edition, [PDF](https://jugglingedge.com/pdf/highgate.pdf)); compared with *The Highgate Collection* (2014, [PDF](https://passing.zone/wp-content/uploads/highgate.pdf), also bundled in the repository). For the 2017 file in this chunk, the PDF page number is one greater than the printed footer.

I checked actual body pages and their printed footers rather than trusting the index. This matters here: several index references drift. Aidan's feasts are printed pp. 252–254 and Four Person Lunch Boxes pp. 254–255, so they fall outside this requested body range even though their index entries point later. The Rick's picnic diagrams are printed pp. 256–257 and are in range.

## Standalone causal graph records

The separate [JSON data](highgate-transcriptions-256-270.json) currently holds 23 records. Sequences, phases, and causal arrows were checked against the corresponding diagram pages.

| Stable ID | Pattern | 2017 printed page | 2014 PDF page | Graph |
|---|---|---:|---:|---|
| `highgate-fourpersonfourcountdropbackline1` | Four person four count drop-back line #1 | 257 | 182 | 16 events / 16 edges |
| `highgate-fourcounty` | Four count y | 258 | 183 | 16 / 16 |
| `highgate-fourpersonfourcountdropbackline2` | Four person four count drop-back line #2 | 258 | 183 | same throw-target graph as Four count y |
| `highgate-fourcountdropbacky` | Four count drop-back y | 259 | 184 | 16 / 16 |
| `highgate-threecounty` | Three count y | 259 | 184 | 24 / 24 |
| `highgate-fourcounttrapeze` | Four count trapeze | 261 | 185 | 32 / 32 |
| `highgate-alloutcivilwarfeed` | All out civil war feed | 261 | 185 | 48 / 48 |
| `highgate-bookendsinvertedbookendsbox` | Bookends / inverted bookends box | 262 | 186 | 40 / 40 |
| `highgate-doubleurbanterrorfeed` | Double urban terror feed | 262 | 186 | 24 / 24 |
| `highgate-threecountaccommodation` | Three count accommodation | 264 | 187 | 24 / 24 |
| `highgate-lowfatchocolatebox` | Low fat chocolate box | 264 | 187 | 48 / 48 |
| `highgate-bookshelf` | Bookshelf | 264 | 187 | 120 / 120 |
| `highgate-chocolatebookshelf` | Chocolate bookshelf | 265 | 188 | 80 / 80 |
| `highgate-countdownaccommodation1` | Countdown accommodation #1 | 265 | 188 | 32 / 32 |
| `highgate-countdownaccommodation2` | Countdown accommodation #2 | 265 | 188 | 48 / 48 |
| `highgate-jimsthreecounty` | Jim's three count y | 259–260 | 184 | 56 nodes / 48 edges; half-beat lane phase and holds |
| `highgate-rowrowrowyourboat` | Row, row, row your boat | 267–268 | 189 | 128 / 128 |
| `highgate-rowrowrowyourboatambidextrousvariation` | Row, row, row your boat (ambidextrous variation) | 269 | 190 | 64 / 64 |
| `highgate-frerejacques` | Frere Jacques | 270 | 191 | 128 / 128; same causal graph as Row, row, row your boat |
| `highgate-fourpersonthreecountrickspicnic` | Four person three count Rick's picnic | 256 | 181 | 96 / 96; hurried hand repetitions preserved |
| `highgate-fourpersontwocountrickspicnic` | Four person two count Rick's picnic | 256 | 181 | 64 / 64; hurried hand repetitions preserved |
| `highgate-fourpersononecountrickspicnic` | Four person one count Rick's picnic | 257 | 182 | 32 / 32; hurried hand repetitions preserved |
| `highgate-funkybookendsparsnipbox` | Funky bookends / parsnip box | 262 | 186 | 40 / 40; phase offsets, heffs, and zip edges preserved |

Odd-length source periods are doubled so each editor cycle returns to the same hand assignment. Thus the five-event Bookends / inverted bookends box has a ten-beat cycle, and the 15-event Bookshelf half-period is repeated with opposite hands in a 30-beat cycle. Jim's three-count y preserves the half-beat offset between Anne/Clare and Ben/Daniel; hold positions remain event nodes without outgoing edges. The song diagrams rotate four line-specific sequences across four juggler rows; “opposite”, “left”, and “right” destinations were read from the source graph.

The four-count drop-back line #2 has its own named ID even though its throw-target graph matches Four count y. Its formation and over-the-head route are retained in notation because the editor does not encode spatial positions or pass style. Club colors, formations, straight/crossing style, and walking routes likewise remain source notation where the editor has no property for them.

## Validity test

`Highgate256To270StandaloneRecordValidationTests` reads all 23 records, verifies the complete expected ID set and resolvable edge endpoints, builds an editor state for each, calls `SynchronizeHeights()`, checks that all cycle offsets remain unchanged, and requires `IsValidForEditor()`.

## Scope notes

All individual causal diagrams found on actual printed pp. 256–270 are represented in the 23-record chunk. The “Generalised y pattern” passage (p. 261) is a construction method, not an individual pattern. Four-person lunch boxes and Aidan’s feasts are outside printed pp. 256–270 based on their actual body pages, despite misleading index entries. No central catalog or inventory files are changed by this audit chunk.
