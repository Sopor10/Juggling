# Highgate transcription review: printed pp. 46–60

The existing [46–60 JSON chunk](highgate-transcriptions-46-60.json) contains Jim's asynchronous two-count and one-count patterns. I added the basic *One count* graph after checking that its inventory ID was absent from the central catalog and all prior transcription chunks.

## Added graph

| Inventory ID | 2017 printed page | 2014 printed page | Graph | Direct check |
|---|---:|---:|---:|---|
| `highgate-onecount` | 47 | 27 | 4 nodes, 4 causal edges | C# state valid; all offsets preserved |

Both diagrams were visually checked. The source specifies straight single passes on every alternating-hand throw; the two-event cycle for each juggler connects each event to the next event on the partner row.

## Scope review

The neighboring synchronous one-count and synchronized transition examples involve paired throws at the same instant; they remain model-boundary candidates. Jim's asynchronous variants are already present in this chunk. The rest of pp. 46–60 includes one-count zaps, countdown patterns, why-not/maybe variants, and spotlight patterns; those IDs remain unreviewed/untranslated here and require separate visual review and state checks.

## Period-five patterns: Why not? and Maybe families

I added six ordinary asynchronous patterns from the printed pp. 58–60 to the separate JSON chunk. Their diagrams and prose were checked against 2017 PDF pp. 59–61 and the corresponding 2014 PDF pp. 32–33; arrows, starting phases, and sequence descriptions match in both editions.

| Stable inventory ID | Pattern | Source pages (2017 printed/PDF; 2014 PDF) | Transcription |
|---|---|---|---|
| `highgate-whynot` | Why not? | 58/59; 32 | Anne `pass heff zip pass self`; Ben `pass self pass heff zip`; phases 0 and 0.5. |
| `highgate-whynotalternativestart` | Why not? alternative start | 58/59; 32 | Anne `pass self pass heff zip`; Ben `heff zip pass self pass`; phases 0 and 0.5. |
| `highgate-notwhy` | Not why | 58/59; 32 | Anne `pass zip self pass heff`; Ben `pass heff pass zip self`; phases 0 and 0.5. |
| `highgate-notwhyalternativestart` | Not why alternative start | 60/61; 33 | Ben `pass heff pass zip self`; Anne `self pass heff pass zip`; Ben starts at 0 and Anne at 1.5. |
| `highgate-maybe` | Maybe | 60/61; 33 | Anne `pass self zip heff pass`; Ben `heff pass pass self zip`; phases 0 and 0.5. |
| `highgate-maybealternativestart` | Maybe alternative start | 60/61; 33 | Ben `pass pass self zip heff`; Anne `heff pass pass self zip`; Ben starts at 0 and Anne at 1.5. |

Each record has a ten-beat graph period, with ten event nodes per juggler. The source sequences have five events, so the second copy keeps hand parity correct. Pass, self, heff, and zip events use their stated 3.5, 3, 4, and 1 beat causal heights respectively. Heights are marked explicit because the source diagram's horizontal layout and causal timing are separate values. Every event links to the matching same- or other-juggler event at its stated throw time; the recorded cycle offset is the exact wrap count.

A structural audit confirmed for all six records: unique event and edge IDs; 20 nodes and 20 edges each; one outgoing edge per node; all endpoints resolve; event hands alternate; and every edge's target time plus its cycle offset equals the source node time plus the source height. A temporary C# test constructed editor states from all six records and confirmed `IsValidForEditor()` plus offset preservation. These six records are now imported into the central Highgate preset data and appear in the causal editor's Highgate select list. The catalog-wide test checks each resulting editor state under its intended ID; the separate chunk remains the page-by-page source transcription.

The adjacent synchronous spotlight patterns and synchronized one-count examples remain outside this chunk because their same-time paired throws need a layout with separate hand lanes. The following five-count popcorn variants also need a separate page review.
