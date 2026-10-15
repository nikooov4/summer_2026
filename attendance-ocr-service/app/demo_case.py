from __future__ import annotations

import csv
from dataclasses import dataclass
from pathlib import Path

from .models import BoundingBox, OcrImageResult, OcrRegion, RegionType, Student


DEMO_LESSON_ID = "demo-public-sample-2026-05-20"
DEMO_DATE_COLUMN = "20-05-2026"
DEMO_PAIR_ID = "pair_1"
DEMO_SUBJECT = "Software Engineering"


@dataclass(frozen=True)
class DemoRosterRow:
    student_id: str
    full_name: str
    group: str
    present: bool


def demo_data_path() -> Path:
    return demo_static_dir() / "public_synthetic_attendance.csv"


def demo_pair_summary_path() -> Path:
    return demo_static_dir() / "public_pair_summary.csv"


def demo_static_dir() -> Path:
    return Path(__file__).resolve().parent.parent / "app" / "static" / "demo" / "public-sample"


def load_demo_roster(date_column: str = DEMO_DATE_COLUMN) -> list[DemoRosterRow]:
    path = demo_data_path()
    if not path.exists():
        return []

    rows: list[DemoRosterRow] = []
    with path.open(encoding="utf-8-sig", newline="") as file:
        for row in csv.DictReader(file):
            full_name = (row.get("Student name") or "").strip()
            group = (row.get("Group") or row.get("Группа") or "").strip()
            if not full_name:
                continue
            rows.append(
                DemoRosterRow(
                    student_id=f"demo_student_{row.get('№', len(rows) + 1)}",
                    full_name=full_name,
                    group=group,
                    present=(row.get(date_column) or "").strip() == "+",
                )
            )
    return rows


def load_demo_students() -> list[Student]:
    return [
        Student(student_id=row.student_id, full_name=row.full_name, group=row.group)
        for row in load_demo_roster()
    ]


def load_demo_ocr_present_names(pair_id: str = DEMO_PAIR_ID) -> list[str]:
    path = demo_pair_summary_path()
    if not path.exists():
        return [row.full_name for row in load_demo_roster() if row.present]

    with path.open(encoding="utf-8-sig", newline="") as file:
        for row in csv.DictReader(file):
            if row.get("pair") == pair_id:
                return [
                    item.strip()
                    for item in (row.get("added_students") or "").split(";")
                    if item.strip()
                ]
    return []


def build_demo_ocr_results(image_ids: list[str]) -> list[OcrImageResult]:
    present_names = set(load_demo_ocr_present_names())
    present_rows = [row for row in load_demo_roster() if row.full_name in present_names]
    if not present_rows:
        return []

    image_count = max(len(image_ids), 1)
    chunk_size = max(1, (len(present_rows) + image_count - 1) // image_count)
    results: list[OcrImageResult] = []

    for image_index, image_id in enumerate(image_ids):
        start = image_index * chunk_size
        chunk = present_rows[start : start + chunk_size]
        regions = [
            OcrRegion(
                type=RegionType.DATE,
                text=DEMO_DATE_COLUMN,
                confidence=0.98,
                bbox=BoundingBox(x1=32, y1=24, x2=176, y2=52),
            ),
            OcrRegion(
                type=RegionType.SUBJECT,
                text=DEMO_SUBJECT,
                confidence=0.97,
                bbox=BoundingBox(x1=204, y1=24, x2=316, y2=52),
            ),
        ]
        for row_index, row in enumerate(chunk):
            y = 92 + row_index * 36
            regions.append(
                OcrRegion(
                    type=RegionType.NAME,
                    text=row.full_name,
                    confidence=0.96,
                    bbox=BoundingBox(x1=48, y1=y, x2=420, y2=y + 24),
                    raw={"group": row.group, "demo_case": DEMO_LESSON_ID},
                )
            )
        results.append(
            OcrImageResult(
                image_id=image_id,
                regions=regions,
                raw={
                    "demo_case": DEMO_LESSON_ID,
                    "source": "public_synthetic_attendance.csv",
                    "date_column": DEMO_DATE_COLUMN,
                },
            )
        )
    return results
