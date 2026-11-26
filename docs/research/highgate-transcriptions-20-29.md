# Highgate transcription: printed pp. 20–29

## Completed in this batch

- `highgate-anearlydouble` — the early double diagram on 2017 expanded printed p. 20 (PDF p. 21), cross-checked against the matching 2014 diagram on printed/PDF p. 6. The 2014 text and diagram describe Ben's sequence as pass, self, double, hold, self, self, followed by normal three count.

The graph keeps the 12 alternating-hand event nodes for Anne and Ben. It has 11 causal edges: the usual three-count continuations except that Ben's right-hand self is replaced by the double pass into Anne's right-hand event. Ben's intervening left-hand event has no incoming or outgoing edge and is the explicit hold shown in the text. The double pass starts two beats before the destination's normal arrival time. The two wraparound edges preserve cycle offset +1.

The graph was checked edge by edge against both printed diagrams. A direct C# state check using the registered `highgate-three-count` ID as a validation placeholder returned `IsValidForEditor() == true` after `SynchronizeHeights()`; all cycle offsets remained unchanged. The new pattern itself is in [the separate JSON chunk](highgate-transcriptions-20-29.json) and has not been imported into the runtime catalog.

## Other entries in this page span

- Basic three count, five-club three count, late double, late single, self double, and French three count already have central presets.
- Early/late/very-late triple pages, self triple, seven-club three count, combination-trick variants, and the not-why examples need additional transcription or model review. The synchronous heff diagram requires same-time two-hand events, and the 441/4440 variants require backwards causal arrows. These remain open; this batch does not claim the full pp. 20–29 range is finished.
