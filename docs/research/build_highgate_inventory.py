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
INDEX_HEADERS = {"Index", "User-Defined Index"}
INDEX_HEADERS_CASEFOLDED = {header.casefold() for header in INDEX_HEADERS}
TOC_HEADERS = {"Table of Contents", "Index"}
TOC_ONLY = "TOC only"
INDEX_SOURCE = "Index"
NOT_PRESENT = "not present"
UNCLEAR = "unclear"
YES = "yes"
NO = "no"
EMPTY = ""
SPACE = " "
COMMA_SPACE = ", "
HYPHEN = "-"
TITLE_FIELD = "title"
PRINTED_PAGES_FIELD = "printed_pages"
CATALOG_SOURCE_FIELD = "catalog_source"
CHAPTER_FIELD = "chapter"
INDEX_PAGES_FIELD = "index_printed_pages"
TOC_PAGES_FIELD = "toc_printed_pages"
EDITION_2017 = "2017"
EDITION_2014 = "2014"
MANIPULATION = "manipulation"
MANIPULATOR_ACTION = "manipulator action"


def is_digit(char: str) -> bool:
    return "0" <= char <= "9"


def parse_digits(value: str, index: int) -> tuple[int, int] | None:
    """Read a decimal integer starting at index; return its end position."""
    start = index
    while index < len(value) and is_digit(value[index]):
        index += 1
    if index == start:
        return None
    return int(value[start:index]), index


def parse_digits_backwards(value: str, end: int) -> tuple[int, int] | None:
    start = end
    while start > 0 and is_digit(value[start - 1]):
        start -= 1
    if start == end:
        return None
    return int(value[start:end]), start


def parse_page_reference_part(value: str) -> tuple[int, int] | None:
    """Parse one page number or range without a regular expression."""
    text = value.strip()
    parsed_start = parse_digits(text, 0)
    if parsed_start is None:
        return None
    start, index = parsed_start
    while index < len(text) and text[index].isspace():
        index += 1
    if index == len(text):
        return start, start
    if text[index] not in "-–":
        return None
    index += 1
    while index < len(text) and text[index].isspace():
        index += 1
    parsed_end = parse_digits(text, index)
    if parsed_end is None:
        return None
    end, index = parsed_end
    while index < len(text) and text[index].isspace():
        index += 1
    return (start, end) if index == len(text) else None


def is_page_reference_list(value: str) -> bool:
    parts = value.split(",")
    return bool(parts) and all(parse_page_reference_part(part) for part in parts)


def dotted_reference_suffix(value: str) -> tuple[str, str] | None:
    """Split a dotted title/page line at its final `..` separator."""
    line = value.rstrip()
    page_start = len(line)
    while page_start > 0 and is_digit(line[page_start - 1]):
        page_start -= 1
    if page_start == len(line):
        return None
    separator_end = page_start
    while separator_end > 0 and line[separator_end - 1].isspace():
        separator_end -= 1
    dots_start = separator_end
    while dots_start > 0 and line[dots_start - 1] == ".":
        dots_start -= 1
    if separator_end - dots_start < 2:
        return None
    title = line[:dots_start].strip().rstrip(".")
    page = line[page_start:].strip()
    if not title or not parse_page_reference_part(page):
        return None
    return title, page


def text_words(value: str) -> list[str]:
    words: list[str] = []
    current: list[str] = []
    for char in value.casefold():
        if char.isalnum():
            current.append(char)
        elif current:
            words.append("".join(current))
            current = []
    if current:
        words.append("".join(current))
    return words


def has_suffix_parenthesis_after_word(title: str, candidates: set[str]) -> bool:
    lowered = title.casefold()
    for candidate in candidates:
        position = lowered.find(candidate)
        while position >= 0:
            end = position + len(candidate)
            before = lowered[position - 1] if position else SPACE
            after = lowered[end] if end < len(lowered) else SPACE
            suffix = lowered[end:].lstrip()
            if (
                not before.isalnum()
                and not after.isalnum()
                and suffix.startswith("(")
                and lowered.rstrip().endswith(")")
            ):
                return True
            position = lowered.find(candidate, position + 1)
    return False


def run(*args: str) -> str:
    return subprocess.run(args, check=True, capture_output=True, text=True).stdout


def normalized(text: str) -> str:
    text = unicodedata.normalize("NFKD", text).casefold()
    return EMPTY.join(char for char in text if char.isalnum())


def stable_id(title: str) -> str:
    slug = normalized(title)
    if len(slug) > 64:
        slug = slug[:48] + HYPHEN + hashlib.sha1(slug.encode()).hexdigest()[:10]
    return "highgate-" + slug


def bbox_pages(pdf: Path, first: int, last: int) -> list[ET.Element]:
    xml = run("pdftotext", "-bbox-layout", "-f", str(first), "-l", str(last), str(pdf), HYPHEN)
    return ET.fromstring(xml).findall(".//h:page", NS)


def line_values(page: ET.Element) -> list[tuple[float, str]]:
    result = []
    for line in page.findall(".//h:line", NS):
        words = line.findall("./h:word", NS)
        if words:
            result.append(
                (float(words[0].attrib["xMin"]), SPACE.join(word.text or EMPTY for word in words).strip())
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
            sides[side].append(word.text or EMPTY)
        for side, values in sides.items():
            grouped.append((side, SPACE.join(values).strip()))
    return grouped


def page_refs(value: str) -> list[int]:
    refs: list[int] = []
    for part in value.split(","):
        parsed = parse_page_reference_part(part)
        if parsed is None:
            continue
        start, end = parsed
        refs.extend(range(start, end + 1))
    return refs


def parse_reference_suffix(value: str) -> tuple[str, str] | None:
    """Split a 2014 index line into its title and numeric page suffix."""
    line = value.rstrip()
    refs_start = find_reference_suffix_start(line)
    if refs_start is None:
        return None
    boundary = skip_spaces_backwards(line, refs_start)
    title = line[:boundary].strip()
    refs = line[refs_start:].strip()
    if not title or not is_page_reference_list(refs):
        return None
    return title, refs


def skip_spaces_backwards(value: str, position: int) -> int:
    while position > 0 and value[position - 1].isspace():
        position -= 1
    return position


def find_reference_suffix_start(line: str) -> int | None:
    position = len(line)
    while True:
        position = skip_spaces_backwards(line, position)
        final_page = parse_digits_backwards(line, position)
        if final_page is None:
            return None
        _, number_start = final_page
        position = number_start
        position = skip_spaces_backwards(line, position)
        if position > 0 and line[position - 1] in "-–":
            position -= 1
            position = skip_spaces_backwards(line, position)
            first_page = parse_digits_backwards(line, position)
            if first_page is None:
                return None
            _, position = first_page
        refs_start = position
        position = skip_spaces_backwards(line, position)
        if position > 0 and line[position - 1] == ",":
            position -= 1
            continue
        return refs_start


def extract_index(pdf: Path, first: int, last: int, edition: str) -> dict[str, dict[str, str]]:
    entries: dict[str, dict[str, str]] = {}
    partial = EMPTY
    # Preserve page order and the two-column reading flow. A wrapped index title
    # can continue from the bottom of a right column onto the next page's left.
    for line in index_lines(pdf, first, last, edition):
        partial = consume_index_line(entries, partial, line, edition)
    if partial:
        print(f"Unparsed {edition} index tail: {partial}", file=sys.stderr)
    return entries


def index_lines(pdf: Path, first: int, last: int, edition: str):
    for page in bbox_pages(pdf, first, last):
        page_columns: list[list[str]] = [[], []]
        for side, line in indexed_columns(page, edition == EDITION_2017):
            if line in INDEX_HEADERS or (line and all(is_digit(char) for char in line)):
                continue
            page_columns[side].append(line)
        for lines in page_columns:
            for line in lines:
                yield line


def consume_index_line(
    entries: dict[str, dict[str, str]], partial: str, line: str, edition: str
) -> str:
    combined = append_index_line(partial, line)
    parsed = parse_index_entry(combined, edition)
    if parsed is None:
        return combined
    add_index_entry(entries, *parsed)
    return EMPTY


def append_index_line(partial: str, line: str) -> str:
    return f"{partial} {line}".strip()


def parse_index_entry(partial: str, edition: str) -> tuple[str, str] | None:
    if edition == EDITION_2017:
        separator = partial.rfind("..")
        if separator < 0:
            return None
        title = partial[:separator].strip(" .")
        refs = partial[separator + 2 :].strip()
        return (title, refs) if title and is_page_reference_list(refs) else None
    return parse_reference_suffix(partial)


def add_index_entry(entries: dict[str, dict[str, str]], title: str, refs: str) -> None:
    if title.casefold() in INDEX_HEADERS_CASEFOLDED:
        return
    key = normalized(title)
    if not key:
        return
    if key in entries:
        old = entries[key][PRINTED_PAGES_FIELD]
        entries[key][PRINTED_PAGES_FIELD] = COMMA_SPACE.join(dict.fromkeys([old, refs]))
        return
    entries[key] = {TITLE_FIELD: title, PRINTED_PAGES_FIELD: refs}


def extract_toc(pdf: Path, first: int, last: int, edition: str) -> list[tuple[int, str]]:
    rows = read_toc_rows(pdf, first, last, TOC_HEADERS)
    if edition == EDITION_2014:
        return [(page, title) for page, _, title in rows]
    return chapter_toc_rows(rows)


def chapter_toc_rows(rows: list[tuple[int, float, str]]) -> list[tuple[int, str]]:
    result: list[tuple[int, str]] = []
    chapter = EMPTY
    for page, x, title in rows:
        if x < 65:
            chapter = title
        result.append((page, chapter or title))
    return result


def extract_toc_candidates(pdf: Path, first: int, last: int) -> dict[str, dict[str, str]]:
    rows = read_toc_rows(pdf, first, last, {"Table of Contents"})
    return merge_toc_candidates(rows)


def read_toc_rows(pdf: Path, first: int, last: int, headers: set[str]) -> list[tuple[int, float, str]]:
    rows: list[tuple[int, float, str]] = []
    for page in bbox_pages(pdf, first, last):
        for x, line in line_values(page):
            if line in headers:
                continue
            parsed = dotted_reference_suffix(line)
            if parsed:
                title, references = parsed
                page_ref = parse_page_reference_part(references)
                if page_ref:
                    rows.append((page_ref[0], x, title))
    rows.sort(key=lambda item: item[0])
    return rows


def merge_toc_candidates(rows: list[tuple[int, float, str]]) -> dict[str, dict[str, str]]:
    result: dict[str, dict[str, str]] = {}
    chapter = EMPTY
    for page, x, title in rows:
        if x < 65:
            chapter = title
        key = normalized(title)
        if key not in result:
            result[key] = {TITLE_FIELD: title, PRINTED_PAGES_FIELD: str(page), CHAPTER_FIELD: chapter or title}
        else:
            previous = result[key][PRINTED_PAGES_FIELD]
            result[key][PRINTED_PAGES_FIELD] = COMMA_SPACE.join(dict.fromkeys([previous, str(page)]))
    return result


def chapter_for(page: int, toc: list[tuple[int, str]]) -> str:
    matches = [title for start, title in toc if start <= page]
    return matches[-1] if matches else "Unclassified by contents"


def pdf_text_pages(pdf: Path, page_count: int, offset: int) -> dict[int, str]:
    # Printed page -> physical PDF page is `printed + offset`.
    return {
        printed: run("pdftotext", "-layout", "-f", str(printed + offset), "-l", str(printed + offset), str(pdf), HYPHEN)
        for printed in range(1, page_count + 1)
    }


def image_pages(pdf: Path) -> set[int]:
    lines = run("pdfimages", "-list", str(pdf)).splitlines()[2:]
    result = set()
    for line in lines:
        columns = line.split()
        if len(columns) >= 3 and is_digit(columns[0][0]) and columns[2] == "image":
            result.add(int(columns[0]))
    return result


def has_text_phrase(text: str, phrase: str) -> bool:
    normalized_text = SPACE.join(text.casefold().split())
    lowered_phrase = phrase.casefold()
    position = normalized_text.find(lowered_phrase)
    while position >= 0:
        end = position + len(lowered_phrase)
        before = normalized_text[position - 1] if position else SPACE
        after = normalized_text[end] if end < len(normalized_text) else SPACE
        if not before.isalnum() and not after.isalnum():
            return True
        position = normalized_text.find(lowered_phrase, position + 1)
    return False


def has_intervening_character(text: str, left: str, right: str) -> bool:
    lowered = text.casefold()
    position = lowered.find(left)
    while position >= 0:
        if has_intervening_match(lowered, position, left, right):
            return True
        position = lowered.find(left, position + 1)
    return False


def has_intervening_match(text: str, position: int, left: str, right: str) -> bool:
    before = text[position - 1] if position else SPACE
    end = skip_intervening_characters(text, position + len(left))
    right_end = end + len(right)
    return (
        not before.isalnum()
        and end > position + len(left)
        and text.startswith(right, end)
        and (right_end == len(text) or not text[right_end].isalnum())
    )


def skip_intervening_characters(text: str, position: int) -> int:
    if position >= len(text):
        return position
    if text[position].isspace():
        while position < len(text) and text[position].isspace():
            position += 1
    else:
        position += 1
    return position


def has_colon_marker(text: str, choices: tuple[str, ...]) -> bool:
    normalized_text = SPACE.join(text.casefold().split())
    for choice in choices:
        marker = choice.casefold()
        position = normalized_text.find(marker)
        while position >= 0:
            before = normalized_text[position - 1] if position else SPACE
            end = position + len(marker)
            after_index = end
            while after_index < len(normalized_text) and normalized_text[after_index].isspace():
                after_index += 1
            if (
                not before.isalnum()
                and after_index < len(normalized_text)
                and normalized_text[after_index] == ":"
            ):
                return True
            position = normalized_text.find(marker, position + 1)
    return False


def has_sequence_marker(text: str) -> bool:
    if has_colon_marker(text, ("siteswap", "prechac")):
        return True
    if has_text_phrase(text, "pattern for") or has_text_phrase(text, "starting pattern for"):
        return True
    return has_colon_marker(text, ("start pattern", "starting pattern"))


def manipulation_tags(text: str) -> list[str]:
    words = text_words(text)
    tags: list[str] = []
    has_lo_jack = "lojack" in words or any(
        words[index] == "lo" and words[index + 1].startswith("jack")
        for index in range(len(words) - 1)
    )
    movement_words = {"walk", "walks", "walked", "walking", "runaround", "formation"}
    if any(word in movement_words for word in words) or has_text_phrase(text, "figure of eight"):
        tags.append("movement/walking")
    if any(word in {"hold", "holds"} for word in words):
        tags.append("holds")
    if (
        any(word in {"zip", "zips"} for word in words)
        or has_intervening_character(text, "hand", "across")
        or "handacross" in words
    ):
        tags.append("zips/hand-across")
    if any(word.startswith("steal") for word in words) or "manipulator" in words:
        tags.append(MANIPULATOR_ACTION)
    if (
        any("hijack" in word for word in words)
        or has_lo_jack
        or any(word.startswith("programming") for word in words)
    ):
        tags.append("pattern-transition manipulation")
    return tags


def manipulation_classification(tags: list[str], sequence_data: str) -> tuple[str, str]:
    if "pattern-transition manipulation" in tags:
        return "manipulation", "pattern transition / hijack"
    if MANIPULATOR_ACTION in tags:
        return UNCLEAR, "possible external manipulator is used to enter/describe the pattern"
    if sequence_data == YES:
        return "not_detected_in_extracted_text", "explicit siteswap/Prechac/throw sequence; no manipulator signal in extracted text (not visually reviewed)"
    return UNCLEAR, "needs visual/context review"


def model_candidate(tags: list[str], sequence_data: str) -> str:
    unmodeled = {"movement/walking", "holds", "zips/hand-across", MANIPULATOR_ACTION}
    if any(tag in unmodeled for tag in tags):
        return NO
    return YES if sequence_data == YES else UNCLEAR


def evidence(edition: str, printed_pages: str, text_by_page: dict[int, str], images: set[int]) -> tuple[str, str, str, str, str, str]:
    printed = page_refs(printed_pages)
    physical_offset = 0 if edition == EDITION_2014 else 1
    texts = [text_by_page[p] for p in printed if p in text_by_page]
    text = "\n".join(texts)
    physical_pages = [p + physical_offset for p in printed]
    page_images = YES if any(page in images for page in physical_pages) else NO
    # The page may contain a photo or other artwork; image presence alone does not
    # establish a causal diagram. A human must inspect the page before this is yes/no.
    causal_diagram = "unclear"
    sequence_markers = has_sequence_marker(text)
    sequence_text = YES if sequence_markers else NO
    sequence_data = YES if sequence_markers else UNCLEAR
    tags = manipulation_tags(text)
    manipulation, kind = manipulation_classification(tags, sequence_data)
    representable = model_candidate(tags, sequence_data)
    details = [kind, "model candidate: " + representable, "requirements: " + (COMMA_SPACE.join(tags) or "none detected")]
    return manipulation, page_images, causal_diagram, sequence_data, sequence_text, "; ".join(details)


def sha(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


CSV_COLUMNS = (
    "stable_id", "title_2017", "title_2014", "entry_kind", "2017_catalog_source", "2014_catalog_source", "edition_change_status",
    "chapter_2017", "2017_index_printed_pages", "2017_toc_printed_pages", "2017_pdf_pages", "2017_source",
    "chapter_2014", "2014_index_printed_pages", "2014_toc_printed_pages", "2014_pdf_pages", "2014_source",
    "manipulation_class_2017", "page_images_present_2017", "causal_diagram_data_status_2017", "sequence_data_status_2017", "sequence_text_evidence_2017", "model_requirements_2017",
    "manipulation_class_2014", "page_images_present_2014", "causal_diagram_data_status_2014", "sequence_data_status_2014", "sequence_text_evidence_2014", "model_requirements_2014",
    "visual_review_status", "processing_status",
)
CONCEPT_TITLES = {
    "passing etiquette", "colour coding patterns", "colour coding", "causal diagrams",
    "four handed siteswaps", "prechac notation", "brolly notation", "basic patterns",
    "how to work out hijacks in period five patterns",
}
FAMILY_PLURALS = {"patterns", "feeds", "feasts", "triangles", "tricks", "runarounds", "weaves", "siteswaps"}
FAMILY_SINGULARS = {"feed", "feast", "triangle", "trick", "runaround", "weave", "siteswap"}


def entry_kind(title: str) -> str:
    lowered = title.casefold()
    if lowered in CONCEPT_TITLES:
        return "concept/instruction index entry"
    words = text_words(title)
    has_family_plural = any(word in FAMILY_PLURALS for word in words)
    if has_family_plural and not has_suffix_parenthesis_after_word(title, FAMILY_SINGULARS):
        return "family/section candidate; confirm whether it expands to individual patterns"
    return "individual pattern or named variant; verify aliases"


def catalog_for_edition(
    pdf: Path, edition: str, index_range: tuple[int, int], toc_range: tuple[int, int]
) -> tuple[dict[str, dict[str, str]], dict[str, dict[str, str]], list[tuple[int, str]], dict[str, dict[str, str]]]:
    index = extract_index(pdf, *index_range, edition)
    toc_candidates = extract_toc_candidates(pdf, *toc_range)
    toc_only = {
        key: value for key, value in toc_candidates.items()
        if key not in index and not (edition == EDITION_2014 and value[TITLE_FIELD].casefold() == "index")
    }
    for key, value in toc_only.items():
        index[key] = {
            TITLE_FIELD: value[TITLE_FIELD],
            PRINTED_PAGES_FIELD: value[PRINTED_PAGES_FIELD],
            CATALOG_SOURCE_FIELD: TOC_ONLY,
            CHAPTER_FIELD: value[CHAPTER_FIELD],
        }
    for value in index.values():
        value.setdefault(CATALOG_SOURCE_FIELD, INDEX_SOURCE)
    for key, value in index.items():
        in_index = value[CATALOG_SOURCE_FIELD] == INDEX_SOURCE
        toc_entry = toc_candidates.get(key)
        value[INDEX_PAGES_FIELD] = value[PRINTED_PAGES_FIELD] if in_index else EMPTY
        value[TOC_PAGES_FIELD] = toc_entry[PRINTED_PAGES_FIELD] if toc_entry else EMPTY
        if toc_entry and in_index:
            value[CATALOG_SOURCE_FIELD] = "Index;TOC"
        value[PRINTED_PAGES_FIELD] = value[INDEX_PAGES_FIELD] or value[TOC_PAGES_FIELD]
    toc = extract_toc(pdf, *toc_range, edition)
    return index, toc_candidates, toc, toc_only


def edition_change(old: dict[str, str] | None, new: dict[str, str] | None) -> str:
    if old and new:
        if new[CATALOG_SOURCE_FIELD] == TOC_ONLY:
            return "present in 2017 TOC only; no current index entry"
        if old[PRINTED_PAGES_FIELD] == new[PRINTED_PAGES_FIELD] and old[CATALOG_SOURCE_FIELD] == INDEX_SOURCE:
            return "same index title and page references"
        return "title/page reference differs across edition indexes; mechanics not compared"
    if new:
        return "candidate title only in 2017 index/TOC; mechanics/aliases not compared"
    return "candidate title only in 2014 index/TOC; absent/renamed/split in 2017 not confirmed"


def missing_edition_fields(edition: str) -> dict[str, str]:
    values = {
        TITLE_FIELD: EMPTY, CHAPTER_FIELD: EMPTY, INDEX_PAGES_FIELD: EMPTY, TOC_PAGES_FIELD: EMPTY,
        "pdf_pages": EMPTY, CATALOG_SOURCE_FIELD: NOT_PRESENT, MANIPULATION: UNCLEAR,
        "page_images": UNCLEAR, "causal_diagram": UNCLEAR, "sequence_data": UNCLEAR,
        "sequence_text": UNCLEAR,
    }
    values["requirements"] = "needs source review" if edition == EDITION_2017 else "not present in this index"
    return values


def edition_fields(
    entry: dict[str, str] | None,
    edition: str,
    text_by_page: dict[int, str],
    images: set[int],
    toc: list[tuple[int, str]],
    toc_candidates: dict[str, dict[str, str]],
    key: str,
) -> dict[str, str]:
    if not entry:
        return missing_edition_fields(edition)
    pages = entry[PRINTED_PAGES_FIELD]
    refs = page_refs(pages)
    chapter = entry.get(CHAPTER_FIELD)
    if not chapter and edition == EDITION_2017:
        chapter = toc_candidates.get(key, {}).get(CHAPTER_FIELD)
    if not chapter and refs:
        chapter = chapter_for(min(refs), toc)
    manipulation, page_images, diagram, sequence, sequence_text, requirements = evidence(
        edition, pages, text_by_page, images
    )
    physical = [page + (1 if edition == EDITION_2017 else 0) for page in refs]
    return {
        TITLE_FIELD: entry[TITLE_FIELD], CHAPTER_FIELD: chapter or EMPTY,
        INDEX_PAGES_FIELD: entry[INDEX_PAGES_FIELD],
        TOC_PAGES_FIELD: entry[TOC_PAGES_FIELD],
        "pdf_pages": COMMA_SPACE.join(map(str, physical)), CATALOG_SOURCE_FIELD: entry[CATALOG_SOURCE_FIELD],
        MANIPULATION: manipulation, "page_images": page_images, "causal_diagram": diagram,
        "sequence_data": sequence, "sequence_text": sequence_text, "requirements": requirements,
    }


def source_description(edition: str, digest: str) -> str:
    if edition == EDITION_2017:
        return f"{CURRENT_URL}; Aidan Burns; CreationDate 2017-10-09; 340 PDF pages; SHA256 {digest}"
    return f"{LOCAL_URL}; bundled Siteswaps.Mcp.Server/Resources/highgate.pdf; Aidan Burns; CreationDate 2014-05-16; 246 PDF pages; SHA256 {digest}"


def csv_row(
    key: str,
    old: dict[str, str] | None,
    new: dict[str, str] | None,
    contexts: dict[str, tuple[dict[int, str], set[int], list[tuple[int, str]], dict[str, dict[str, str]]]],
    hashes: dict[str, str],
) -> dict[str, str]:
    selected = new or old
    assert selected is not None
    row: dict[str, str] = {
        "stable_id": stable_id(selected[TITLE_FIELD]),
        "edition_change_status": edition_change(old, new),
        "visual_review_status": "pending",
        "processing_status": "open",
        "entry_kind": entry_kind(selected[TITLE_FIELD]),
    }
    for edition, entry in ((EDITION_2017, new), (EDITION_2014, old)):
        text_by_page, images, toc, candidates = contexts[edition]
        values = edition_fields(entry, edition, text_by_page, images, toc, candidates, key)
        row[f"title_{edition}"] = values[TITLE_FIELD]
        row[f"chapter_{edition}"] = values[CHAPTER_FIELD]
        row[f"{edition}_index_printed_pages"] = values[INDEX_PAGES_FIELD]
        row[f"{edition}_toc_printed_pages"] = values[TOC_PAGES_FIELD]
        row[f"{edition}_pdf_pages"] = values["pdf_pages"]
        row[f"{edition}_catalog_source"] = values[CATALOG_SOURCE_FIELD]
        row[f"manipulation_class_{edition}"] = values[MANIPULATION]
        row[f"page_images_present_{edition}"] = values["page_images"]
        row[f"causal_diagram_data_status_{edition}"] = values["causal_diagram"]
        row[f"sequence_data_status_{edition}"] = values["sequence_data"]
        row[f"sequence_text_evidence_{edition}"] = values["sequence_text"]
        row[f"model_requirements_{edition}"] = values["requirements"]
        row[f"{edition}_source"] = source_description(edition, hashes[edition])
    return row


def print_candidate_counts(
    local: dict[str, dict[str, str]],
    current: dict[str, dict[str, str]],
    local_toc_only: dict[str, dict[str, str]],
    current_toc_only: dict[str, dict[str, str]],
) -> None:
    local_index_count = len(local) - len(local_toc_only)
    current_index_count = len(current) - len(current_toc_only)
    shared = len(local.keys() & current.keys())
    union = len(local.keys() | current.keys())
    print(
        f"candidate counts: local-index={local_index_count} local-toc-only={len(local_toc_only)} "
        f"current-index={current_index_count} current-toc-only={len(current_toc_only)} "
        f"union={union} exact-shared={shared}"
    )


def write_inventory(
    local: dict[str, dict[str, str]],
    current: dict[str, dict[str, str]],
    contexts: dict[str, tuple[dict[int, str], set[int], list[tuple[int, str]], dict[str, dict[str, str]]]],
    hashes: dict[str, str],
) -> None:
    OUT.parent.mkdir(parents=True, exist_ok=True)
    with OUT.open("w", newline=EMPTY, encoding="utf-8") as file:
        writer = csv.DictWriter(file, fieldnames=CSV_COLUMNS)
        writer.writeheader()
        for key in sorted(local.keys() | current.keys()):
            writer.writerow(csv_row(key, local.get(key), current.get(key), contexts, hashes))


def build_contexts(current_pdf: Path):
    local, local_toc_candidates, local_toc, local_toc_only = catalog_for_edition(
        LOCAL_PDF, EDITION_2014, (239, 246), (2, 3)
    )
    current, current_toc_candidates, current_toc, current_toc_only = catalog_for_edition(
        current_pdf, EDITION_2017, (332, 340), (3, 17)
    )
    local_text = pdf_text_pages(LOCAL_PDF, 246, 0)
    current_text = pdf_text_pages(current_pdf, 339, 1)
    contexts = {
        EDITION_2014: (local_text, image_pages(LOCAL_PDF), local_toc, local_toc_candidates),
        EDITION_2017: (current_text, image_pages(current_pdf), current_toc, current_toc_candidates),
    }
    hashes = {EDITION_2014: sha(LOCAL_PDF), EDITION_2017: sha(current_pdf)}
    print_candidate_counts(local, current, local_toc_only, current_toc_only)
    return local, current, contexts, hashes


def main() -> None:
    if len(sys.argv) != 2:
        raise SystemExit("Usage: build_highgate_inventory.py /path/to/2017-highgate.pdf")
    current_pdf = Path(sys.argv[1]).resolve()
    local, current, contexts, hashes = build_contexts(current_pdf)
    write_inventory(local, current, contexts, hashes)


if __name__ == "__main__":
    main()
