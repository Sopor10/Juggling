# Highgate causal transcriptions: Parsnip and five-club variants

This source batch records three periodic diagrams from Aidan Burns's *The New Highgate Collection* (2017 expanded edition), printed p. 42 / PDF p. 43. I compared each diagram and its written sequence with the matching 2014 page 24. The source records remain separate from the runtime catalog; the three graphs are now imported into the Highgate select list, with inventory status and editor validity tests updated.

| Inventory ID | Pattern | Siteswap / Prechac | Cycle | Events / edges |
| --- | --- | --- | ---: | ---: |
| `highgate-parsnip` | Parsnip | `77722` / `3.5p3.5p13.5p1` | 10 beats | 20 / 20 |
| `highgate-fiveclubwhynot` | Five club why not? | `78622` / `3.5p3141` | 10 beats | 20 / 20 |
| `highgate-fiveclubnotwhy` | Five club not why | `72286` / `3.5p1314` | 10 beats | 20 / 20 |

The source gives five actions per person. I repeat those five actions twice because the first hand alternates on each beat; after five beats the action sequence returns but the first hand has changed. The ten-beat graph cycle restores both the action and hand pairing. Each throw's listed Prechac height determines its arrival time. Pass edges use the stated straight/crossing route and recipient; self, heff, and zip edges return to the same juggler's next hand/event. For these patterns, every landing time resolves to exactly one event in the receiving hand. Cycle offsets preserve arrivals that cross the ten-beat boundary.

## Source checks

- **Parsnip:** Anne starts on the right at time 0 and throws pass, pass, zip, pass, zip with straight singles. Ben starts half a beat later on the right and throws pass, zip, pass, pass, zip with crossing singles.
- **Five club why not?:** Anne starts on the right at time 0 and throws pass, self, zip, heff, zip. Ben starts half a beat later on the right and throws heff, zip, pass, self, zip. Anne's single passes are straight; Ben's are crossing.
- **Five club not why:** Ben starts on the right at time 0 and throws pass, zip, self, zip, heff with crossing singles. Anne starts on the right one and a half beats later and throws heff, pass, zip, self, zip with straight singles. The diagram confirms that starting hand and phase.

## Structural validation

The records were generated from those source rows and checked locally: each has unique event IDs, 20 event nodes, one outgoing causal edge per node, a unique target at each edge's arrival time and hand, and cycle offsets consistent with the ten-beat repeat. This is a source-transcription check; the repository's full editor-state preset validator will run after catalog import.
