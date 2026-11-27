# Highgate transcription batch: 2017 printed pages 76–90

This is a partial, visually reviewed batch for the 2017 expanded Highgate PDF. Pattern data is kept in [highgate-transcriptions-76-90.json](highgate-transcriptions-76-90.json); it contains 13 records, including four newly transcribed seven-club patterns from printed p. 82. The page range is still under review.

## Batch contents and sources

| Stable/catalog ID | Pattern | 2017 source | 2014 comparison | State size |
|---|---|---|---|---|
| `highgate-jimsfizzbuzzasynchronousversion` | Jim's fizz buzz (asynchronous version) | printed p. 76, PDF p. 77 | PDF p. 47 has Fizz buzz and Jim's synchronous version; not this asynchronous pattern | 60 nodes, 48 edges |
| `highgate-jimsbookendsakabrainstormasynchronousversion` | Jim's bookends aka brainstorm (asynchronous version) | printed p. 76, PDF p. 77 | No exact counterpart found; 2014 Funky Bookends is a different pattern | 52 nodes, 40 edges |
| `highgate-sevenclubonecountonsingles` | Seven club one count on singles | printed p. 80, PDF p. 81 | PDF p. 16 has related seven-club one-count material, not the exact singles-only variant | 4 nodes, 4 edges |
| `highgate-sevenclubtwocountonsingles` | Seven club two count on singles | printed p. 81, PDF p. 82 | 2014 PDF p. 51 checked; no exact matching diagram found | 4 nodes, 4 edges |
| `highgate-777786` | 777786 | printed pp. 83–84, PDF pp. 84–85 | 2014 PDF p. 53 | 12 nodes, 12 edges |
| `highgate-777867` | 777867 | printed p. 84, PDF p. 85 | No exact counterpart found | 12 nodes, 12 edges |
| `highgate-779667withcrossingdoubles` | 779667 with crossing doubles | printed p. 83, PDF p. 84 | PDF p. 53; matching sequence/diagram | 12 nodes, 12 edges |
| `highgate-777966withstraightdoubles` | 777966 with straight doubles | printed p. 83, PDF p. 84 | PDF p. 53; matching sequence/diagram | 12 nodes, 12 edges |
| `highgate-8x6x68x8x6withcrossingsinglesandstraightdoubles` | (8x,6x)(6,8x)(8x,6), crossing singles and straight doubles | printed p. 84, PDF p. 85 | PDF p. 53; matching sequence/diagram | 12 nodes, 12 edges |
| `highgate-sevenclubthreecountondoubles` | Seven club three count on doubles | printed p. 82, PDF p. 83 | PDF p. 52; matching sequence/diagram | 12 nodes, 12 edges |
| `highgate-sevenclubthreecountonsinglestechno` | Seven club three count on singles (techno) | printed p. 82, PDF p. 83 | PDF p. 52; matching sequence/diagram | 12 nodes, 12 edges |
| `highgate-sevenclubfourcounttripleself` | Seven club four count (triple self) | printed p. 82, PDF p. 83 | PDF p. 52; matching sequence/diagram | 8 nodes, 8 edges |
| `highgate-sevenclubfourcountondoubles` | Seven club four count on doubles | printed p. 82, PDF p. 83 | PDF p. 52; matching sequence/diagram | 16 nodes, 16 edges |

The source is Aidan Burns, [expanded Highgate PDF (2017 file)](https://jugglingedge.com/pdf/highgate.pdf). The locally bundled 2014 PDF is the same file served at [passing.zone](https://passing.zone/wp-content/uploads/highgate.pdf). The printed-page/PDF-page offset is recorded per record in the JSON.

## Review and transcription method

I rendered and visually checked the body pages covering printed pp. 76–90 (PDF pp. 77–91). This chunk contains 13 selected patterns, not every pattern on those pages. For each included item, I checked its title, printed notation or sequence, hand order, pass direction, and every arrow in the source diagram against its graph. Holds in the two asynchronous patterns are represented as explicit nodes without outgoing edges, matching the source's “hold” tokens. The visual review found no third-person manipulation in these ordinary two-person patterns. The two p. 83 siteswap graphs double the three-beat pattern into a six-beat cycle to preserve alternating hands. The p. 84 double-count graph preserves its one-beat phase, 4-beat double passes, 3-beat singles/self throws, and hand-specific pass routes. On printed p. 82, the three-count-on-doubles graph follows 966 / 4.5p33 and the stated 1.5-beat phase; the techno graph follows its simultaneous pass/self events, intervening half-beat self events, and one-beat phase for Ben. The four-count triple-self graph follows its printed siteswap and Prechac notation. The four-count-on-doubles diagram has alternating-hand events at half-beat spacing; its sequence and straight pass arrows are transcribed over a four-beat cycle. Every causal edge in these four records was checked against the represented throw height and cycle offset.

The fizz-buzz diagram shows one half of each pattern and says it repeats with the other hand; the graph therefore repeats each 15-token row with opposite-hand events to make a 30-beat cycle. The asynchronous brainstorm similarly doubles its 13-token rows to preserve alternating hands. Ben's half-beat phase is present in both patterns. The seven-club singles diagram has a two-beat repeating graph with Anne at integer beats and Ben offset by half a beat. The selected patterns use no unsupported negative zip/gap arrows.

The first draft cited “Seven club one count on singles” at printed p. 79. Visual page review caught that error: printed p. 79 is “Pass pass self vs one count”; the seven-club section starts at printed p. 80. Both the JSON source citation and this note use the corrected page.

Several seven-club site-swap inventory rows point to index/catalog pages rather than the body diagram pages. The new 779667 and 777966 charts are on printed p. 83 / PDF p. 84, and the (8x,6x)(6,8x)(8x,6) chart is on printed p. 84 / PDF p. 85; use those body citations when reconciling the inventory.

## Validation

A temporary validator constructed a `CausalDiagramEditorPreset` from each JSON record, called `Create()`, called `SynchronizeHeights()`, and compared the serialized cycle offsets before and after. Results:

- Jim's fizz buzz async: `Create()` succeeded; 60 nodes / 48 edges; offsets unchanged.
- Jim's bookends/brainstorm async: `Create()` succeeded; 52 nodes / 40 edges; offsets unchanged.
- Seven club one count on singles: `Create()` succeeded; 4 nodes / 4 edges; offsets unchanged.
- Seven club two count on singles: `Create()` succeeded; 4 nodes / 4 edges; offsets unchanged; temporary registered-ID validation returned `IsValidForEditor() == true`.
- 777786: 12 nodes / 12 edges; offsets unchanged; included in the standalone NUnit validity test.
- 777867: 12 nodes / 12 edges; offsets unchanged; included in the standalone NUnit validity test.

The standalone validator assigns the registered ID `highgate-three-count` so it can test the raw chunk before import. Its per-record loop checks `IsValidForEditor()` and unchanged offsets, validates the imported preset itself, and compares each source record with its runtime preset. All 13 records, including the four p. 82 patterns, are registered in the runtime catalog and pass the validity test.

## Known scope and limitations

This is a checked partial batch, not completion of printed pp. 76–90. The four independent non-manipulation patterns shown on printed p. 82 are now transcribed as separate source records; the rest of the page range has not been exhaustively inventoried here. The editor graph stores causal nodes and edges; it does not store the club-count starting inventory or the book's color-coded club identity. Those annotations are therefore not reproduced in these records. No included pattern required a model extension.

The slow-fast diagrams on printed pp. 78–79 are documented separately in [highgate-transcriptions-78-79.md](highgate-transcriptions-78-79.md). Three static graphs from this section are registered in the runtime preset list; two other slow-fast graphs still need a complete endpoint/tempo audit.

## Additional seven-club count graphs

I added `highgate-777786` and `highgate-777867` to the separate JSON chunk after transcribing their event cycles from the diagrams and per-juggler throw descriptions. The 2017 diagrams are on PDF pp. 84–85 (printed pp. 83–84); the 2014 PDF p. 53 reproduces the 777786 diagram and its text. The 777867 pattern is present in the 2017 edition, and I did not find an exact 2014 counterpart.

Each siteswap period has three beats, with Anne's events on integer beats and Ben's events a half beat later. I doubled that period to six beats because each juggler has an odd number of throws per three-beat repeat; doubling preserves the alternating right/left hand labels. The graph records all twelve event nodes and one causal edge per throw. For 777786, Anne's `pass, pass, heff` routes straight and Ben's `pass, pass, self` routes crossing. For 777867, Anne's sequence is `pass, pass, self`, while Ben's is `pass, heff, pass`; their pass directions are straight for Anne and crossing for Ben. The cycle offsets and resulting heights follow the printed siteswap values, and the drawn endpoints agree with the source diagrams.

A standalone NUnit test constructs every record in this chunk, synchronizes heights, checks that all serialized cycle offsets remain unchanged, and requires `IsValidForEditor()` to pass. It also compares the source nodes and edges with the imported editor preset for each record.
