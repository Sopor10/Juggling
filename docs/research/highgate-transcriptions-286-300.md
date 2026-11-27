# Highgate transcription audit: 2017 printed pp. 286–300

Sources: Aidan Burns, *The Highgate Collection*, 2017 expanded [PDF](https://jugglingedge.com/pdf/highgate.pdf), checked against the 2014 [PDF](https://passing.zone/wp-content/uploads/highgate.pdf). In this section the 2017 PDF page is printed page + 1. The corresponding 2014 sequence is pp. 203–215 (PDF pp. 204–216). The older edition confirms the descriptions and diagrams for the named patterns below; the four-person feed section also matches in throw rows, start positions, and pass descriptions.

## Standalone records present in this chunk

| Stable ID | Pattern | 2017 printed page | 2014 printed page |
|---|---|---:|---:|
| `highgate-rotatingy` | Rotating y | 293 | 211 |
| `highgate-thirteenclubtwocountfeed` | Thirteen club two count feed | 295 | 212 |
| `highgate-thirteenclubpasspassselffeed` | Thirteen club pass pass self feed | 295 | 212 |
| `highgate-thirteenclubonecountfeedingfourcount` | Thirteen club one count feeding four count | 295–296 | 212–213 |
| `highgate-thirteenclubonecountfeedingthreecount` | Thirteen club one count feeding three count | 297 | 213 |
| `highgate-thirteenclubonecountfeedingtwocount` | Thirteen club one count feeding two count | 297 | 213 |
| `highgate-thirteenclubonecountfeedingpasspassself` | Thirteen club one count feeding pass pass self | 297 | 213–214 |
| `highgate-thirteenclubonecountfeedingchocolatebar` | Thirteen club one count feeding chocolate bar | 298 | 214 |
| `highgate-thirteenclubonecountfeedingbookends` | Thirteen club one count feeding bookends | 298–299 | 214 |
| `highgate-thirteenclubonecountfeedinginvertedbookends` | Thirteen club one count feeding inverted bookends | 299–300 | 214–215 |

The source JSON remains research data and is separate from the runtime catalog and inventory. The records above were compared structurally with the catalog entries (ignoring the catalog-only `visualVerification` annotation); a focused NUnit test checks every record's endpoints, editor validity, and preservation of `CycleOffset` after height synchronization. The four-row descriptions and diagram edges in the feed section are explicit in the book. Pass height is inferred from timing between causal events; the one-count and two-count interval distinctions follow the printed counts.

## Other patterns and section transitions found in the actual body pages

| 2017 printed page | 2014 printed page | Heading / content | Source evidence and transcription status |
|---:|---:|---|---|
| 286 | 203 | It's a good one (countdown weave) | Complete numbered spatial sequence and explicit causal diagram. Transcribed as `highgate-itsagoodonecountdownweave`; movement is supplementary notation. |
| 287 | 204 | Gorilla weave | Complete numbered sequence, explicit causal diagram, and club-color convention. Transcribed as `highgate-gorillaweave`; the color convention and figure-eight movement stay in notation. |
| 288 | 205 | Havana feed | Feeder changes after five passes; source explicitly says the displayed diagram omits the whole pattern and gives a role permutation for its final-to-first transition. Not yet present. The source defines enough transition detail to investigate a full expanded cycle; this is not evidence of an editor limitation. |
| 289 | 206 | Interlocking weaves | Feeder changes after four passes; text specifies starting order, mirrored feed, count roles, and two figure-eight paths. No separate causal-diagram caption on the body page. Not yet present. The sequence needs reconstruction from the text and panels before deciding whether any state is missing. |
| 290 | 208 | Karamazov shuffle | Eight numbered formation panels and explicit causal diagram. Transcribed as `highgate-karamazovshuffle`; alternating R/L throw lanes and diagram pass links are recorded, with movement retained in notation. |
| 291 | 209 | Cyclone | Numbered formation panels and explicit causal diagram. Transcribed as `highgate-cyclone`; oval movement and count-dependent roles remain in notation. |
| 292 | 210 | Seattle shuffle | Numbered sequence and explicit causal diagram; source says the last diagram is the first with Anne/Clare and Ben/Daniel roles swapped. Transcribed as `highgate-seattleshuffle`, with the role-swapped half expanded into a periodic cycle. |
| 293 | 211 | Rotating y | Explicit diagram; already in the standalone chunk and runtime catalog. |
| 294 | 212 | Rotating λ | Twelve numbered moving states and explicit causal diagram. Transcribed as `highgate-rotatingλ`; the three repeated four-count pass phases and hand rows are recorded, with walking directions retained in notation. |
| 295 onward | 212 onward | Thirteen club four person feeds | Section heading, not itself a pattern. Individual feed patterns are listed separately above. |

The inventory's page references are index/title-derived and drift from the printed body pages in this range. Examples: `highgate-cyclone` points to 295 while its body page is 291; `highgate-gorillaweave` points to 291 while its body page is 287; `highgate-seattleshuffle` points to 296 while its body page is 292; `highgate-rotatingλ` points to 298 while its body page is 294. These are source-audit findings only; this task does not edit the inventory.

## Names, duplicates, and transitions

The 2014 inventory has a separate `Countdown weave` title row without a body-page reference, while the actual body pattern here is explicitly titled `It's a good one (countdown weave)` (2017 p. 286 / 2014 p. 203). The title resemblance does not establish that they are duplicates, so this transcription keeps the body title's stable ID and leaves that candidate alias unresolved. Likewise, the inventory's `Gorilla weave (13 clubs)` is a distinct pattern from the four-person `Gorilla weave` on p. 287 / 2014 p. 204; do not merge them by title. `Rotating λ` (p. 294) is explicitly introduced as an Australian version of Rotating y (p. 293), but the changed pass/movement sequence makes it a separate pattern record.

Havana feed (p. 288 / 2014 p. 205) says its diagram omits the whole pattern and specifies a role permutation at the end. Seattle shuffle (p. 292 / 2014 p. 210) likewise says its diagram is incomplete and gives its end-to-start role swap. The Seattle record expands the stated role-swapped half into a complete periodic graph. Interlocking weaves (p. 289 / 2014 p. 206) has no separate causal-diagram caption, but the numbered formation panels and prose describe the feeder rotation and mirrored feed. These are source representation/transition details; none demonstrates an editor limitation.

## Manipulation and model limits

No manipulation is described or shown in the patterns on these pages. The diagrams show ordinary throws, passes, self throws, and role rotations; no manipulator, persistent club, or club-to-club manipulation is present. Movement paths alone are not a reason to exclude a pattern: they can be retained as textual notation alongside a causal graph. The editor graph models throw event order and destinations, but not the jugglers' positions, walking paths, or the four-color club identity scheme in Gorilla weave; these remain in notation. Havana and Seattle's role transitions can be represented by expanding the causal cycle, as done for Seattle. All six patterns with explicit causal diagrams are now transcribed into the standalone JSON; their new IDs are not registered in the runtime catalog.
