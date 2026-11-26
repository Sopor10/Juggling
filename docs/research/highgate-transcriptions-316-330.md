# Highgate transcription batch: 2017 printed pages 316–330

Work file: [highgate-transcriptions-316-330.json](highgate-transcriptions-316-330.json). It contains four checked, importable graph records. No central preset catalog, inventory, or test file was changed.

## Included patterns

| Stable inventory ID | Pattern | 2017 body page | 2014 PDF page | Graph |
|---|---|---|---|---|
| `highgate-thirteenclubfourcount` | Thirteen club four count | printed p. 319 / PDF p. 320 | p. 228 | 16 nodes / 16 causal edges |
| `highgate-thirteenclubthreecount` | Thirteen club three count | printed p. 319 / PDF p. 320 | p. 228 | 12 nodes / 12 causal edges |
| `highgate-thirteenclubtwocount` | Thirteen club two count | printed p. 320 / PDF p. 321 | p. 229 | 8 nodes / 8 causal edges |
| `highgate-thirteenclubonecount` | Thirteen club one count | printed p. 320 / PDF p. 321 | p. 229 | 4 nodes / 4 causal edges |

Each person gets one throw node per entry in their written sequence, including self throws. Every throw has a causal edge to the next event that receives that club, so each complete periodic graph has one edge per node. Self edges use height 3; pass edges use the Prechac heights 4, 3¾, 3½, and 3¼. Those heights, event phases, and cycle offsets make the arrow endpoints line up with the recipients' event nodes. There are no holds in these four sequences. The source's starting club inventory and the straight/crossing trajectory details are noted but are not state fields.

For the four-count pattern, the 2014 book also has a differently named `Thirteen club four count box` pattern on PDF p. 221. Its written sequences and routing differ, so it is not treated as an edition-equivalent alias. The corresponding standalone four-count, three-count, two-count, and one-count patterns were visually compared between the 2017 and 2014 sources on pp. 228–229; their phases, sequences, and routes match.

## State validation

A temporary validator constructed each record, called `Create()`, ran `SynchronizeHeights()`, and compared all serialized cycle offsets. Results:

- `Create()` succeeded for all four graphs (16/16, 12/12, 8/8, and 4/4 nodes/edges).
- Graph invariants passed `IsValidForEditor()` when the new preset ID was temporarily replaced with registered placeholder ID `highgate-three-count`.
- `SynchronizeHeights()` preserved all four graphs' serialized cycle offsets.
- The intended IDs are not centrally registered yet, so `IsValidForEditor()` returns false with those IDs until catalog import. Rerun after import with the real IDs.
- The initial pass-only graphs were incomplete and have been replaced with full graphs containing every self-throw edge. No source action is a hold in these four patterns.

## Other patterns in the requested span

The following named entries are visible in the section and remain untranscribed in this chunk; they must stay discoverable in the inventory and receive separate presets if later found representable:

- Printed pp. 316–318: `Thirteen club funky bookends line feed, feeding five count popcorn and five club why not?`; `Thirteen club Martin's one count line feed, feeding five count popcorn and why not?`; `Thirteen club one count line feed, feeding five count popcorn and parsnip`; `Thirteen club one count line feed, feeding why not? and five count popcorn`; and `Thirteen club one count line feed, feeding maybe and five count popcorn`.
- Printed pp. 320–323: `Thirteen club double urban terror feed`; `Thirteen club three count accommodation`; `Thirteen club low fat chocolate box`; `Thirteen club bookshelf`; `Thirteen club chocolate bookshelf`; `Thirteen club countdown accommodation`; and `Thirteen club runaway`.
- Printed pp. 325–330: `Thirteen club weave`; `Thirteen club dresser drawer weave`; `Thirteen club it's a good one (countdown weave)`; `Thirteen club gorilla weave`; and `Thirteen club synchronous gorilla weave`.

The line-feed pages repeatedly use explicit `heff` and `zip` tokens; those actions need exact semantics and cannot safely be turned into self-holds or ordinary passes. Several other patterns have longer or alternating sequences and require each diagram arrow to be traced before graph acceptance. The weave and runaway entries describe changing spatial formations and walking paths; the current editor can retain a causal throw graph but cannot represent those positions, paths, or transition cues. I have not marked these named entries unrepresentable solely because they are not in this batch.

## Manipulation review

The included four patterns and the visible candidate entries in this span describe ordinary passing, feeding, accommodation, boxes, and walking/weaving. The source does not describe an external manipulator, takeout, hijack, or lo-jack in this span. Walking and movement are not classified as manipulation.

## Source coverage

The requested body range is 2017 printed pp. 316–330, corresponding to physical PDF pp. 317–331. Each source page in this range was visually scanned for named patterns and diagrams. The four included graphs were individually compared with both edition diagrams and the accompanying sequences. The 2017 expanded PDF is [jugglingedge.com/pdf/highgate.pdf](https://jugglingedge.com/pdf/highgate.pdf); the 2014 PDF is [passing.zone/wp-content/uploads/highgate.pdf](https://passing.zone/wp-content/uploads/highgate.pdf).
