# Highgate transcription audit: 2017 printed pp. 224–240

Sources: Aidan Burns, *The Highgate Collection*, 2017 expanded edition ([PDF](https://jugglingedge.com/pdf/highgate.pdf)); compared with the 2014 edition bundled at `Siteswaps.Mcp.Server/Resources/highgate.pdf` and published [here](https://passing.zone/wp-content/uploads/highgate.pdf). The 2017 printed page number is one less than its PDF page number throughout this span (for example, printed p. 224 is PDF p. 225). The initial chunk covered pp. 226–240; the audit now also includes the adjacent four-person feed pages 224–225.

## Standalone causal graphs in this span and adjacent pages

The records below are transcribed as one causal event per listed throw. I checked the sequence rows, starts, and page labels against the 2017 PDF. I also checked the corresponding patterns in the 2014 PDF at pp. 157–167 and 168. Their throw sequences and phase starts agree. These 28 ordinary, non-manipulation patterns are structurally valid for the causal editor; the focused test checks editor validity, resolvable endpoints, source timing, and that `CycleOffset` values survive height synchronization.

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
| `highgate-mildmadnessbox` | Mild madness box | 235 | 165 |
| `highgate-bookendsbox` | Bookends box | 236 | 166 |
| `highgate-invertedbookendsbox` | Inverted bookends box | 236 | 166 |
| `highgate-whynotbox` | Why Not? box | 237 | 166 |
| `highgate-maybebox` | Maybe box | 238 | 167 |
| `highgate-972box` | 972 box | 238 | 167 |
| `highgate-twocountlinefeed` | Two count line feed | 240 | 168 |
| `highgate-urbanterrorfeedonecountlinefeed` | Urban terror feed (one count line feed) | 240 | 168 |

The inventory CSV currently omits 2014 page values for several patterns whose sequences are present in the 2014 PDF, and some of its 2017 page references do not match the printed body pages. The inventory now marks the imported graphs as translated; its page references are corrected where they differed from the printed body pages.

## Reviewed exceptions and limits

The following source graphs remain untranscribed because their timing or throw semantics need a dedicated transcription pass. I did not infer edges from the sequence alone.

- **Four person mild madness feed** (`highgate-fourpersonmildmadnessfeed`) and **Four person Rachel’s feed** (`highgate-fourpersonrachelsfeed`) (p. 228): the source rows include `hold` actions. Those are catches retained in a hand, not throws; the editor graph has throw events as nodes and no separate held-catch node, so routing the incoming causal edge to the next throw would change its flight time. Rachel’s diagram also shows only half the repeating hand phase; the action rows do specify that the other hand repeats.

All six separately titled period-five feeds shown on pp. 229–233 are transcribed, including their zip/heff actions and half-beat starts. The source says the list is non-exhaustive and describes interchangeable variants without printing separate diagrams; those unillustrated combinations are not separate source-backed graphs here.

The period-five graphs have ten-beat cycles with ten alternating-hand events per person. Ben and Daniel start at beat 0; Anne and Clare start half a beat later. Named passes target the receiver’s next listed event at or after source time + 1; self, zip, and heff actions target the same juggler’s next, previous, and second-next event. Each destination was checked against the 2017 arrow diagram and the corresponding 2014 diagram. The box graphs include periods of five, seven, and three doubled for hand parity. A named single pass targets the next receiver event at or after source time + 1; a named double pass in 972 targets the next event at or after source time + 2. The source explicitly identifies zip as the previous event and heff as the second-next event. The 2017 pp. 238 source text gives the Maybe and 972 action rows and starts, but the available page image lacks the diagrams; the corresponding 2014 p. 167 diagrams were used to visually verify their arrows. For the adjacent pp. 224–225 base feeds, periods of three repeat twice so hands return to the same phase; even periods repeat once. Their named passes and self throws target the receiver or thrower’s next listed event. The “Line Feeds” section heading, `Two count line feed`, and `Urban terror feed (one count line feed)` are all on printed p. 240. Their inventory PDF-page field is inconsistent with the body page and footer, so this standalone source reference follows the PDF page image/text directly. The five four-person feeds from pp. 224–225 and the four box graphs are imported into the runtime catalog and marked translated in the inventory. The Maybe/972 records explicitly cite the 2014 diagrams because the available 2017 page image lacks the diagrams.
