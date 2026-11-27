# Highgate transcription: printed pp. 20–29

## Completed in this batch

- `highgate-anearlydouble` — the early double diagram on 2017 expanded printed p. 20 (PDF p. 21), cross-checked against the matching 2014 diagram on printed/PDF p. 6.
- `highgate-anearlytriple` — the early triple diagram on 2017 expanded printed p. 24 (PDF p. 25), cross-checked against the matching 2014 diagram on printed/PDF p. 10. Ben's sequence is pass, triple, hold, hold, self, self.
- `highgate-alatetriple` — the late triple diagram on the same pages. Ben's sequence is pass, self, triple, hold, self, self; Anne pauses before the triple arrives, then throws one self before her next pass.
- `highgate-averylatetriple` — the very late triple diagram on the same pages. Ben's pass beat is a triple; Anne holds for two beats, then catches it at her next pass event.

All three graphs keep 12 alternating-hand event nodes for Anne and Ben. The early double has 11 causal edges; Ben's intervening left-hand beat is a disconnected hold. The early triple has 10 edges and two disconnected Ben hold nodes. The late triple also has 10 edges: its route reaches Anne's event after three beats, replacing the self continuation that normally precedes her next pass. The very late triple has 10 edges too: Ben's usual first pass edge is retargeted from Anne's first left event to her next pass event, three beats later. Anne's two intervening events are disconnected holds. The usual three-count routes continue afterward, and wraparound edges preserve cycle offset +1.

I checked every edge against both editions' diagrams and their written throw sequences. Direct C# state checks using registered `highgate-three-count` as a validation placeholder returned `IsValidForEditor() == true` after `SynchronizeHeights()` for all four graphs; all cycle offsets remained unchanged. The patterns are in [the separate JSON chunk](highgate-transcriptions-20-29.json) and have not been imported into the runtime catalog.

## Other entries in this page span

- Basic three count, five-club three count, late double, late single, self double, and French three count already have central presets.
- Self triple, seven-club three count, combination-trick variants, and the not-why examples need additional transcription or model review. The synchronous heff diagram requires same-time two-hand events, and the 441/4440 variants require backwards causal arrows. These remain open; this batch does not claim the full pp. 20–29 range is finished.
