# Highgate Collection pattern inventory

**Working list:** [CSV inventory](highgate-pattern-inventory.csv)
**Rebuild script:** [Inventory extractor](build_highgate_inventory.py)
**Independent source audit:** [Index and edition audit](highgate-pattern-inventory-audit.md)

## Scope and source editions

The inventory combines named candidates from the detailed table of contents and alphabetical index of both available Highgate PDFs. It keeps patterns, aliases, section/family headings, and cross references as separate review candidates; it does not claim that every row is a distinct pattern mechanic.

| Edition | Source | PDF details | Page numbering |
| --- | --- | --- | --- |
| 2014 bundled copy | [passing.zone PDF](https://passing.zone/wp-content/uploads/highgate.pdf); same bytes as `Siteswaps.Mcp.Server/Resources/highgate.pdf` | Aidan Burns; PDF metadata creation date 2014-05-16; 246 pages; SHA-256 `ce454cbc9059dc5f332085638d34aaa45b69afb4bbc298e0f3f86a3feab591c8` | Printed page equals physical PDF page |
| Expanded 2017 copy | [Juggling Edge PDF](https://jugglingedge.com/pdf/highgate.pdf) | Aidan Burns; LibreOffice 5.1; metadata creation date 2017-10-09; server Last-Modified 2017-10-09; 340 physical pages; SHA-256 `51778649f1889fd5bff6e377ae46e63818df7be8543adf787d9e19f636521ab2` | One unnumbered cover page: physical PDF page is printed page + 1 |

The `passing.zone` PDF supplied by the user, including its `/2016/01/` URL, is byte-identical to the bundled 2014 copy. It is not the expanded Juggling Edge copy.

## Coverage and reconciliation

| Source candidates | 2014 | 2017 |
| --- | ---: | ---: |
| Index titles after exact-title normalization | 430 | 645 |
| Detailed TOC title lines | 65 | 657 |
| Distinct TOC titles | 65 | 652 |
| TOC titles absent from that edition's index | 59 | 14 |
| Combined edition candidates | 489 | 659 |

The combined CSV has **919 rows** after joining both edition candidate sets by normalized title. The join has **229 exact normalized-title matches**. These counts are title-candidate counts, not unique pattern-mechanic counts. The difference between editions can mean a new title, a split/renamed entry, a different index alias, or changed page references; the CSV labels these as unconfirmed until their source pages are compared.

The 2017 TOC was cross-checked against the 2017 index; every one of the 14 TOC-only titles is retained as a candidate instead of being silently dropped. The same check added the 59 2014 TOC headings that have no local index entry. Section headings and families are tagged in `entry_kind` so they can be separated from individual-pattern rows during later review.

The TOC/index reconciliation covers titled entries. It does **not** prove that the books contain no unnamed example patterns embedded in prose or diagrams. Those examples need to be captured during page-by-page pattern review; do not treat the candidate count as a count of every drawable pattern.

## Extraction and stable IDs

The reproducible script uses Poppler `pdftotext -bbox-layout` to retain index/TOC columns and page references, `pdftotext -layout` to look for explicit notation/throw rows, and `pdfimages -list` to record whether a cited page contains an embedded image. The current index is two-column; its entries may wrap from a right column to the next page's left column, so extraction follows page and column flow. We also retained the complete index-only audit as a human-readable cross-check.

Stable IDs are `highgate-` plus a title normalized by Unicode case-folding and removing punctuation/spacing. Both edition titles and their index/TOC printed page references remain separate fields. For the 2017 edition, the CSV also lists physical PDF page references; those are one greater than the printed page references.

## Field meanings and limits

- `2017_catalog_source` / `2014_catalog_source`: `Index`, `TOC only`, or `Index;TOC`. The index and TOC page references are stored separately.
- `edition_change_status`: compares titles and references only. A changed page citation does not mean the pattern itself changed. Added/absent labels are candidates needing alias/mechanics review, not claims about new/lost patterns.
- `entry_kind`: a first-pass tag for individual titles, families, and concept/instruction entries. Aliases and whether a family expands to individual patterns still need review.
- `manipulation_class_*`: `manipulation` is assigned only when extracted source text explicitly describes hijacking/lo-jacking/programming; `unclear` is used for ambiguous manipulator language or no usable sequence evidence; `not_detected_in_extracted_text` means the text contains a direct sequence/notation and no manipulation signal was found. That last value is **not** a final `normal` classification. `visual_review_status` is `pending` for every row.
- `page_images_present_*`: whether Poppler finds an embedded image on a cited PDF page. It may be a diagram, photo, or other artwork.
- `causal_diagram_data_status_*`: remains `unclear` until a person visually confirms that the cited page contains a causal diagram. Image presence alone is not proof.
- `sequence_data_status_*`: `yes` when extracted text contains a siteswap, Prechac notation, or explicit per-juggler throw sequence; otherwise `unclear` because the sequence may be in a graphic. `sequence_text_evidence_*` records only the text-extraction result.
- `model_requirements_*`: preliminary source-text flags such as movement/walking, holds, zips, and manipulator actions. They mark model-review work; they do not exclude those patterns from the desired list.
- `processing_status=open`: the candidate has not yet been transcribed into the causal editor and validated there.

The book pages still need visual review for all `unclear` fields, including detection of non-raster/vector diagrams and sequence notation rendered in graphics. Later translation should use the source page, not infer a causal graph from a title or an image-presence flag.
