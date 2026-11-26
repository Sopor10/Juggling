# Highgate open diagram audit: pp. 25–30

This audit records close source review of ordinary, non-manipulation patterns
that were previously left open. It intentionally contains no guessed causal
edges and does not change the runtime catalog or inventory status.

## `highgate-doubleself53` — Double self (53)

- **2017 source:** expanded PDF p. 29, printed p. 28.
- **2014 source:** PDF p. 14.
- **Cross-edition check:** diagram raster extracted from each PDF is byte-identical,
  SHA-256 `aad907fde1580ef38fe789f88c3e76a9e58a4fdead891f1b64cbc5c63da39a74`.
- **Text verified against both editions:** the section identifies Brolly's 53;
  the prose defines 5 as a crossing double pass and 3 as an ordinary self.
  The figure caption prose specifies Ben's right-hand crossing double pass,
  then left-hand self, while Anne pauses and omits the left-hand pass.
- **Diagram extraction:** the 2017 page SVG contains this as an embedded 370×52
  PNG, not vector paths. The raster contains repeated alternating R/L event
  nodes and crossing lines, but no arrowheads, beat marks, or cycle boundary.
  The prose supplies the named throw sequence and the missing Anne pass, but
  does not name which repeated occurrence of the otherwise cyclic hand node is
  the displayed cycle's origin. As a result, exact node IDs and wrap offsets
  cannot be assigned from the source alone without choosing an origin. The
  diagram may admit a cyclically equivalent graph, but no canonical source
  endpoint transcription is claimed here.

## `highgate-triplepasshold642` — Triple pass hold (642)

- **2017 source:** expanded PDF p. 29, printed p. 28.
- **2014 source:** PDF p. 14.
- **Cross-edition check:** diagram raster extracted from each PDF is byte-identical,
  SHA-256 `8afd398ff38f669e26b433f0874d6721d7e62a2095713a5f3a966f86d66dab73`.
- **Text verified against both editions:** 642 is described as a straight
  triple pass followed by a hold. The figure prose specifies Ben's right-hand
  straight triple pass, left-hand single pass, then right-hand hold; Anne pauses.
  It also says the single pass is caught before the triple, despite being thrown
  afterward.
- **Diagram extraction:** the SVG embeds this as a 428×52 PNG, not vector paths.
  The line drawing has repeated R/L nodes and intersecting routes, but no
  arrowheads, beat labels, or explicit cycle boundary. The prose identifies
  Ben's three actions and their order, but does not state the exact catch-node
  index or wrap offset for the delayed triple relative to the single. Therefore
  the two arrivals cannot be attached to canonical editor node IDs from the
  text alone. No endpoints are inferred from visual crossings.

## Other previously reviewed patterns

### `highgate-combinationtricks`

- 2017 expanded printed p. 22 / PDF p. 23; 2014 PDF p. 8.
- Both editions contain the same two raster causal diagrams. The prose names
  the first as Ben's late double followed by a heff/self-double and the second
  as late double followed by early double. The first diagram's additional
  self-double route has not been mapped to an exact pair of node IDs; no graph
  record is made here.

### `highgate-sevenclubthreecount`

- 2017 expanded printed p. 25 / PDF p. 26; 2014 PDF p. 11.
- Both editions agree: seven clubs, three-count with floaty double passes,
  Ben starts first and Anne starts 1.5 beats later; Anne's double passes cross
  and Ben's are straight. Individual arrow endpoints and cycle offsets remain
  untranscribed; no graph record is made here.

## Commands used to verify source material

```sh
pdftotext -f 29 -l 29 -layout /tmp/highgate-2017.pdf -
pdftotext -f 14 -l 14 -layout Siteswaps.Mcp.Server/Resources/highgate.pdf -
pdftocairo -f 29 -l 29 -svg /tmp/highgate-2017.pdf /tmp/hg29.svg
pdftocairo -f 14 -l 14 -svg Siteswaps.Mcp.Server/Resources/highgate.pdf /tmp/hg14p14.svg
sha256sum /tmp/hg29-0.png /tmp/hg14p14-0.png /tmp/hg29-1.png /tmp/hg14p14-1.png
```

No C# graph validation was run because no complete graph was transcribed.
