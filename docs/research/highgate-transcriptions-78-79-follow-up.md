# Slow-fast follow-up source audit: printed pp. 78–79

The source-only transcriptions are in [highgate-transcriptions-78-79-follow-up.json](highgate-transcriptions-78-79-follow-up.json). They cover the two remaining static patterns from this section. I visually compared the 2017 arrows and action rows with the corresponding 2014 diagrams before assigning endpoints.

| Pattern | 2017 body source | 2014 body source | Lane schedule |
|---|---|---|---|
| `highgate-twocountvspasspassself` | printed p. 78 / PDF p. 79 | printed p. 50 / PDF p. 50 | Anne: eight events at 3/4-beat intervals; Ben: six at one-beat intervals; six-beat cycle |
| `highgate-passpassselfvsonecount` | printed p. 79 / PDF p. 80 | printed p. 50 / PDF p. 50 | Anne: six events at 2/3-beat intervals; Ben: four at one-beat intervals; four-beat cycle |

For two count versus pass-pass-self, the diagram arrows give Anne-to-Ben targets at lane indices 1, 2, 3, and 5 for Anne's pass events 0, 2, 4, and 6. Ben's pass events 0, 1, 3, and 4 target Anne indices 1, 2, 4, and 6. All self throws connect to the next event in the same lane. The 2014 and 2017 diagrams show the same pattern and landing nodes.

For pass-pass-self versus one count, Anne's pass events 0, 2, 3, and 5 target Ben indices 1, 2, 3, and 4. The final event lands in the next four-beat cycle. Ben's events 0–3 target Anne indices 1, 2, 4, and 5. Anne's self throws connect to her next event. The matching 2014 drawing has the same endpoints and phase spacing.

The permanent source test checks the event times, each destination lane and index, cycle offsets, offset preservation after `SynchronizeHeights()`, `IsValidForEditor()`, and exact equality with the imported runtime presets. The focused test passes. After CI for `ab69936c` completed successfully, both records were added to the runtime catalog and master inventory.
