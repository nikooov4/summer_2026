from __future__ import annotations

from datetime import date, datetime
from pathlib import Path
from typing import Any

from .models import BoundingBox, OcrImageResult, OcrRegion, RegionType


UNDEFINED_SUBJECT = "undefined"


def convert_nasledie_result(
    result: dict[str, Any],
    image_id: str,
    acceptable_names: dict[str, list[list[str]]] | None = None,
) -> OcrImageResult:
    """Convert the new nasledie pipeline result into the service OCR contract."""
    roster = flatten_acceptable_names(acceptable_names or {})
    regions: list[OcrRegion] = []
    meta = result.get("meta", {}) if isinstance(result.get("meta"), dict) else {}

    date_text = _date_to_text(result.get("date"))
    if date_text:
        regions.append(
            OcrRegion(
                type=RegionType.DATE,
                text=date_text,
                confidence=0.85,
                bbox=_clusters_bbox(meta.get("date_clusters")),
                raw={"source": "nasledie", "matched": True},
            )
        )

    subject = result.get("subject")
    if subject:
        regions.append(
            OcrRegion(
                type=RegionType.SUBJECT,
                text=str(subject),
                confidence=0.85,
                bbox=_clusters_bbox(meta.get("subject_clusters")),
                raw={"source": "nasledie", "matched": True},
            )
        )

    ids = list(result.get("ids", []) or [])
    matched_clusters = list(meta.get("name_clusters", []) or [])
    matched_names = list(meta.get("matched_names", []) or [])
    matched_signs = list(meta.get("matched_signs", []) or [])

    for index, raw_student_id in enumerate(ids):
        student_index = _safe_int(raw_student_id)
        region_text = _student_name_for_index(student_index, roster)
        if not region_text:
            region_text = _name_to_text(_list_get(matched_names, index))

        regions.append(
            OcrRegion(
                type=RegionType.NAME,
                text=region_text,
                confidence=0.82,
                bbox=_cluster_bbox(_list_get(matched_clusters, index)),
                raw={
                    "source": "nasledie",
                    "matched": True,
                    "student_index": student_index,
                    "nasledie_name": _jsonable(_list_get(matched_names, index)),
                    "nasledie_signs": _jsonable(_list_get(matched_signs, index, [])),
                    "status": "present",
                },
            )
        )

    for cluster in meta.get("unmatched_clusters", []) or []:
        regions.append(
            OcrRegion(
                type=RegionType.NAME,
                text="",
                confidence=0.0,
                bbox=_cluster_bbox(cluster),
                raw={
                    "source": "nasledie",
                    "matched": False,
                    "status": "needs_review",
                },
            )
        )

    return OcrImageResult(
        image_id=image_id,
        regions=regions,
        raw={
            "source": "nasledie",
            "nasledie_result": _jsonable(result),
        },
    )


def flatten_acceptable_names(acceptable_names: dict[str, list[list[str]]]) -> list[dict[str, Any]]:
    """Return the same student order the nasledie pipeline uses for result['ids']."""
    roster: list[dict[str, Any]] = []
    for group, names in acceptable_names.items():
        for parts in names:
            clean_parts = [str(part).strip() for part in parts if str(part).strip()]
            roster.append(
                {
                    "student_index": len(roster),
                    "group": str(group),
                    "parts": clean_parts,
                    "full_name": " ".join(clean_parts),
                }
            )
    return roster


def build_attendance_tables(
    samples: list[dict[str, Any]],
    acceptable_names: dict[str, list[list[str]]],
) -> dict[str, list[dict[str, Any]]]:
    """Build subject keyed attendance tables using result['ids'] columns."""
    roster = flatten_acceptable_names(acceptable_names)
    tables: dict[str, dict[int, dict[str, Any]]] = {}
    subject_columns: dict[str, set[str]] = {}
    undefined_counter = 1

    for sample in samples:
        result = sample.get("result", sample)
        subject = str(result.get("subject") or UNDEFINED_SUBJECT)
        if subject not in tables:
            tables[subject] = _initial_table(roster)
            subject_columns[subject] = set()

        date_label = _date_to_text(result.get("date"))
        if not date_label:
            date_label = f"undef_{undefined_counter}"
            undefined_counter += 1
        column = _unique_column(date_label, subject_columns[subject])
        subject_columns[subject].add(column)

        signs = list((result.get("meta") or {}).get("matched_signs", []) or [])
        for match_index, raw_student_id in enumerate(result.get("ids", []) or []):
            student_index = _safe_int(raw_student_id)
            if student_index is None or student_index not in tables[subject]:
                continue
            tables[subject][student_index][column] = _attendance_mark(_list_get(signs, match_index, []))

    rendered: dict[str, list[dict[str, Any]]] = {}
    for subject, rows_by_index in tables.items():
        rendered[subject] = list(rows_by_index.values())
    return rendered


def _initial_table(roster: list[dict[str, Any]]) -> dict[int, dict[str, Any]]:
    return {
        int(student["student_index"]): {
            "student_index": student["student_index"],
            "group": student["group"],
            "full_name": student["full_name"],
        }
        for student in roster
    }


def _unique_column(label: str, used: set[str]) -> str:
    if label not in used:
        return label
    suffix = 2
    while f"{label}_{suffix}" in used:
        suffix += 1
    return f"{label}_{suffix}"


def _attendance_mark(signs: Any) -> str:
    if isinstance(signs, (list, tuple)) and signs:
        text = "".join(str(sign) for sign in signs if str(sign).strip())
        return text or "+"
    return "+"


def _student_name_for_index(student_index: int | None, roster: list[dict[str, Any]]) -> str:
    if student_index is None or student_index < 0 or student_index >= len(roster):
        return ""
    return str(roster[student_index]["full_name"])


def _name_to_text(name: Any) -> str:
    if name is None:
        return ""
    if isinstance(name, str):
        return name
    if isinstance(name, (list, tuple, set, frozenset)):
        parts = [str(part) for part in name if str(part).strip()]
        if len(parts) > 1 and _looks_like_group(parts[-1]):
            parts = parts[:-1]
        return " ".join(parts)
    return str(name)


def _looks_like_group(value: str) -> bool:
    return bool(value) and any(char.isdigit() for char in value) and "-" in value


def _date_to_text(value: Any) -> str:
    if value is None:
        return ""
    if isinstance(value, datetime):
        return value.date().isoformat()
    if isinstance(value, date):
        return value.isoformat()
    return str(value)


def _clusters_bbox(clusters: Any) -> BoundingBox | None:
    boxes = []
    for cluster in clusters or []:
        bbox = _cluster_bbox(cluster)
        if bbox is not None:
            boxes.append(bbox)
    if not boxes:
        return None
    return BoundingBox(
        x1=min(box.x1 for box in boxes),
        y1=min(box.y1 for box in boxes),
        x2=max(box.x2 for box in boxes),
        y2=max(box.y2 for box in boxes),
    )


def _cluster_bbox(cluster: Any) -> BoundingBox | None:
    boxes = [_box_to_float_tuple(box) for box in _iter_boxes(cluster)]
    boxes = [box for box in boxes if box is not None]
    if not boxes:
        return None
    return BoundingBox(
        x1=min(box[0] for box in boxes),
        y1=min(box[1] for box in boxes),
        x2=max(box[2] for box in boxes),
        y2=max(box[3] for box in boxes),
    )


def _iter_boxes(cluster: Any):
    if cluster is None:
        return
    if _is_box(cluster):
        yield cluster
        return
    for item in cluster:
        if _is_box(item):
            yield item
        else:
            yield from _iter_boxes(item)


def _is_box(value: Any) -> bool:
    return isinstance(value, (list, tuple)) and len(value) == 4 and all(_is_number(item) for item in value)


def _is_number(value: Any) -> bool:
    return isinstance(value, (int, float)) and not isinstance(value, bool)


def _box_to_float_tuple(box: Any) -> tuple[float, float, float, float] | None:
    if not _is_box(box):
        return None
    return float(box[0]), float(box[1]), float(box[2]), float(box[3])


def _safe_int(value: Any) -> int | None:
    try:
        return int(value)
    except (TypeError, ValueError):
        return None


def _list_get(values: list[Any], index: int, default: Any = None) -> Any:
    return values[index] if 0 <= index < len(values) else default


def _jsonable(value: Any) -> Any:
    if isinstance(value, (str, int, float, bool)) or value is None:
        return value
    if isinstance(value, (date, datetime)):
        return _date_to_text(value)
    if isinstance(value, Path):
        return str(value)
    if isinstance(value, dict):
        return {str(key): _jsonable(item) for key, item in value.items()}
    if isinstance(value, (list, tuple, set, frozenset)):
        return [_jsonable(item) for item in value]
    if hasattr(value, "item"):
        try:
            return value.item()
        except ValueError:
            pass
    if hasattr(value, "tolist"):
        return _jsonable(value.tolist())
    return str(value)
