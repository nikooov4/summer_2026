from fastapi.testclient import TestClient

from app.dependencies import container
from app.main import app
from app.models import JobStatus


def test_process_job_endpoint_runs_queued_job():
    client = TestClient(app)
    create_response = client.post(
        "/attendance/ocr/jobs",
        json={
            "image_ids": ["demo_image_001"],
            "lesson": {"group": "PI-101"},
        },
    )
    assert create_response.status_code == 202
    job = create_response.json()
    assert job["status"] == JobStatus.QUEUED

    process_response = client.post(f"/attendance/ocr/jobs/{job['job_id']}/process")

    assert process_response.status_code == 200
    processed = process_response.json()
    assert processed["status"] in {JobStatus.COMPLETED, JobStatus.NEEDS_REVIEW}
    assert processed["result"] is not None


def test_process_next_endpoint_runs_fifo_job():
    client = TestClient(app)
    container.job_queue.clear()
    first = client.post(
        "/attendance/ocr/jobs",
        json={
            "image_ids": ["demo_image_first"],
            "lesson": {"group": "PI-101"},
        },
    ).json()
    client.post(
        "/attendance/ocr/jobs",
        json={
            "image_ids": ["demo_image_second"],
            "lesson": {"group": "PI-101"},
        },
    )

    response = client.post("/attendance/ocr/jobs/process-next")

    assert response.status_code == 200
    assert response.json()["job_id"] == first["job_id"]
