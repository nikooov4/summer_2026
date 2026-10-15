from __future__ import annotations

import re
from difflib import SequenceMatcher

from .models import AttendanceStatus, OcrRegion, RecognizedStudent, RegionType, Student


def normalize_name(value: str) -> str:
    value = value.lower()
    value = re.sub(r"[^а-яa-zё ]+", " ", value)
    value = re.sub(r"\s+", " ", value)
    return value.strip()


def similarity(left: str, right: str) -> float:
    left_norm = normalize_name(left)
    right_norm = normalize_name(right)
    if not left_norm or not right_norm:
        return 0.0
    return SequenceMatcher(None, left_norm, right_norm).ratio()


def match_name_regions(
    regions: list[OcrRegion],
    students: list[Student],
    auto_accept_threshold: float = 0.82,
    review_threshold: float = 0.62,
) -> list[RecognizedStudent]:
    recognized: list[RecognizedStudent] = []
    used_student_ids: set[str] = set()

    for region in regions:
        if region.type != RegionType.NAME or not region.text.strip():
            continue

        scored = sorted(
            (
                (student, similarity(region.text, student.full_name))
                for student in students
                if student.student_id not in used_student_ids
            ),
            key=lambda item: item[1],
            reverse=True,
        )
        best_student, best_score = scored[0] if scored else (None, 0.0)
        combined_score = best_score * region.confidence

        if best_student and combined_score >= auto_accept_threshold:
            used_student_ids.add(best_student.student_id)
            recognized.append(
                RecognizedStudent(
                    student_id=best_student.student_id,
                    full_name=best_student.full_name,
                    source_text=region.text,
                    confidence=combined_score,
                    status=AttendanceStatus.PRESENT,
                    region_ids=[region.region_id],
                )
            )
        elif best_student and combined_score >= review_threshold:
            recognized.append(
                RecognizedStudent(
                    student_id=best_student.student_id,
                    full_name=best_student.full_name,
                    source_text=region.text,
                    confidence=combined_score,
                    status=AttendanceStatus.NEEDS_REVIEW,
                    region_ids=[region.region_id],
                    candidates=[student for student, _score in scored[:3]],
                )
            )
        else:
            recognized.append(
                RecognizedStudent(
                    student_id=None,
                    full_name=None,
                    source_text=region.text,
                    confidence=combined_score,
                    status=AttendanceStatus.NEEDS_REVIEW,
                    region_ids=[region.region_id],
                    candidates=[student for student, _score in scored[:3]],
                )
            )

    return recognized

