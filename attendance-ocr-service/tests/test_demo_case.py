from fastapi.testclient import TestClient

from app.demo_case import DEMO_DATE_COLUMN, load_demo_ocr_present_names, load_demo_roster
from app.main import app
from app.models import JobStatus


def test_demo_case_roster_loads_public_sample_data():
    rows = load_demo_roster()
    ocr_present_names = load_demo_ocr_present_names()

    assert len(rows) == 8
    assert sum(row.present for row in rows) == 5
    assert len(ocr_present_names) == 5
    assert "Ivan Smirnov" in ocr_present_names
    assert DEMO_DATE_COLUMN == "20-05-2026"


def test_demo_mode_processes_two_sheet_attendance_case():
    client = TestClient(app)
    create_response = client.post(
        "/attendance/ocr/jobs",
        json={
            "image_ids": ["demo_sheet_front", "demo_sheet_back"],
            "lesson": {
                "lesson_id": "demo-public-sample-2026-05-20",
                "date": "2026-05-20",
                "group": "PI-101",
                "subject": "Software Engineering",
            },
        },
    )
    assert create_response.status_code == 202
    job = create_response.json()

    process_response = client.post(f"/attendance/ocr/jobs/{job['job_id']}/process?mode=demo")

    assert process_response.status_code == 200
    processed = process_response.json()
    assert processed["status"] == JobStatus.COMPLETED
    assert len(processed["result"]["students"]) == 5
    assert len(processed["result"]["ocr"]) == 2
    assert any(
        student["full_name"] == "Ivan Smirnov"
        for student in processed["result"]["students"]
    )
