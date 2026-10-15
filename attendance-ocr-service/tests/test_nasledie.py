from app.models import RegionType
from app.nasledie import build_attendance_tables, convert_nasledie_result, flatten_acceptable_names


ACCEPTABLE_NAMES = {
    "PI-101": [
        ["Ivan", "Smirnov"],
        ["Maria", "Petrova"],
    ],
    "PI-102": [
        ["Sergey", "Orlov"],
    ],
}


NASLEDIE_RESULT = {
    "subject": "Algorithms",
    "date": "2026-05-29",
    "ids": (0, 2),
    "names": (
        ("Ivan", "Smirnov", "PI-101"),
        ("Sergey", "Orlov", "PI-102"),
    ),
    "meta": {
        "matched_names": (
            ("Ivan", "Smirnov", "PI-101"),
            ("Sergey", "Orlov", "PI-102"),
        ),
        "matched_signs": (("+",), tuple()),
        "name_clusters": (
            (((10, 20, 80, 40), (90, 20, 160, 40)),),
            (((10, 60, 150, 80),),),
        ),
        "date_clusters": ((((300, 10, 360, 30),),),),
        "subject_clusters": ((((380, 10, 520, 30),),),),
        "unmatched_clusters": ((((10, 100, 120, 120),),),),
    },
}


def test_convert_nasledie_result_maps_regions_and_roster_ids():
    result = convert_nasledie_result(
        NASLEDIE_RESULT,
        image_id="sheet_001",
        acceptable_names=ACCEPTABLE_NAMES,
    )

    name_regions = [region for region in result.regions if region.type == RegionType.NAME]
    assert result.image_id == "sheet_001"
    assert [region.text for region in name_regions[:2]] == [
        "Ivan Smirnov",
        "Sergey Orlov",
    ]
    assert name_regions[0].bbox.x1 == 10
    assert name_regions[0].bbox.x2 == 160
    assert name_regions[0].raw["student_index"] == 0
    assert name_regions[-1].raw["status"] == "needs_review"


def test_flatten_acceptable_names_matches_pipeline_index_order():
    roster = flatten_acceptable_names(ACCEPTABLE_NAMES)

    assert [student["full_name"] for student in roster] == [
        "Ivan Smirnov",
        "Maria Petrova",
        "Sergey Orlov",
    ]
    assert roster[2]["student_index"] == 2


def test_build_attendance_tables_uses_subject_and_undefined_columns():
    samples = [
        {"image_id": "sheet_001", "result": NASLEDIE_RESULT},
        {"image_id": "sheet_002", "result": {"subject": None, "date": None, "ids": (1,), "meta": {}}},
    ]

    tables = build_attendance_tables(samples, ACCEPTABLE_NAMES)

    assert tables["Algorithms"][0]["2026-05-29"] == "+"
    assert tables["Algorithms"][2]["2026-05-29"] == "+"
    assert tables["undefined"][1]["undef_1"] == "+"
