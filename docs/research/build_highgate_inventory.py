#!/usr/bin/env python3
"""Build a reviewable Highgate index inventory from both supplied PDF editions.

Requires Poppler's pdftotext and pdfimages utilities. The 2017 source PDF is
provided as the first command line argument; the 2014 PDF is bundled in the
repository. This script only extracts index/TOC metadata and evidence flags; it
does not create or modify editor presets.
"""

from __future__ import annotations

import csv
import hashlib
import re
import subprocess
import sys
import unicodedata
import xml.etree.ElementTree as ET
from collections import defaultdict
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
LOCAL_PDF = ROOT / "Siteswaps.Mcp.Server/Resources/highgate.pdf"
CURRENT_URL = "https://jugglingedge.com/pdf/highgate.pdf"
LOCAL_URL = "https://passing.zone/wp-content/uploads/highgate.pdf"
OUT = ROOT / "docs/research/highgate-pattern-inventory.csv"
NS = {"h": "http://www.w3.org/1999/xhtml"}


def run(*args: str) -> str:
    return subprocess.run(args, check=True, capture_output=True, text=True).stdout


def normalized(text: str) -> str:
    text = unicodedata.normalize("NFKD", text).casefold()
    return "".join(char for char in text if char.isalnum())


def stable_id(title: str) -> str:
    slug = normalized(title)
    if len(slug) > 64:
        slug = slug[:48] + "-" + hashlib.sha1(slug.encode()).hexdigest()[:10]
    return "highgate-" + slug


def bbox_pages(pdf: Path, first: int, last: int) -> list[ET.Element]:
    xml = run("pdftotext", "-bbox-layout", "-f", str(first), "-l", str(last), str(pdf), "-")
    return ET.fromstring(xml).findall(".//h:page", NS)


def line_values(page: ET.Element) -> list[tuple[float, str]]:
    result = []
    for line in page.findall(".//h:line", NS):
        words = line.findall("./h:word", NS)
        if words:
            result.append(
                (float(words[0].attrib["xMin"]), " ".join(word.text or "" for word in words).strip())
            )
    return result


def indexed_columns(page: ET.Element, split: bool) -> list[tuple[int, str]]:
    """Return text grouped by physical column, splitting a bbox line if needed."""
    grouped: list[tuple[int, str]] = []
    for line in page.findall(".//h:line", NS):
        words = line.findall("./h:word", NS)
        if not words:
            continue
        sides: dict[int, list[str]] = defaultdict(list)
        for word in words:
            side = 0 if not split or float(word.attrib["xMin"]) < 300 else 1
            sides[side].append(word.text or "")
        for side, values in sides.items():
            grouped.append((side, " ".join(values).strip()))
    return grouped


def page_refs(value: str) -> list[int]:
    refs: list[int] = []
    for part in re.split(r"\s*,\s*", value):
        match = re.fullmatch(r"\s*(\d+)\s*(?:[-\u2013]\s*(\d+))?\s*", part)
        if not match:
            continue
        start = int(match.group(1))
        end = int(match.group(2) or start)
        refs.extend(range(start, end + 1))
    return refs


def parse_reference_suffix(value: str) -> tuple[str, str] | None:
    """Split a 2014 index line into its title and numeric page suffix."""
    match = re.search(r"\s\d[\d\s,\u2013-]*$", value)
    if not match:
        return None
    title = value[: match.start()].strip()
    refs = match.group().strip()
    parts = re.split(r"\s*,\s*", refs)
    if not title or not all(re.fullmatch(r"\d+\s*(?:[-\u2013]\s*\d+)?", part) for part in parts):
        return None
    return title, refs


def extract_index(pdf: Path, first: int, last: int, edition: str) -> dict[str, dict[str, str]]:
    pages = bbox_pages(pdf, first, last)
    entries: dict[str, dict[str, str]] = {}
    partial = ""
    # Preserve page order and the two-column reading flow. A wrapped index title
    # can continue from the bottom of a right column onto the next page's left.
    for page in pages:
        page_columns: list[list[str]] = [[], []]
        for side, line in indexed_columns(page, edition == "2017"):
            if line in {"Index", "User-Defined Index"} or re.fullmatch(r"\d+", line):
                continue
            page_columns[side].append(line)
        for lines in page_columns:
            for line in lines:
                partial = f"{partial} {line}".strip()
                if edition == "2017":
                    separator = partial.rfind("..")
                    if separator < 0:
                        continue
                    title = partial[:separator].strip(" .")
                    refs = partial[separator + 2 :].strip()
                    parts = re.split(r"\s*,\s*", refs)
                    if not title or not all(re.fullmatch(r"\d+\s*(?:[-\u2013]\s*\d+)?", part) for part in parts):
                        continue
                else:
                    parsed = parse_reference_suffix(partial)
                    if not parsed:
                        continue
                    title, refs = parsed
                partial = ""
                if title.casefold() in {"index", "user-defined index"}:
                    continue
                key = normalized(title)
                if not key:
                    continue
                if key in entries:
                    old = entries[key]["printed_pages"]
                    entries[key]["printed_pages"] = ", ".join(dict.fromkeys([old, refs]))
                else:
                    entries[key] = {"title": title, "printed_pages": refs}
    if partial:
        print(f"Unparsed {edition} index tail: {partial}", file=sys.stderr)
    return entries


def extract_toc(pdf: Path, first: int, last: int, edition: str) -> list[tuple[int, str]]:
    rows: list[tuple[int, float, str]] = []
    for page in bbox_pages(pdf, first, last):
        for x, line in line_values(page):
            if line in {"Table of Contents", "Index"}:
                continue
            match = re.fullmatch(r"(.+?)\.{2,}\s*(\d+)\s*", line)
            if match:
                rows.append((int(match.group(2)), x, match.group(1).strip().rstrip(".")))
    rows.sort(key=lambda item: item[0])
    if edition == "2014":
        return [(page, title) for page, _, title in rows]
    result: list[tuple[int, str]] = []
    chapter = ""
    for page, x, title in rows:
        if x < 65:
            chapter = title
        result.append((page, chapter or title))
    return result


def extract_toc_candidates(pdf: Path, first: int, last: int) -> dict[str, dict[str, str]]:
    rows: list[tuple[int, float, str]] = []
    for page in bbox_pages(pdf, first, last):
        for x, line in line_values(page):
            if line == "Table of Contents":
                continue
            match = re.fullmatch(r"(.+?)\.{2,}\s*(\d+)\s*", line)
            if match:
                rows.append((int(match.group(2)), x, match.group(1).strip().rstrip(".")))
    rows.sort(key=lambda item: item[0])
    result: dict[str, dict[str, str]] = {}
    chapter = ""
    for page, x, title in rows:
        if x < 65:
            chapter = title
        key = normalized(title)
        if key not in result:
            result[key] = {"title": title, "printed_pages": str(page), "chapter": chapter or title}
        else:
            previous = result[key]["printed_pages"]
            result[key]["printed_pages"] = ", ".join(dict.fromkeys([previous, str(page)]))
    return result


def chapter_for(page: int, toc: list[tuple[int, str]]) -> str:
    matches = [title for start, title in toc if start <= page]
    return matches[-1] if matches else "Unclassified by contents"


def pdf_text_pages(pdf: Path, page_count: int, offset: int) -> dict[int, str]:
    # Printed page -> physical PDF page is `printed + offset`.
    return {
        printed: run("pdftotext", "-layout", "-f", str(printed + offset), "-l", str(printed + offset), str(pdf), "-")
        for printed in range(1, page_count + 1)
    }


def image_pages(pdf: Path) -> set[int]:
    lines = run("pdfimages", "-list", str(pdf)).splitlines()[2:]
    result = set()
    for line in lines:
        match = re.match(r"\s*(\d+)\s+\d+\s+image\b", line)
        if match:
            result.add(int(match.group(1)))
    return result


def evidence(edition: str, printed_pages: str, text_by_page: dict[int, str], images: set[int]) -> tuple[str, str, str, str, str, str]:
    printed = page_refs(printed_pages)
    physical_offset = 0 if edition == "2014" else 1
    texts = [text_by_page[p] for p in printed if p in text_by_page]
    text = "\n".join(texts)
    physical_pages = [p + physical_offset for p in printed]
    page_images = "yes" if any(page in images for page in physical_pages) else "no"
    # The page may contain a photo or other artwork; image presence alone does not
    # establish a causal diagram. A human must inspect the page before this is yes/no.
    causal_diagram = "unclear"
    sequence_markers = re.search(
        r"\b(?:siteswap|prechac)\s*:|\bpattern for\b|\bstarting pattern for\b|\bstart(?:ing)? pattern\s*:",
        text,
        re.IGNORECASE,
    )
    sequence_text = "yes" if sequence_markers else "no"
    sequence_data = "yes" if sequence_markers else "unclear"
    low = text.casefold()
    tags = []
    for needle, label in [
        (r"\bwalk(?:s|ed|ing)?\b|runaround|figure of eight|formation", "movement/walking"),
        (r"\bholds?\b", "holds"),
        (r"\bzips?\b|hand.?across", "zips/hand-across"),
        (r"\bsteal(?:s|ing|en)?\b|manipulator", "manipulator action"),
        (r"hijack|lo.?jack|programming", "pattern-transition manipulation"),
    ]:
        if re.search(needle, low):
            tags.append(label)
    if any(tag == "pattern-transition manipulation" for tag in tags):
        manipulation = "manipulation"
        kind = "pattern transition / hijack"
    elif "manipulator action" in tags:
        manipulation = "unclear"
        kind = "possible external manipulator is used to enter/describe the pattern"
    elif sequence_data == "yes":
        manipulation = "not_detected_in_extracted_text"
        kind = "explicit siteswap/Prechac/throw sequence; no manipulator signal in extracted text (not visually reviewed)"
    else:
        manipulation = "unclear"
        kind = "needs visual/context review"
    if any(tag in tags for tag in ("movement/walking", "holds", "zips/hand-across", "manipulator action")):
        representable = "no"
    elif sequence_data == "yes":
        representable = "yes"
    else:
        representable = "unclear"
    return manipulation, page_images, causal_diagram, sequence_data, sequence_text, "; ".join([kind, "model candidate: " + representable, "requirements: " + (", ".join(tags) or "none detected")])


def edition_pages(entry: dict[str, str] | None) -> tuple[str, str]:
    if not entry:
        return "", ""
    printed = entry["printed_pages"]
    physical = []
    for page in page_refs(printed):
        physical.append(page + (1 if entry.get("edition") == "2017" else 0))
    # Keep ranges readable when they are contiguous.
    return printed, ", ".join(map(str, physical))


def sha(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main() -> None:
    if len(sys.argv) != 2:
        raise SystemExit("Usage: build_highgate_inventory.py /path/to/2017-highgate.pdf")
    current_pdf = Path(sys.argv[1]).resolve()
    local = extract_index(LOCAL_PDF, 239, 246, "2014")
    local_toc_entries = extract_toc_candidates(LOCAL_PDF, 2, 3)
    local_toc_only = {
        key: value for key, value in local_toc_entries.items()
        if key not in local and value["title"].casefold() != "index"
    }
    for key, value in local_toc_only.items():
        local[key] = {
            "title": value["title"],
            "printed_pages": value["printed_pages"],
            "catalog_source": "TOC only",
            "chapter": value["chapter"],
        }
    for value in local.values():
        value.setdefault("catalog_source", "Index")
    for key, value in local.items():
        value["index_printed_pages"] = value["printed_pages"] if value["catalog_source"] == "Index" else ""
        toc_entry = local_toc_entries.get(key)
        value["toc_printed_pages"] = toc_entry["printed_pages"] if toc_entry else ""
        if toc_entry and value["catalog_source"] == "Index":
            value["catalog_source"] = "Index;TOC"
        value["printed_pages"] = value["index_printed_pages"] or value["toc_printed_pages"]
    current = extract_index(current_pdf, 332, 340, "2017")
    current_toc_candidates = extract_toc_candidates(current_pdf, 3, 17)
    toc_only = {key: value for key, value in current_toc_candidates.items() if key not in current}
    for key, value in toc_only.items():
        current[key] = {
            "title": value["title"],
            "printed_pages": value["printed_pages"],
            "catalog_source": "TOC only",
            "chapter": value["chapter"],
        }
    for value in current.values():
        value.setdefault("catalog_source", "Index")
    for key, value in current.items():
        value["index_printed_pages"] = value["printed_pages"] if value["catalog_source"] == "Index" else ""
        toc_entry = current_toc_candidates.get(key)
        value["toc_printed_pages"] = toc_entry["printed_pages"] if toc_entry else ""
        if toc_entry and value["catalog_source"] == "Index":
            value["catalog_source"] = "Index;TOC"
        value["printed_pages"] = value["index_printed_pages"] or value["toc_printed_pages"]
    local_toc = extract_toc(LOCAL_PDF, 2, 3, "2014")
    current_toc = extract_toc(current_pdf, 3, 17, "2017")
    print(f"candidate counts: local-index={len(local) - len(local_toc_only)} local-toc-only={len(local_toc_only)} current-index={len(current) - len(toc_only)} current-toc-only={len(toc_only)} union={len(local.keys() | current.keys())} exact-shared={len(local.keys() & current.keys())}")

    local_text = pdf_text_pages(LOCAL_PDF, 246, 0)
    current_text = pdf_text_pages(current_pdf, 339, 1)
    local_images, current_images = image_pages(LOCAL_PDF), image_pages(current_pdf)
    local_sha, current_sha = sha(LOCAL_PDF), sha(current_pdf)
    all_keys = sorted(local.keys() | current.keys())
    OUT.parent.mkdir(parents=True, exist_ok=True)
    columns = [
        "stable_id", "title_2017", "title_2014", "entry_kind", "2017_catalog_source", "2014_catalog_source", "edition_change_status",
        "chapter_2017", "2017_index_printed_pages", "2017_toc_printed_pages", "2017_pdf_pages", "2017_source",
        "chapter_2014", "2014_index_printed_pages", "2014_toc_printed_pages", "2014_pdf_pages", "2014_source",
        "manipulation_class_2017", "page_images_present_2017", "causal_diagram_data_status_2017", "sequence_data_status_2017", "sequence_text_evidence_2017", "model_requirements_2017",
        "manipulation_class_2014", "page_images_present_2014", "causal_diagram_data_status_2014", "sequence_data_status_2014", "sequence_text_evidence_2014", "model_requirements_2014",
        "visual_review_status", "processing_status",
    ]
    with OUT.open("w", newline="", encoding="utf-8") as file:
        writer = csv.DictWriter(file, fieldnames=columns)
        writer.writeheader()
        for key in all_keys:
            old, new = local.get(key), current.get(key)
            title = (new or old)["title"]
            old_pages = old["printed_pages"] if old else ""
            new_pages = new["printed_pages"] if new else ""
            if old and new:
                if new["catalog_source"] == "TOC only":
                    change = "present in 2017 TOC only; no current index entry"
                elif old_pages == new_pages and old["catalog_source"] == "Index":
                    change = "same index title and page references"
                else:
                    change = "title/page reference differs across edition indexes; mechanics not compared"
            elif new:
                change = "candidate title only in 2017 index/TOC; mechanics/aliases not compared"
            else:
                change = "candidate title only in 2014 index/TOC; absent/renamed/split in 2017 not confirmed"
            if new:
                refs = page_refs(new_pages)
                chap = new.get("chapter") or current_toc_candidates.get(key, {}).get("chapter") or (chapter_for(min(refs), current_toc) if refs else "")
                manipulation, page_images, causal_diagram, sequence, sequence_text, requirements = evidence("2017", new_pages, current_text, current_images)
                base = dict(
                    title_2017=new["title"], chapter_2017=chap,
                    **{"2017_index_printed_pages": new["index_printed_pages"], "2017_toc_printed_pages": new["toc_printed_pages"], "2017_pdf_pages": ", ".join(str(p + 1) for p in refs)},
                    **{"2017_catalog_source": new["catalog_source"]},
                    manipulation_class_2017=manipulation, page_images_present_2017=page_images,
                    causal_diagram_data_status_2017=causal_diagram, sequence_data_status_2017=sequence,
                    sequence_text_evidence_2017=sequence_text,
                    model_requirements_2017=requirements,
                )
            else:
                base = dict(title_2017="", chapter_2017="", **{"2017_index_printed_pages": "", "2017_toc_printed_pages": "", "2017_pdf_pages": "", "2017_catalog_source": "not present"}, manipulation_class_2017="unclear", page_images_present_2017="unclear", causal_diagram_data_status_2017="unclear", sequence_data_status_2017="unclear", sequence_text_evidence_2017="unclear", model_requirements_2017="needs source review")
            if old:
                refs = page_refs(old_pages)
                chap = old.get("chapter") or (old["title"] if old["catalog_source"] == "TOC only" else chapter_for(min(refs), local_toc) if refs else "")
                manipulation, page_images, causal_diagram, sequence, sequence_text, requirements = evidence("2014", old_pages, local_text, local_images)
                base.update(title_2014=old["title"], chapter_2014=chap,
                    **{"2014_index_printed_pages": old["index_printed_pages"], "2014_toc_printed_pages": old["toc_printed_pages"], "2014_pdf_pages": ", ".join(map(str, refs))},
                    **{"2014_catalog_source": old["catalog_source"]},
                    manipulation_class_2014=manipulation, page_images_present_2014=page_images,
                    causal_diagram_data_status_2014=causal_diagram, sequence_data_status_2014=sequence,
                    sequence_text_evidence_2014=sequence_text,
                    model_requirements_2014=requirements)
            else:
                base.update(title_2014="", chapter_2014="", **{"2014_index_printed_pages": "", "2014_toc_printed_pages": "", "2014_pdf_pages": ""},
                    **{"2014_catalog_source": "not present"}, manipulation_class_2014="unclear", page_images_present_2014="unclear", causal_diagram_data_status_2014="unclear", sequence_data_status_2014="unclear", sequence_text_evidence_2014="unclear", model_requirements_2014="not present in this index")
            title_lower = title.casefold()
            if title_lower in {"passing etiquette", "colour coding patterns", "colour coding", "causal diagrams", "four handed siteswaps", "prechac notation", "brolly notation", "basic patterns", "how to work out hijacks in period five patterns"}:
                kind = "concept/instruction index entry"
            elif re.search(r"\b(patterns|feeds|feasts|triangles|tricks|runarounds|weaves|siteswaps)\b", title_lower) and not re.search(r"\b(?:feed|feast|triangle|trick|runaround|weave|siteswap)\b\s*\([^)]*\)$", title_lower):
                kind = "family/section candidate; confirm whether it expands to individual patterns"
            else:
                kind = "individual pattern or named variant; verify aliases"
            writer.writerow(dict(stable_id=stable_id(title), edition_change_status=change, visual_review_status="pending", processing_status="open", **base,
                entry_kind=kind,
                **{"2017_source": f"{CURRENT_URL}; Aidan Burns; CreationDate 2017-10-09; 340 PDF pages; SHA256 {current_sha}", "2014_source": f"{LOCAL_URL}; bundled Siteswaps.Mcp.Server/Resources/highgate.pdf; Aidan Burns; CreationDate 2014-05-16; 246 PDF pages; SHA256 {local_sha}"}))


if __name__ == "__main__":
    main()
