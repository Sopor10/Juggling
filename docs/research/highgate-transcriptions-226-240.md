# Highgate transcription audit: 2017 printed pp. 224–240

Sources: Aidan Burns, *The Highgate Collection*, 2017 expanded edition ([PDF](https://jugglingedge.com/pdf/highgate.pdf)); compared with the 2014 edition bundled at `Siteswaps.Mcp.Server/Resources/highgate.pdf` and published [here](https://passing.zone/wp-content/uploads/highgate.pdf). The 2017 printed page number is one less than its PDF page number throughout this span (for example, printed p. 224 is PDF p. 225). The initial chunk covered pp. 226–240; the audit now also includes the adjacent four-person feed pages 224–225.

## Standalone causal graphs in this span and adjacent pages

The records below are transcribed as one causal event per listed throw. I checked the sequence rows, starts, and page labels against the 2017 PDF. I also checked the corresponding patterns in the 2014 PDF at pp. 157–163, 164–166, and 168. Their throw sequences and phase starts agree. These 24 ordinary, non-manipulation patterns are structurally valid for the causal editor; the focused test checks editor validity, resolvable endpoints, source timing, and that `CycleOffset` values survive height synchronization.

| Stable ID | Pattern | 2017 printed page | 2014 PDF page |
|---|---|---:|---:|
| `highgate-fourpersontwocountfeed` | Four person two count feed | 224 | 157 |
| `highgate-fourpersonpasspassselffeed` | Four person pass pass self feed | 224 | 157 |
| `highgate-fourpersononecountfeedingfourcount` | Four person one count feeding four count | 224 | 158 |
| `highgate-fourpersononecountfeedingthreecount` | Four person one count feeding three count | 224 | 158 |
| `highgate-fourpersononecountfeedingtwocount` | Four person one count feeding two count | 225 | 158 |
| `highgate-fourpersononecountfeedingpasspassself` | Four person one count feeding pass pass self | 226 | 159 |
| `highgate-fourpersononecountfeedingchocolatebar` | Four person one count feeding chocolate bar | 226 | 159 |
| `highgate-fourpersononecountfeedingbookends` | Four person one count feeding bookends | 226 | 159 |
| `highgate-fourpersononecountfeedinginvertedbookends` | Four person one count feeding inverted bookends | 227 | 160 |
| `highgate-fourpersononecountfeedingcountdown` | Four person one count feeding countdown | 227 | 160 |
| `highgate-fourpersoncivilwarfeed` | Four person civil war feed | 227 | 160 |
| `highgate-fourpersonparsnipfeedingfivecountpopcorn` | Four person parsnip feeding five count popcorn | 229 | 161 |
| `highgate-fourpersonfunkybookendsfeedingfiveclubwhynot` | Four person funky bookends feeding five club why not | 229–230 | 161–162 |
| `highgate-fourpersonmartinsonecountfeedingwhynot` | Four person Martin’s one count feeding Why Not? | 230 | 162 |
| `highgate-fourpersonmartinsonecountfeedingmaybe` | Four person Martin’s one count feeding Maybe | 230–231 | 162–163 |
| `highgate-fourpersononecountfeedingfiveclubwhynot` | Four person one count feeding five club Why Not? | 231–232 | 163 |
| `highgate-fourpersononecountfeedingparsnip` | Four person one count feeding Parsnip | 232 | 163 |
| `highgate-mixedcountfeed` | Mixed count feed | 234 | 164 |
| `highgate-passpassselfbox` | Pass pass self box | 235 | 165 |
| `highgate-chocolatesquare` | Chocolate square | 235 | 165 |
| `highgate-bookendsbox` | Bookends box | 236 | 166 |
| `highgate-invertedbookendsbox` | Inverted bookends box | 236 | 166 |
| `highgate-twocountlinefeed` | Two count line feed | 240 | 168 |
| `highgate-urbanterrorfeedonecountlinefeed` | Urban terror feed (one count line feed) | 240 | 168 |

The inventory CSV currently omits 2014 page values for several patterns whose sequences are present in the 2014 PDF, and some of its 2017 page references do not match the printed body pages. This audit preserves the direct PDF evidence; it does not change the inventory.

## Reviewed exceptions and limits

The following source graphs remain untranscribed because their timing or throw semantics need a dedicated transcription pass. I did not infer edges from the sequence alone.

- **Four person mild madness feed** (`highgate-fourpersonmildmadnessfeed`) and **Four person Rachel’s feed** (`highgate-fourpersonrachelsfeed`) (p. 228): staggered half-beat starts; hold and zip notation; Rachel’s diagram shows only half the repeating hand phase.
- **Box patterns** (pp. 235–238): `highgate-mildmadnessbox`, `highgate-whynotbox`, `highgate-maybebox`, and `highgate-972box` use zip/heff timing. The 972 box also has alternating double/single passes, nonuniform start phases, and initial-hand exceptions.

All six separately titled period-five feeds shown on pp. 229–233 are transcribed, including their zip/heff actions and half-beat starts. The source says the list is non-exhaustive and describes interchangeable variants without printing separate diagrams; those unillustrated combinations are not separate source-backed graphs here.

The period-five graphs have ten-beat cycles with ten alternating-hand events per person. Ben and Daniel start at beat 0; Anne and Clare start half a beat later. Named passes target the receiver’s next listed event at or after source time + 1; self, zip, and heff actions target the same juggler’s next, previous, and second-next event. Each destination was checked against the 2017 arrow diagram and the corresponding 2014 diagram. For the adjacent pp. 224–225 base feeds, periods of three repeat twice so hands return to the same phase; even periods repeat once. Their named passes and self throws target the receiver or thrower’s next listed event. The “Line Feeds” section heading, `Two count line feed`, and `Urban terror feed (one count line feed)` are all on printed p. 240. Their inventory PDF-page field is inconsistent with the body page and footer, so this standalone source reference follows the PDF page image/text directly. No central catalog or inventory entries were changed.
