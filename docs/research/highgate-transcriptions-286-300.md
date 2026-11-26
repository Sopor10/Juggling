# Highgate transcription audit: 2017 printed pp. 286–300

Sources: Aidan Burns, *The Highgate Collection*, 2017 expanded [PDF](https://jugglingedge.com/pdf/highgate.pdf), checked against the 2014 [PDF](https://passing.zone/wp-content/uploads/highgate.pdf). The 2017 printed page is one less than its PDF page in this section. The 13-club patterns also occur in 2014 pp. 212–215, with matching throw rows, start positions, and straight/crossing pass descriptions.

## Imported graphs

| Stable inventory ID | Pattern | 2017 printed pages | 2014 printed pages | Validation |
|---|---|---:|---:|---|
| `highgate-rotatingy` | Rotating y | 293 | 211 | Valid; offsets preserved |
| `highgate-thirteenclubtwocountfeed` | Thirteen club two count feed | 295 | 212 | Valid; offsets preserved |
| `highgate-thirteenclubpasspassselffeed` | Thirteen club pass pass self feed | 295 | 212 | Valid; offsets preserved |
| `highgate-thirteenclubonecountfeedingfourcount` | Thirteen club one count feeding four count | 295–296 | 212–213 | Valid; offsets preserved |
| `highgate-thirteenclubonecountfeedingthreecount` | Thirteen club one count feeding three count | 297 | 213 | Valid; offsets preserved |
| `highgate-thirteenclubonecountfeedingtwocount` | Thirteen club one count feeding two count | 297 | 213 | Valid; offsets preserved |
| `highgate-thirteenclubonecountfeedingpasspassself` | Thirteen club one count feeding pass pass self | 297 | 213–214 | Valid; offsets preserved |
| `highgate-thirteenclubonecountfeedingchocolatebar` | Thirteen club one count feeding chocolate bar | 298 | 214 | Valid; offsets preserved |
| `highgate-thirteenclubonecountfeedingbookends` | Thirteen club one count feeding bookends | 298–299 | 214 | Valid; offsets preserved |
| `highgate-thirteenclubonecountfeedinginvertedbookends` | Thirteen club one count feeding inverted bookends | 299–300 | 214–215 | Valid; offsets preserved |

I transcribed the four throw rows from the printed text and compared them with the causal-diagram arrows and hand sequence in the page images. Each throw's recipient comes from its row; source pass type determines the minimum flight interval (one beat for a single, two for the explicitly labeled double), and the causal edge reaches the recipient's first eligible throw after that interval. Self throws connect to the next throw by the same person. The stated phase delays are represented in the throw start times. Single/double, straight/crossing, and hand labels match the printed descriptions. Holds, zips, or heffs are absent in these records.

The component `CausalDiagramEditorState` was constructed directly from every record, then `SynchronizeHeights()` and `IsValidForEditor()` were run. Every state was valid and every serialized `CycleOffset` survived unchanged. As these new IDs are not centrally registered yet, validation temporarily used registered ID `highgate-three-count` for `PresetId`; the per-record JSON fields note this. Derived heights were 3 for Rotating y; 3 and 4 for the two-count feed; and 3 and 3.5 for the other listed 13-club feed patterns.

## Remaining patterns in the inspected block

All identifiable patterns in these pages are collaborative feeds/rotating patterns; none has a solo manipulator. The following were inspected and classified as non-manipulation patterns but not transcribed in this chunk:

- `It's a good one (countdown weave)` (p. 286), `Gorilla weave` (p. 287): moving figure-eight walk paths and changing partners.
- `Havana feed` (p. 288), `Interlocking weaves` (p. 289): feeder role changes and moving formations; Havana's diagram explicitly omits the full cycle.
- `Karamazov shuffle` (p. 290), `Cyclone` (p. 291), `Seattle shuffle` (p. 292): formation-dependent recipient changes and movement paths.
- `Rotating λ` (p. 294): a 12-step moving variation of Rotating y; the spatial role changes are not represented by the one-cycle graph here.
- The section heading `Thirteen club four person feeds` (p. 295) is not itself a pattern. Its individual simple asynchronous patterns listed above are transcribed.

No manipulation pattern was found in the inspected page range. This classification is based on the pattern descriptions and diagrams, not solely on the inventory's extracted-text flags.
