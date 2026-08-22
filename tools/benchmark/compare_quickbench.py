#!/usr/bin/env python3
"""Compare two QuickBench JSON reports without external dependencies."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path


def load(path: Path) -> dict:
    with path.open(encoding="utf-8") as handle:
        report = json.load(handle)
    if not isinstance(report.get("Scenarios"), list):
        raise ValueError(f"{path} has no Scenarios array")
    return report


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("baseline", type=Path)
    parser.add_argument("current", type=Path)
    parser.add_argument("--minimum-speedup", type=float, default=0.50)
    args = parser.parse_args()

    baseline = load(args.baseline)
    current = load(args.current)
    before = {row["Scenario"]: row for row in baseline["Scenarios"]}
    after = {row["Scenario"]: row for row in current["Scenarios"]}
    if set(before) != set(after):
        print(f"scenario mismatch: baseline={sorted(before)} current={sorted(after)}")
        return 2

    failed = False
    total_before = 0.0
    total_after = 0.0
    print("scenario                         before ms   after ms   speedup   results")
    for name in sorted(before):
        old = before[name]
        new = after[name]
        if old["ResultCount"] != new["ResultCount"]:
            print(f"{name:32} result-count mismatch: {old['ResultCount']} -> {new['ResultCount']}")
            failed = True
            continue
        old_ms = float(old["WallMilliseconds"])
        new_ms = float(new["WallMilliseconds"])
        total_before += old_ms
        total_after += new_ms
        speedup = 1.0 - new_ms / old_ms if old_ms > 0 else 0.0
        print(f"{name:32} {old_ms:10.3f} {new_ms:10.3f} {speedup:8.1%} {new['ResultCount']:8}")
        if speedup < args.minimum_speedup:
            failed = True

    total_speedup = 1.0 - total_after / total_before if total_before > 0 else 0.0
    print(f"total                              {total_before:10.3f} {total_after:10.3f} {total_speedup:8.1%}")
    if failed:
        print(f"FAIL: not every scenario reached {args.minimum_speedup:.1%} speedup")
        return 1
    print(f"PASS: every scenario reached {args.minimum_speedup:.1%} speedup")
    return 0


if __name__ == "__main__":
    sys.exit(main())
