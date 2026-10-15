from io import BytesIO

from fastapi.testclient import TestClient
from PIL import Image

from app.main import app


def test_demo_process_image_returns_regions_and_students():
    client = TestClient(app)
    image_bytes = BytesIO()
    Image.new("RGB", (800, 600), "white").save(image_bytes, format="PNG")
    image_bytes.seek(0)

    response = client.post(
        "/demo/process-image",
        files={"file": ("sheet.png", image_bytes, "image/png")},
    )

    assert response.status_code == 200
    payload = response.json()
    assert payload["mode"] == "stub_demo"
    assert len(payload["ocr"]["regions"]) >= 1
    assert len(payload["students"]) >= 1


def test_upload_image_returns_stored_image_id():
    client = TestClient(app)
    image_bytes = BytesIO()
    Image.new("RGB", (320, 200), "white").save(image_bytes, format="PNG")
    image_bytes.seek(0)

    response = client.post(
        "/attendance/ocr/images",
        files={"file": ("sheet.png", image_bytes, "image/png")},
    )

    assert response.status_code == 201
    payload = response.json()
    assert payload["image_id"].startswith("image_")
    assert payload["width"] == 320
    assert payload["height"] == 200


def test_demo_pipeline_status_reports_legacy_v1_availability():
    client = TestClient(app)
    response = client.get("/demo/pipeline-status")

    assert response.status_code == 200
    payload = response.json()
    assert payload["stub_demo_ready"] is True
    assert isinstance(payload["legacy_v1_configured"], bool)
    assert isinstance(payload["legacy_v1_errors"], list)
