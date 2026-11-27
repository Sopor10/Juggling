# Highgate source transcription review: printed pp. 121–125

The companion [JSON file](highgate-transcriptions-121-135.json) is a separate source-data file. It does not modify the runtime preset catalog or the inventory CSV. This review covers actual 2017 body pages 121–125. Parent work owns printed pp. 126–130; those pages are not included here.

## New independent graphs transcribed

| Stable ID | Pattern | 2017 body page | 2014 body page | Events / edges |
|---|---|---:|---:|---:|
| `highgate-whynotfeedingfiveclubwhynotandfivecountpopcorn` | Why Not? feeding five club Why Not? and five count popcorn | 122 (PDF 123) | 81 | 30 / 30 |
| `highgate-maybefeedingfiveclubwhynotandfivecountpopcorn` | Maybe feeding five club Why Not? and five count popcorn | 123 (PDF 124) | 82 | 30 / 30 |
| `highgate-parsnipfeedingfivecountpopcornandwhynot` | Parsnip feeding five count popcorn and Why Not? | 123 (PDF 124) | 82 | 30 / 30 |
| `highgate-parsnipfeedingmaybeandfivecountpopcorn` | Parsnip feeding Maybe and five count popcorn | 123 (PDF 124) | 82 | 30 / 30 |
| `highgate-funkybookendsfeedingfiveclubwhynotandwhynot` | Funky bookends feeding five club Why Not? and Why Not? | 125 (PDF 126) | 83 | 30 / 30 |
| `highgate-funkybookendsfeedingmaybeandfiveclubwhynot` | Funky bookends feeding Maybe and five club Why Not? | 125 (PDF 126) | 83 | 30 / 30 |
| `highgate-martinsonecountfeedingwhynot` | Martin's one count feeding Why Not? | 125 (PDF 126) | 83 | 30 / 30 |

The titles, row sequences, hand labels, pass destinations, zip arrows, and period-five siteswaps were checked against the printed body diagrams. The 2014 diagrams and sequences were also visually compared at the cited PDF pages. For each pattern the five-action sequence repeats twice, producing a ten-beat cycle with alternating hands. Starting phases follow each pattern's start text; the Parsnip/Maybe row for Anne begins one-and-a-half beats after Ben, with its graph cycle normalized so every lane has ten nodes in the same `[0,10)` period.

Each event has one outgoing causal edge. The transcription uses the source lane action and corresponding arrow: a named pass goes to the named receiver's next event at +1.5 beats; self and heff go to the next and second-next same-lane events; zip goes to the previous same-lane event. Heights are explicit and each endpoint satisfies `target.startTime + cycleOffset × 10 = source.startTime + height − 2`. The JSON was checked for unique event IDs, resolvable endpoints, 30 distinct sources, and the timing equation on all 210 edges.

## Index-to-body page mismatches and scope boundaries

The inventory's page column is an index pointer, not a reliable body-diagram location. The following in-scope index candidates point to body pages before this batch, so they were not duplicated here:

| Inventory ID | Index pointer | Visible 2017 body location | Handling |
|---|---:|---:|---|
| `highgate-civilwarfeed` | printed 121 / PDF 122 | printed 117 / PDF 118 | Page mapping issue; no transcription in this batch. |
| `highgate-martinsmildnessmartinsmadness` | printed 121 / PDF 122 | printed 117 / PDF 118 | Page mapping issue; no transcription in this batch. |
| `highgate-martinsneurosismartinspsychosis` | printed 122 / PDF 123 | printed 118 / PDF 119 | Page mapping issue; no transcription in this batch. |
| `highgate-scatteredsunshine` | printed 122 / PDF 123 | printed 119 / PDF 120 | Page mapping issue; no transcription in this batch. |
| `highgate-jimstwocountfeedrachelsfeed` | printed 123 / PDF 124 | printed 120 / PDF 121 | Diagram starts before this batch; continuation on printed p.121 is not an independent graph, so no transcription here. |
| `highgate-passpasszipfeedingthreecount` | printed 123 / PDF 124 | printed 120 / PDF 121 | Existing separate record; excluded from this batch as a 106–120 overlap. |
| `highgate-passpasszipfeedingfrenchthreecount` | printed 123 / PDF 124 | printed 120 / PDF 121 | Existing separate record; excluded from this batch as a 106–120 overlap. |
| `highgate-jimsinvertedbookendsfeed` | printed 125 / PDF 126 | printed 122 / PDF 123 | Existing separate record; the body graph is on printed p.122. |

The `highgate-threepersonperiodfivefeeds` entry is a section heading, not an independent pattern graph. The titles whose index pointers fall on pp. 126–130 are assigned to the parent batch and are intentionally absent here. No manipulation patterns or aliases were added.

## Source links

- [2017 expanded Highgate PDF, printed p. 122 / PDF p. 123](https://jugglingedge.com/pdf/highgate.pdf#page=123)
- [2017 expanded Highgate PDF, printed p. 123 / PDF p. 124](https://jugglingedge.com/pdf/highgate.pdf#page=124)
- [2017 expanded Highgate PDF, printed p. 125 / PDF p. 126](https://jugglingedge.com/pdf/highgate.pdf#page=126)
- [2014 Highgate PDF, body p. 81](https://passing.zone/wp-content/uploads/highgate.pdf#page=81)
- [2014 Highgate PDF, body p. 82](https://passing.zone/wp-content/uploads/highgate.pdf#page=82)
- [2014 Highgate PDF, body p. 83](https://passing.zone/wp-content/uploads/highgate.pdf#page=83)
