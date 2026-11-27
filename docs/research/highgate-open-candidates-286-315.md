# Open Highgate candidates around printed pp. 286–315

This is a source crosswalk for inventory rows still marked `open`; it does not
change the inventory or runtime preset catalog. The page values in the
inventory are often from an index or contents list, rather than the body page.
Both the 2017 expanded PDF and 2014 PDF were checked.

| Open inventory candidate | Body source | Finding |
| --- | --- | --- |
| `highgate-fourpersonbenzenering` | 2017 printed p. 282 / PDF p. 283; 2014 PDF p. 199 | The body diagram is already transcribed and registered as `highgate-benzenering`. The open row is a title/index candidate for this same diagram, not a second graph. Resolve as an alias or duplicate in the inventory. |
| `highgate-havanafeed` | 2017 printed p. 288 / PDF p. 289; 2014 PDF p. 205 | Genuine missing graph. The page has 16 numbered formation/pass panels, describes a five-pass feeder change, and gives a final-to-first role permutation. It explicitly says the diagram does not show the whole pattern. Exact event timing and the complete role-expanded cycle are not yet reconstructed, so no graph is guessed. |
| `highgate-interlockingweaves` | 2017 printed p. 289 / PDF p. 290; 2014 PDF p. 206 | Genuine missing graph. The page has 16 numbered formation/pass panels and prose describing a four-pass feeder change, a mirrored feed, and role swaps. Unlike Havana, it does not label the panels as an incomplete diagram, but it gives no written throw rows or causal-diagram heading. The full event cadence and cycle closure need to be reconstructed before importing. |
| `highgate-thirteenclubperiodfivefeeds` | Section starts 2017 printed p. 304; 2014 pp. 218–221 | Section, not a standalone pattern. The individual period-five feeds are transcribed in `highgate-transcriptions-301-315.json`, including the nine additions cross-checked against 2014. |
| `highgate-thirteenclubboxfeeds` | Section starts 2017 printed p. 310; 2014 p. 221 onward | Section, not a standalone pattern. The four individual box patterns are present in `highgate-transcriptions-301-315.json`. |
| `highgate-thirteenclublinefeeds` | Section starts 2017 printed p. 313; 2014 p. 224 onward | Section, not a standalone pattern. The two patterns on 2017 pp. 313–314 and the three patterns beginning on p. 315 are present in `highgate-transcriptions-301-315.json`. Later line-feed entries continue past p. 315 and are outside this crosswalk. |
| `highgate-thirteenclubfourpersonfeeds` | Section starts 2017 printed p. 295; 2014 p. 212 | Section, not a standalone pattern. The individual feed records are in `highgate-transcriptions-286-300.json`. |
| `highgate-thirteenclubonecountfeedingcountdown305thirteenc-9dcb228c44` | 2017 printed pp. 295–300; 2014 pp. 212–215 | This joined inventory row names both the one-count countdown and one-count four-count feeds. Both are represented in the chunk/catalog as `highgate-thirteenclubonecountfeedingcountdown` and `highgate-thirteenclubonecountfeedingfourcount`. Split the inventory row rather than adding another graph. |
| `highgate-thirteenclubonecountfeedingthreecount301thirteen-2cb9bdf7ac` | 2017 printed p. 297; 2014 p. 213 | This joined inventory row names the three-count and two-count feeds. Both are represented as `highgate-thirteenclubonecountfeedingthreecount` and `highgate-thirteenclubonecountfeedingtwocount`. Split the inventory row rather than adding duplicates. |

## Remaining work

The immediate unresolved source graphs in this range are Havana Feed and
Interlocking Weaves. Their formation arrows and walking paths are not enough on
their own to assert a complete causal cycle. A source-page reconstruction must
establish each throw event, its timing/hand, and the cycle boundary before a
standalone record can safely be added.
