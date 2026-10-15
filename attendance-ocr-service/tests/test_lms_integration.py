from dataclasses import replace

import pytest
from fastapi.testclient import TestClient

from app.adapters import StubOcrAdapter
from app.dependencies import AppContainer
from app.main import create_app
from app.models import LessonContext, Student
from app.settings import settings
from app.sqlite_repository import SQLiteJobRepository


@pytest.fixture
def integration(tmp_path):
    configured = replace(settings, upload_dir=tmp_path / "uploads", sqlite_database_path=tmp_path / "jobs.sqlite3")
    container = AppContainer(configured)
    return TestClient(create_app(container)), container


def create(client, students):
    response = client.post("/attendance/ocr/jobs", json={
        "image_ids": ["test-image"], "lesson": {"lesson_id": "lms-lesson"}, "students": students,
    })
    assert response.status_code == 202
    return response.json()["job_id"]


def test_each_job_matches_only_its_lms_roster(integration):
    client, container = integration
    first = create(client, [{"student_id": "lms-a", "full_name": "Ivan Smirnov"}])
    second = create(client, [{"student_id": "lms-b", "full_name": "Ivan Smirnov"}])
    for job_id, expected in [(second, "lms-b"), (first, "lms-a")]:
        job = client.post(f"/attendance/ocr/jobs/{job_id}/process").json()
        assert job["result"]["students"][0]["student_id"] == expected
    restored = SQLiteJobRepository(container.settings.sqlite_database_path).get(first)
    assert restored.students[0].student_id == "lms-a"


def test_empty_lms_roster_does_not_use_stub_students(integration):
    client, _ = integration
    job_id = create(client, [])
    job = client.post(f"/attendance/ocr/jobs/{job_id}/process").json()
    assert all(row["student_id"] is None for row in job["result"]["students"])


def test_full_roster_available_for_manual_correction(integration):
    client, _ = integration
    students = [{"student_id": str(index), "full_name": f"Different Person {index}"} for index in range(5)]
    job_id = create(client, students)
    job = client.post(f"/attendance/ocr/jobs/{job_id}/process").json()
    decisions = job["review"]["decisions"]
    assert "4" not in [candidate["student_id"] for candidate in decisions[0]["candidates"]]
    response = client.patch(f"/attendance/ocr/jobs/{job_id}/review", json={"corrections": [
        {"decision_id": decisions[0]["decision_id"], "action": "select_student", "selected_student_id": "4"},
        {"decision_id": decisions[1]["decision_id"], "action": "ignore"},
    ]})
    assert response.status_code == 200
    assert response.json()["decisions"][0]["resolved"]
    assert client.post(f"/attendance/ocr/jobs/{job_id}/review/confirm").status_code == 200


def test_snapshot_passed_to_ocr_adapter_without_mutating_shared_lms(integration):
    client, container = integration

    class CapturingOcr(StubOcrAdapter):
        def recognize_for_students(self, image_ids, lesson, students):
            assert lesson.lesson_id == "lms-lesson"
            assert students == [Student(student_id="lms", full_name="Custom Person")]
            return []

    container.job_processor.ocr = CapturingOcr()
    job_id = create(client, [{"student_id": "lms", "full_name": "Custom Person"}])
    job = client.post(f"/attendance/ocr/jobs/{job_id}/process").json()
    assert job["status"] == "completed"
    assert container.lms.get_students(LessonContext())[0].student_id == "student_001"


def test_duplicate_student_ids_rejected(integration):
    client, _ = integration
    response = client.post("/attendance/ocr/jobs", json={"image_ids": ["x"], "students": [
        {"student_id": "same", "full_name": "First Person"}, {"student_id": "same", "full_name": "Second Person"},
    ]})
    assert response.status_code == 422


def test_browser_cors_allows_only_configured_lms(integration):
    client, _ = integration
    response = client.options("/attendance/ocr/jobs", headers={
        "Origin": "http://localhost:5293", "Access-Control-Request-Method": "POST", "Access-Control-Request-Headers": "content-type",
    })
    assert response.status_code == 200
    assert response.headers["access-control-allow-origin"] == "http://localhost:5293"
    assert client.options("/attendance/ocr/jobs", headers={
        "Origin": "http://untrusted.example", "Access-Control-Request-Method": "POST",
    }).status_code == 400
