# Highgate printed pages 30–45: visual review and causal transcription

This work block uses the 2017 expanded edition, printed pages 30–45 (PDF pages 31–46). Its matching material was checked against the 2014 PDF where the content is shared: 2014 PDF pages 16–17 for the Brolly trick diagrams, pages 18–22 for four-handed siteswap notation and the recipe examples, pages 23–25 for the five-club patterns, and page 26 for the popular six-club counts. The corresponding diagrams and throw descriptions agree across those pages. The 2017 edition adds or rearranges content, so the 2014 PDF page number is recorded per pattern rather than inferred by a constant offset.

The JSON file `highgate-transcriptions-30-45.json` uses the same individual pattern record schema as the editor's Highgate dataset. Its three records preserve the visible node timing, hand labels, empty hold nodes, causal edges, and source links. Their source sequences are ordinary passing patterns; the reviewed pages contain no external manipulator role.

## Inventory mapping

| Stable inventory ID | Pattern | Source pages (2017 printed/PDF; 2014 PDF) | Status |
| --- | --- | --- | --- |
| `highgate-fiveclubonecount` | Five club one count | 40/41; 23 | Transcribed as `highgate-fiveclubonecount` in the JSON chunk. Keeps the 1.5-beat phase offset, pass/hold/hold rows, and holds as unconnected nodes. |
| `highgate-zapholdself` | Zap hold self | 40/41; 23 | Transcribed as `highgate-zapholdself`. Keeps the half-beat phase offset, half-spin zap edges, self edges, and all hold beats. |
| `highgate-onecountwithzaps` | One count with zaps | 41/42; 24 | Transcribed as `highgate-onecountwithzaps`. Keeps alternating hands, half-beat phase, and crossing versus straight zap destinations. |
| `highgate-sevenclubonecount` | Seven club one count | 30/31; 16 | Introductory base-pattern explanation; the three following Brolly tricks have their own diagrams and are listed separately. |
| `highgate-fiveclubonecountwithzaps` | Five club one count with zaps | 31/32; 17 | Introductory notation section; individual Double hold/zap/zip examples are listed separately. |
| `highgate-fourhandedsiteswaps` | Four handed siteswaps | 32/33; 18 | Section heading; recipe and derived patterns are listed separately. |
| `highgate-whydoesthisrecipework` | Why does this recipe work? | 34/35; 19 | Explanatory prose, not an independent pattern. |
| `highgate-convertingsymmetricasynchronousfourhandedsiteswa-0987b5f53e`, `highgate-convertingsymmetricsynchronousfourhandedsiteswap-d9ed3bf63a` | Converting asynchronous/synchronous four-handed siteswaps to Prechac notation | 39/40; 22 | Explanatory conversion examples; their rendered graphs contain backward zip arrows, so they are not translated with the current model. |
| `highgate-fiveclubpatterns` | Five club patterns | 40/41; 23 | Section heading; its individual patterns are listed separately. |
| `highgate-fiveclubthreecount` | Five club three count | 40/41; 23 | Already represented in the main Highgate dataset as `highgate-fiveclubthreecount`; not duplicated in this chunk. |
| `highgate-sixcount`, `highgate-fourcount`, `highgate-twocount`, `highgate-fivecount`, `highgate-threecount` | Popular six-club counts | 44–45/45–46; 26 | Already represented in the main dataset, including the ambidextrous variants. |
| `highgate-doubleheff64` | Double heff (64) | 30/31; 16 | Ordinary partner trick. Diagram reviewed; full event timing/hand mapping still being derived, so no preset claim yet. |
| `highgate-triplepassself753` | Triple pass self (753) | 30/31; 16 | Ordinary partner trick. Diagram reviewed; full event timing/hand mapping still being derived. |
| `highgate-tripleself73` | Triple self (73) | 30/31; 16 | Ordinary excited-state entry/trick/exit sequence. Diagram reviewed; full event timing/hand mapping still being derived. |
| `highgate-doublehold42` | Double hold (42) | 31/32; 17 | Ordinary partner trick. Diagram reviewed; exact phase and target timing still being derived. |
| `highgate-doublezapzip531`, `highgate-doublezip51` | Double zap zip (531), Double zip (51) | 31/32; 17 | Ordinary tricks, but their source diagrams include backward zip arrows. Current editor normalization cannot preserve these directions. |
| `highgate-therecipe` | Four-handed siteswaps recipe, example 9784266 | 32–33/33–34; 18–19 | Ordinary pattern; derived sequence includes `-2` zip arrows. Current editor normalization changes backward edges. |
| `highgate-thesynchronousrecipe` | Synchronous recipe example | 35–36/36–37; 20–21 | Ordinary pattern; derived sequence includes `-2` zip arrows and simultaneous events. Backward edges cannot be preserved; simultaneous same-time throw layout is also limited. |
| `highgate-passselfzip` | Pass self zip (726) | 40/41; 23 | Ordinary pattern with backward zip arrows; model extension required. |
| `highgate-parsnip`, `highgate-fiveclubwhynot`, `highgate-fiveclubnotwhy` | Parsnip, Five club why not, Five club not why | 41–42/42–43; 24–25 | Ordinary patterns with backward zip arrows; model extension required. |
| `highgate-fiveclubnotwhyalternativestart` | Five club not why alternative start | 42/43; 25 | Ordinary pattern with backward zip arrows; model extension required. |
| `highgate-heffzipzap`, `highgate-zapzipheff`, `highgate-zapzipheffalternativestart` | Heff zip zap, Zap zip heff, Zap zip heff alternative start | 43/44; 25 | Ordinary patterns with backward zip arrows; model extension required. |
| `highgate-58552` | 58552 | Not in the reviewed 2017 body; 2014 PDF p. 25 | Ordinary 2014 pattern with backward zip arrows; no matching 2017 body entry was found. |
| `highgate-prechacnotation` | Prechac notation explanation | 38–39/39–40; 22 | Explanatory notation page, not an individual pattern. |

## Validation

The JSON is valid and all three graphs have unique node/edge IDs, resolvable endpoints, and nonnegative derived throw heights. I constructed each state and ran `SynchronizeHeights`; its `CycleOffset`s remained unchanged. Each graph also passes `IsValidForEditor` when checked with an already registered preset ID, which exercises all state/edge invariants. With its actual new `PresetId`, `IsValidForEditor` returns false until that ID is imported into the central preset catalog, because the catalog's known-ID guard rejects IDs that are not yet registered. Per scope, this block does not edit that catalog.

Zip-bearing patterns are not labeled translated: `SynchronizeHeights` increments negative cycle offsets to nonnegative values, turning a backward arrow into a forward one. Those entries remain explicit in the table for follow-up after the editor model can preserve edge direction. In the expanded PDF, the body diagrams for Zap hold self and Five club one count are printed on p. 40 (PDF p. 41), while the index points to printed p. 43; the JSON cites the body diagram page. The 2014 visual pages 23–26 match the corresponding pattern body diagrams.
