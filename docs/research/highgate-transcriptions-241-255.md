# Highgate transcription batch: 2017 printed pages 241–255

The source records for this chunk are in [highgate-transcriptions-241-255.json](highgate-transcriptions-241-255.json). The page-range review is complete for the 23 included four-person feeds and feasts. The remaining candidates are explicitly listed below; none was added because its graph or timing remains ambiguous, changes pattern state over time, or includes hurried throws whose timing or endpoints are unresolved.

## Included patterns

| Inventory/catalog ID | Pattern | 2017 body page | 2014 body page | State |
|---|---|---|---|---|
| `highgate-fourpersonfourcountfeast` | Four person four count feast | printed p. 245 / PDF p. 246 | PDF p. 173 | 64 nodes / 64 edges |
| `highgate-bookendslinefeed` | Bookends line feed | printed p. 241 / PDF p. 242 | PDF p. 169 | 40 / 40 |
| `highgate-mobmildnessmadness` | Mob mildness / madness | printed p. 241 / PDF p. 242 | PDF p. 169 | 72 / 63 |
| `highgate-mobneurosispsychosis` | Mob neurosis / psychosis | printed p. 242 / PDF p. 243 | PDF p. 170 | 120 / 96 |
| `highgate-parsniplinefeed` | Parsnip line feed | printed p. 242 / PDF p. 243 | PDF p. 170 | 40 / 40 |
| `highgate-funkybookendslinefeed` | Funky bookends line feed | printed p. 243 / PDF p. 244 | PDF p. 171 | 40 / 40 |
| `highgate-onecountfeedingfiveclubwhynotandfunkybookends` | One count feeding five-club Why Not? and Funky Bookends | printed p. 243 / PDF p. 244 | PDF p. 171 | 40 / 40 |
| `highgate-onecountfeedingwhynotandfiveclubwhynot` | One count feeding Why Not? and five-club Why Not? | printed p. 244 / PDF p. 245 | PDF p. 172 | 40 / 40 |
| `highgate-onecountfeedingmaybeandfiveclubwhynot` | One count feeding Maybe and five-club Why Not? | printed p. 244 / PDF p. 245 | PDF p. 172 | 40 / 40 |
| `highgate-fourpersonthreecountfeast` | Four person three count feast | printed p. 245 / PDF p. 246 | PDF p. 173 | 48 / 48 |
| `highgate-fourpersontwocountfeast` | Four person two count feast | printed p. 245 / PDF p. 246 | PDF p. 173 | 32 / 32 |
| `highgate-fourpersononecountfeast` | Four person one count feast | printed p. 246 / PDF p. 247 | PDF p. 174 | 16 / 16 |
| `highgate-fourpersonpasspassselffeast` | Four person pass pass self feast | printed p. 247 / PDF p. 248 | PDF p. 174 | 24 / 24 |
| `highgate-fourpersonbookendsfeast` | Four person bookends feast | printed pp. 247–248 / PDF pp. 248–249 | PDF p. 174 | 80 / 80 |
| `highgate-fourpersontwocountrevolvingfeed` | Four person two count revolving feed | printed p. 248 / PDF p. 249 | PDF p. 175 | 64 / 64 |
| `highgate-fourpersononecountrevolvingfeed` | Four person one count revolving feed | printed p. 248 / PDF p. 249 | PDF p. 175 | 32 / 32 |
| `highgate-fourpersonquasar` | Four person quasar | printed p. 248 / PDF p. 249 | PDF p. 175 | 96 / 96 |
| `highgate-fourpersonfizzbuzzfeast` | Four person fizz buzz feast | printed p. 250 / PDF p. 251 | PDF p. 176 | 96 / 96 |
| `highgate-fourpersonselflessfizzbuzz` | Four person selfless fizz buzz | printed p. 251 / PDF p. 252 | PDF p. 177 | 48 / 48 |
| `highgate-aidansfeast1` | Aidan’s feast #1 | printed p. 251 / PDF p. 252 | PDF p. 177 | 96 / 96 |
| `highgate-aidansfeast2` | Aidan’s feast #2 | printed p. 253 / PDF p. 254 | PDF p. 178 | 48 / 48 |
| `highgate-aidansfeast3` | Aidan’s feast #3 | printed p. 253 / PDF p. 254 | PDF p. 178 | 24 / 24 |
| `highgate-aidansfeast4` | Aidan’s feast #4 | printed p. 254 / PDF p. 255 | PDF p. 179 | 48 / 48 |

The first four IDs already exist in the central causal preset list; I checked their stored events and edges against these standalone records. The other 19 IDs are source records pending central import. Sources are [the 2017 expanded PDF](https://jugglingedge.com/pdf/highgate.pdf) and [the 2014 PDF](https://passing.zone/wp-content/uploads/highgate.pdf), also available locally as `Siteswaps.Mcp.Server/Resources/highgate.pdf`.

## Transcription and source review

I rendered all requested source pages. For each included pattern, I checked the row sequence against the causal diagram: each pass routes to the named person, each `self` stays on the same row, hands alternate, and the last edge wraps to the next cycle. Bookends line feed repeats its five-event row twice to preserve alternating hands; the feast and feed periods otherwise match the displayed full pattern cycles. The 2014 diagrams and row sequences were compared at the corresponding PDF pages; titles, sequences, initial hand inventories, and causal edges match.

The source gives the initial inventory as two clubs in the right hand and one in the left hand for each juggler. That inventory, plus the 2014 three-count feast's red/white color key, is retained in the record notes; the editor has no club-inventory or color fields.

## State validation

The focused NUnit test constructs an editor state for every JSON record, checks unique record and throw IDs plus resolvable edge endpoints and a single outgoing edge per source throw, runs `SynchronizeHeights()`, verifies every `CycleOffset` is preserved, and requires `IsValidForEditor()`. All 23 records pass. The test uses a registered baseline preset ID because the 19 new records are not yet in the central preset list. The four earlier records are byte-for-byte aligned with their existing central event and edge data. For `highgate-onecountfeedingwhynotandfiveclubwhynot`, `C-R10` has explicit height 1: the source lists the final handoff as a zip, while the normalized cyclic diagram edge points to the preceding displayed event and otherwise derives a negative height. This preserves the source arrow and gives the throw a positive physical height. No central runtime or inventory files were changed.

## Remaining material in this page range

The following source patterns remain unresolved or outside this printed-page range:

- Printed p. 251 / PDF p. 252: Four person countdown feast. The four printed action rows have lengths 30, 30, 29, and 30, but the diagram shows a 30-event cycle for each person. The matching 2014 page (PDF p. 176) repeats the same Clare-row omission, so the absent action cannot be resolved by edition comparison. The graph remains open rather than inventing the missing throw.
- Printed p. 253: Extended Aidan’s feast #1 switches between four-count and three-count every twelve beats. This finite transition needs explicit prefix/loop/suffix support before it can be represented faithfully.
- Printed pp. 254–255: Four-person three-, two-, and one-count lunch boxes. The source labels several throws “hurried”. Crossing/straight trajectory is only spatial notation, but the hurried marker’s exact endpoint/timing has not been resolved from the arrow drawings. This is a transcription question, not yet an established editor-model blocker; the graph data should not guess those endpoints.
- Printed pp. 256–257: Four-person three- and two-count Rick’s picnics. These fall outside the requested printed pp. 241–255 range even though the inventory currently associates them with this page range; keep them for the following chunk, and correct their source-page references during inventory reconciliation.

The 2017 body pages and corresponding 2014 pages were rendered or text-extracted and compared for the included records. The current inventory CSV often records the index page instead of the body-page number; the body page references in this chunk are authoritative for those translations. The four-person feasts, lunch boxes, and line feeds labels are section headings, not independent pattern records. Mob mildness/madness and mob neurosis/psychosis are each one diagram with a named variant; no manipulation pattern occurs on these pages. This chunk does not claim pp. 241–255 are complete.
