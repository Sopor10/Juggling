# Highgate transcription audit: 2017 printed pp. 226–240

Source edition: Aidan Burns, *The Highgate Collection*, 2017 expanded edition, [PDF](https://jugglingedge.com/pdf/highgate.pdf). Printed pages 226–240 correspond to PDF pages 227–241. Compared against Aidan Burns, 2014 edition, [PDF](https://passing.zone/wp-content/uploads/highgate.pdf). The feeding patterns on 2017 pp. 226–228 match 2014 pp. 158–160; mixed count feed matches p. 164; pass-pass-self box and chocolate square p. 165; bookends/inverted bookends box p. 166; line feeds p. 168. Pattern sequences and labels match across these editions.

## Included, visually checked graphs

All records have stable inventory IDs, complete per-event edges, and explicit source pages in `highgate-transcriptions-226-240.json`.

| Stable ID | Pattern | 2017 printed page | 2014 printed page | State check |
|---|---|---:|---:|---|
| `highgate-fourpersononecountfeedingpasspassself` | Four person one count feeding pass pass self | 226 | 158 | valid; offsets preserved |
| `highgate-fourpersononecountfeedingchocolatebar` | Four person one count feeding chocolate bar | 226 | 158 | valid; offsets preserved |
| `highgate-fourpersononecountfeedingbookends` | Four person one count feeding bookends | 226–227 | 159 | valid; offsets preserved |
| `highgate-fourpersononecountfeedinginvertedbookends` | Four person one count feeding inverted bookends | 227 | 159 | valid; offsets preserved |
| `highgate-fourpersononecountfeedingcountdown` | Four person one count feeding countdown | 227 | 159 | valid; offsets preserved |
| `highgate-fourpersoncivilwarfeed` | Four person civil war feed | 227–228 | 160 | valid; offsets preserved |
| `highgate-mixedcountfeed` | Mixed count feed | 234 | 164 | valid; offsets preserved |
| `highgate-passpassselfbox` | Pass pass self box | 235 | 165 | valid; offsets preserved |
| `highgate-chocolatesquare` | Chocolate square | 235 | 165 | valid; offsets preserved |
| `highgate-bookendsbox` | Bookends box | 236 | 166 | valid; offsets preserved |
| `highgate-invertedbookendsbox` | Inverted bookends box | 236 | 166 | valid; offsets preserved |
| `highgate-twocountlinefeed` | Two count line feed | 240 | 168 | valid; offsets preserved |
| `highgate-urbanterrorfeedonecountlinefeed` | Urban terror feed (one count line feed) | 240 | 168 | valid; offsets preserved |

I read the printed throw rows and checked them against the diagram for each included pattern. Each row is expanded into one event per listed throw per juggler, with alternating right/left hands. For odd sequence lengths the sequence repeats twice to restore the hand phase. Causal edges follow each caught club to that recipient's next throw; the editor derives height 3 from this timing. All cycles have one event per beat per person. No source sequence contains a manipulator role: these are ordinary feeds and box/line feeds.

Validation used the component's `CausalDiagramEditorState`: `SynchronizeHeights()` followed by `IsValidForEditor()` succeeded for every record, and every edge `CycleOffset` survived unchanged. Since these new stable IDs have not been added to the central preset catalog, the direct state check temporarily used the already registered ID `highgate-three-count`; the data IDs themselves remain the requested inventory IDs. This limitation is recorded per JSON record.

## Classified but not transcribed in this chunk

The inspected block also contains these named patterns, none of which has a solo manipulator; these are ordinary four-person feeds:

- `Four person mild madness feed` and `Four person Rachel’s feed` (2017 pp. 228–229): staggered phases and zip/hold throws.
- Four-person period-five feed section and its listed variants, including parsnip/five-count popcorn, funky bookends/five-club why-not, and one-count/five-club why-not (2017 pp. 229–233): period-five phase arrangements and/or zip/hold semantics require source-specific modeling.
- `Mild madness box`, `Why not? box`, `Maybe box`, and `972 box` (2017 pp. 235–239): zip/heff/hold throws and staggered or hand-shifted starts. These cannot be represented faithfully by the simple one-event-per-beat graph used here.

These exclusions are model/timing limitations, not manipulations. The inventory contains other candidate titles in its wider catalog, but the classification here is limited to named patterns actually appearing in the inspected page range. No manipulation pattern was identified in this block.
