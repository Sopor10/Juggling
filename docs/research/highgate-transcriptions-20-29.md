# Highgate transcription: printed pp. 20–29

## Completed in this batch

- `highgate-anearlydouble` — the early double diagram on 2017 expanded printed p. 20 (PDF p. 21), cross-checked against the matching 2014 diagram on printed/PDF p. 6.
- `highgate-anearlytriple` — the early triple diagram on 2017 expanded printed p. 24 (PDF p. 25), cross-checked against the matching 2014 diagram on printed/PDF p. 10. Ben's sequence is pass, triple, hold, hold, self, self.

Both graphs keep 12 alternating-hand event nodes for Anne and Ben. The early double has 11 causal edges: the usual three-count continuations except that Ben's right-hand self is replaced by the double pass into Anne's right-hand event. Ben's intervening left-hand beat is a disconnected hold. The early triple has 10 causal edges: Ben's next throw is the triple pass into Anne's usual right-hand event; his following two beat nodes are disconnected holds. In both graphs, the usual three-count routes continue afterward, and wraparound edges preserve cycle offset +1.

I checked every edge against both editions' diagrams and their written throw sequences. Direct C# state checks using registered `highgate-three-count` as a validation placeholder returned `IsValidForEditor() == true` after `SynchronizeHeights()` for both graphs; all cycle offsets remained unchanged. Both patterns are in [the separate JSON chunk](highgate-transcriptions-20-29.json) and have not been imported into the runtime catalog.

## Other entries in this page span

- Basic three count, five-club three count, late double, late single, self double, and French three count already have central presets.
- Late/very-late triple pages, self triple, seven-club three count, combination-trick variants, and the not-why examples need additional transcription or model review. The synchronous heff diagram requires same-time two-hand events, and the 441/4440 variants require backwards causal arrows. These remain open; this batch does not claim the full pp. 20–29 range is finished.
