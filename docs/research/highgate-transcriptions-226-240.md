# Highgate transcription audit: 2017 printed pp. 226–240

Sources: Aidan Burns, *The Highgate Collection*, 2017 expanded edition ([PDF](https://jugglingedge.com/pdf/highgate.pdf)); compared with the 2014 edition bundled at `Siteswaps.Mcp.Server/Resources/highgate.pdf` and published [here](https://passing.zone/wp-content/uploads/highgate.pdf). The 2017 printed page number is one less than its PDF page number in this span (for example, printed p. 226 is PDF p. 227).

## Standalone causal graphs in this span

The records below are transcribed as one causal event per listed throw. I checked the sequence rows, starts, and page labels against the 2017 PDF. I also checked the corresponding patterns in the 2014 PDF at pp. 159–161, 164–166, and 168. Their throw sequences and phase starts agree. These 14 ordinary, non-manipulation patterns are structurally valid for the causal editor; the focused test checks editor validity, resolvable endpoints, source timing, and that `CycleOffset` values survive height synchronization.

| Stable ID | Pattern | 2017 printed page | 2014 PDF page |
|---|---|---:|---:|
| `highgate-fourpersononecountfeedingpasspassself` | Four person one count feeding pass pass self | 226 | 159 |
| `highgate-fourpersononecountfeedingchocolatebar` | Four person one count feeding chocolate bar | 226 | 159 |
| `highgate-fourpersononecountfeedingbookends` | Four person one count feeding bookends | 226 | 159 |
| `highgate-fourpersononecountfeedinginvertedbookends` | Four person one count feeding inverted bookends | 227 | 160 |
| `highgate-fourpersononecountfeedingcountdown` | Four person one count feeding countdown | 227 | 160 |
| `highgate-fourpersoncivilwarfeed` | Four person civil war feed | 227 | 160 |
| `highgate-fourpersonparsnipfeedingfivecountpopcorn` | Four person parsnip feeding five count popcorn | 229 | 161 |
| `highgate-mixedcountfeed` | Mixed count feed | 234 | 164 |
| `highgate-passpassselfbox` | Pass pass self box | 235 | 165 |
| `highgate-chocolatesquare` | Chocolate square | 235 | 165 |
| `highgate-bookendsbox` | Bookends box | 236 | 166 |
| `highgate-invertedbookendsbox` | Inverted bookends box | 236 | 166 |
| `highgate-twocountlinefeed` | Two count line feed | 240 | 168 |
| `highgate-urbanterrorfeedonecountlinefeed` | Urban terror feed (one count line feed) | 240 | 168 |

The inventory CSV currently omits 2014 page values for several patterns whose sequences are present in the 2014 PDF, and some of its 2017 page references do not match the printed body pages. This audit preserves the direct PDF evidence; it does not change the inventory.

## Reviewed but not transcribed

These patterns are in the requested 2017 span, but their timing or throw semantics need a dedicated transcription pass before they can be represented faithfully. I did not infer edges from the sequence alone.

- **Four person mild madness feed** (`highgate-fourpersonmildmadnessfeed`) and **Four person Rachel’s feed** (`highgate-fourpersonrachelsfeed`) (p. 228): staggered half-beat starts; hold and zip notation; Rachel’s diagram shows only half the repeating hand phase.
- **Remaining four person period-five feeds** (pp. 229–233): `highgate-fourpersonfunkybookendsfeedingfiveclubwhynot`, `highgate-fourpersonmartinsonecountfeedingwhynot`, `highgate-fourpersonmartinsonecountfeedingmaybe`, `highgate-fourpersononecountfeedingfiveclubwhynot`, and `highgate-fourpersononecountfeedingparsnip`. These use zip/heff throws and staggered starts; several also change club count or initial hand. The Parsnip/five-count-popcorn feed on p. 229 is transcribed separately because its action rows and arrow endpoints form a complete, unambiguous graph. The source says the list is non-exhaustive.
- **Box patterns** (pp. 235–238): `highgate-mildmadnessbox`, `highgate-whynotbox`, `highgate-maybebox`, and `highgate-972box` use zip/heff timing. The 972 box also has alternating double/single passes, nonuniform start phases, and initial-hand exceptions.

The newly added Parsnip/five-count-popcorn graph has a ten-beat cycle with ten alternating-hand events per person. Ben and Daniel start at beat 0; Anne and Clare start half a beat later. Named passes target the receiver's next listed event after one beat; self, zip, and heff actions target the same juggler's next, previous, and second-next event respectively. I checked these endpoints against the 2017 arrow diagram and the matching 2014 diagram. The “Line Feeds” section heading, `Two count line feed`, and `Urban terror feed (one count line feed)` are all on printed p. 240. Their inventory PDF-page field is inconsistent with the body page and footer, so this standalone source reference follows the PDF page image/text directly. No central catalog or inventory entries were changed.
