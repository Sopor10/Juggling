# Highgate transcription review: printed pp. 46–60

The existing [46–60 JSON chunk](highgate-transcriptions-46-60.json) contains Jim's asynchronous two-count and one-count patterns. The chunk now also contains the asynchronous Jim's three-count and two synchronous one-count graphs, directly transcribed from their printed event rows and causal lines. These three graphs are imported into the runtime Highgate select list; the source records remain separate.

## Added graph

| Inventory ID | 2017 printed page | 2014 printed page | Graph | Direct check |
|---|---:|---:|---:|---|
| `highgate-onecount` | 47 | 27 | 4 nodes, 4 causal edges | C# state valid; all offsets preserved |
| `highgate-jimsthreecountasynchversion` | 49 | 29 | 28 nodes / 24 causal edges over 14 beats | The seven-event sequence repeats twice for alternating-hand parity; event rows, holds, and routes are checked against both editions. |
| `highgate-synchronousonecount` | 47 | 27 | 4 simultaneous throw nodes, 4 causal edges | Both hand tracks and cycle offsets follow the source diagram; imported editor state validates |
| `highgate-synchronousonecountwithheffs` | 47 | 27 | 8 simultaneous throw nodes, 8 causal edges | Pass/heff sequence, both hand routes, and cycle offsets checked in both diagrams; imported editor state validates |

Both diagrams were visually checked. The source specifies straight single passes on every alternating-hand throw; the two-event cycle for each juggler connects each event to the next event on the partner row.

For Jim's asynchronous three-count, the seven-event source sequence is repeated twice so each row returns to its starting hand. Anne starts at phase 0, Ben at half a beat; hold nodes have no outgoing causal edge. I followed each drawn line from a non-hold event to the next active event in the specified destination row, skipping a hold when the arriving club waits in that hand. The chart's straight (Anne) and crossing (Ben) pass routes agree with the written sequences. The repeated graph has 28 event nodes and 24 outgoing edges.

The inventory previously pointed this title to 2017 PDF p. 53, an index/catalog reference. It now points to the matching body diagram on printed p. 49 / PDF p. 50 and 2014 PDF p. 29.

The synchronous one-count graph has one simultaneous R/L pair for Anne at beat 0 and one for Ben at beat 1, repeating every two beats. The four straight single-pass edges connect to the opposite hand on the receiving juggler; their cycle offsets give three beats of flight. The 2017 and 2014 source diagrams match. Its imported state passes the batch validator and catalog-wide validity test.

The synchronous one-count-with-heffs graph repeats every four beats. Anne sends the pass pair at beat 0 and heffs at beat 2; Ben starts one beat later with heffs, then passes. Pass edges switch to the opposite hand, while heff edges return to the same hand in the same juggler. The flight heights (3 for the passes, 4 for heffs) agree with the cycle-offset arrivals; the imported state passes both validity checks.

## Scope review

The synchronous one-count patterns are transcribed above. The asynchronous Jim's two- and three-count graphs are in this chunk. Synchronous transition examples are finite sequences and are not represented by these periodic graph records. The remaining spotlight and related patterns require separate visual review and edge transcription; some include zip or hurried actions.

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

Synchronous spotlight patterns and additional synchronized examples remain outside this chunk pending source transcription. The editor now supports simultaneous hand tracks. The following five-count popcorn variants still need page review.
