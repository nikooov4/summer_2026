from io import BytesIO

from PIL import Image

from app.storage import FileImageStorage


def test_file_image_storage_persists_metadata_across_instances(tmp_path):
    image_bytes = BytesIO()
    Image.new("RGB", (320, 200), "white").save(image_bytes, format="PNG")

    storage = FileImageStorage(tmp_path)
    stored = storage.save_upload(
        filename="sheet.png",
        content_type="image/png",
        content=image_bytes.getvalue(),
    )

    reloaded_storage = FileImageStorage(tmp_path)
    reloaded = reloaded_storage.load(stored.image_id)

    assert reloaded.meta == stored
    assert reloaded.image.size == (320, 200)
