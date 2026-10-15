from io import BytesIO

from fastapi.testclient import TestClient
from PIL import Image

from app.main import app
from app.storage import FileImageStorage, UploadTooLargeError


def _image_bytes(image_format: str = "PNG", size: tuple[int, int] = (320, 200)) -> BytesIO:
    image_bytes = BytesIO()
    Image.new("RGB", size, "white").save(image_bytes, format=image_format)
    image_bytes.seek(0)
    return image_bytes


def test_upload_accepts_valid_png_image():
    client = TestClient(app)

    response = client.post(
        "/attendance/ocr/images",
        files={"file": ("sheet.png", _image_bytes("PNG"), "image/png")},
    )

    assert response.status_code == 201
    payload = response.json()
    assert payload["image_id"].startswith("image_")
    assert payload["filename"] == "sheet.png"
    assert payload["content_type"] == "image/png"
    assert payload["width"] == 320
    assert payload["height"] == 200


def test_upload_rejects_unsupported_content_type():
    client = TestClient(app)

    response = client.post(
        "/attendance/ocr/images",
        files={"file": ("sheet.png", _image_bytes("PNG"), "application/octet-stream")},
    )

    assert response.status_code == 400
    assert "Unsupported content type" in response.json()["detail"]


def test_upload_rejects_extension_format_mismatch():
    client = TestClient(app)

    response = client.post(
        "/attendance/ocr/images",
        files={"file": ("sheet.jpg", _image_bytes("PNG"), "image/jpeg")},
    )

    assert response.status_code == 400
    assert "extension does not match" in response.json()["detail"]


def test_upload_rejects_payload_over_byte_limit():
    client = TestClient(app)
    oversized = BytesIO(b"x" * (12 * 1024 * 1024 + 1))

    response = client.post(
        "/attendance/ocr/images",
        files={"file": ("sheet.png", oversized, "image/png")},
    )

    assert response.status_code == 413
    assert "too large" in response.json()["detail"]


def test_storage_rejects_payload_over_pixel_limit(tmp_path):
    storage = FileImageStorage(tmp_path, max_pixel_count=100)

    try:
        storage.save_upload(
            filename="sheet.png",
            content_type="image/png",
            content=_image_bytes("PNG", size=(11, 10)).getvalue(),
        )
    except UploadTooLargeError as exc:
        assert "too many pixels" in str(exc)
    else:
        raise AssertionError("Expected UploadTooLargeError")
