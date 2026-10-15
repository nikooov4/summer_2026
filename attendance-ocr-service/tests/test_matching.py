from app.matching import match_name_regions, similarity
from app.models import OcrRegion, RegionType, Student


def test_similarity_normalizes_case_and_punctuation():
    assert similarity("Ivan Smirnov!", "ivan smirnov") == 1.0


def test_match_name_regions_accepts_high_confidence_match():
    regions = [
        OcrRegion(type=RegionType.NAME, text="Ivan Smirnov", confidence=0.95),
    ]
    students = [
        Student(student_id="student_001", full_name="Ivan Smirnov"),
    ]

    result = match_name_regions(regions, students)

    assert result[0].student_id == "student_001"
    assert result[0].status == "present"


def test_match_name_regions_marks_weak_match_for_review():
    regions = [
        OcrRegion(type=RegionType.NAME, text="Случайный текст", confidence=0.7),
    ]
    students = [
        Student(student_id="student_001", full_name="Ivan Smirnov"),
    ]

    result = match_name_regions(regions, students)

    assert result[0].student_id is None
    assert result[0].status == "needs_review"
