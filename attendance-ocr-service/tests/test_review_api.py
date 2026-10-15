from fastapi.testclient import TestClient

from app.dependencies import container
from app.models import JobStatus, ReviewAction
from app.main import app


def _create_processed_job(client: TestClient) -> dict:
    response = client.post(
        "/attendance/ocr/jobs",
        json={
            "image_ids": ["demo_image_001"],
            "lesson": {"group": "PI-101"},
        },
    )
    assert response.status_code == 202
    job = response.json()
    processed = container.job_processor.process_job(job["job_id"])
    assert processed.status in {JobStatus.COMPLETED, JobStatus.NEEDS_REVIEW}
    return job


def test_get_review_requires_ready_result():
    client = TestClient(app)
    response = client.post(
        "/attendance/ocr/jobs",
        json={
            "image_ids": ["demo_image_queued"],
            "lesson": {"group": "PI-101"},
        },
    )
    job = response.json()

    review_response = client.get(f"/attendance/ocr/jobs/{job['job_id']}/review")

    assert review_response.status_code == 409


def test_get_review_projects_recognized_students_to_decisions():
    client = TestClient(app)
    job = _create_processed_job(client)

    response = client.get(f"/attendance/ocr/jobs/{job['job_id']}/review")

    assert response.status_code == 200
    review = response.json()
    assert review["job_id"] == job["job_id"]
    assert review["decisions"]
    assert review["summary"]["needs_review_count"] >= 0
    first = review["decisions"][0]
    assert first["decision_id"].startswith("decision_")
    assert first["region_ids"]
    assert first["source_text"]


def test_patch_review_can_ignore_unresolved_decision_and_confirm():
    client = TestClient(app)
    job = _create_processed_job(client)
    review = client.get(f"/attendance/ocr/jobs/{job['job_id']}/review").json()
    unresolved = [
        decision for decision in review["decisions"] if not decision["resolved"]
    ]
    if not unresolved:
        return

    response = client.patch(
        f"/attendance/ocr/jobs/{job['job_id']}/review",
        json={
            "corrections": [
                {
                    "decision_id": unresolved[0]["decision_id"],
                    "action": ReviewAction.IGNORE.value,
                    "region_id": unresolved[0]["region_ids"][0],
                }
            ]
        },
    )

    assert response.status_code == 200
    patched = response.json()
    assert patched["summary"]["needs_review_count"] == review["summary"]["needs_review_count"] - 1

    confirm_response = client.post(f"/attendance/ocr/jobs/{job['job_id']}/review/confirm")
    if patched["summary"]["needs_review_count"] == 0:
        assert confirm_response.status_code == 200
        assert confirm_response.json()["status"] == JobStatus.COMPLETED
    else:
        assert confirm_response.status_code == 409


def test_patch_review_rejects_unknown_decision():
    client = TestClient(app)
    job = _create_processed_job(client)

    response = client.patch(
        f"/attendance/ocr/jobs/{job['job_id']}/review",
        json={
            "corrections": [
                {
                    "decision_id": "decision_missing",
                    "action": ReviewAction.IGNORE.value,
                }
            ]
        },
    )

    assert response.status_code == 409


def test_export_requires_completed_job():
    client = TestClient(app)
    response = client.post(
        "/attendance/ocr/jobs",
        json={
            "image_ids": ["demo_image_queued"],
            "lesson": {"group": "PI-101"},
        },
    )
    job = response.json()

    export_response = client.post(f"/attendance/ocr/jobs/{job['job_id']}/export")

    assert export_response.status_code == 409
