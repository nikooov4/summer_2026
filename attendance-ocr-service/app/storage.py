from __future__ import annotations

import json
import warnings
from dataclasses import dataclass
from io import BytesIO
from pathlib import Path

from PIL import Image, UnidentifiedImageError

from .models import StoredImage, new_id
from .settings import settings


class UploadValidationError(ValueError):
    """Raised when uploaded bytes do not match the accepted image policy."""


class UploadTooLargeError(UploadValidationError):
    """Raised when uploaded bytes or decoded pixels exceed configured limits."""


@dataclass(frozen=True)
class LoadedImage:
    meta: StoredImage
    image: Image.Image


class FileImageStorage:
    def __init__(
        self,
        root: Path,
        max_size_bytes: int = settings.max_upload_size_bytes,
        max_pixel_count: int = settings.max_upload_pixel_count,
        allowed_content_types: tuple[str, ...] = settings.allowed_upload_content_types,
        allowed_extensions: tuple[str, ...] = settings.allowed_upload_extensions,
        allowed_formats: tuple[str, ...] = settings.allowed_upload_formats,
    ) -> None:
        self.root = root
        self.max_size_bytes = max_size_bytes
        self.max_pixel_count = max_pixel_count
        self.allowed_content_types = frozenset(item.lower() for item in allowed_content_types)
        self.allowed_extensions = frozenset(item.lower() for item in allowed_extensions)
        self.allowed_formats = frozenset(item.upper() for item in allowed_formats)
        self.root.mkdir(parents=True, exist_ok=True)
        self._index: dict[str, StoredImage] = {}

    def save_upload(self, filename: str, content_type: str, content: bytes) -> StoredImage:
        if len(content) > self.max_size_bytes:
            raise UploadTooLargeError(f"Image is too large: {len(content)} bytes")

        safe_name = Path(filename or "upload.png").name
        declared_content_type = (content_type or "application/octet-stream").split(";", 1)[0].lower()
        extension = Path(safe_name).suffix.lower()
        if declared_content_type not in self.allowed_content_types:
            raise UploadValidationError(f"Unsupported content type: {declared_content_type}")
        if extension not in self.allowed_extensions:
            raise UploadValidationError(f"Unsupported file extension: {extension or '<none>'}")

        image_format, width, height = self._inspect_image(content)
        if image_format not in self.allowed_formats:
            raise UploadValidationError(f"Unsupported image format: {image_format}")
        if not self._extension_matches_format(extension, image_format):
            raise UploadValidationError("File extension does not match image format")
        if not self._content_type_matches_format(declared_content_type, image_format):
            raise UploadValidationError("Content type does not match image format")
        if width * height > self.max_pixel_count:
            raise UploadTooLargeError(f"Image has too many pixels: {width * height}")

        image = Image.open(BytesIO(content)).convert("RGB")
        image_id = new_id("image")
        path = self._path_for(image_id)
        image.save(path, format="PNG")

        meta = StoredImage(
            image_id=image_id,
            filename=safe_name,
            content_type=content_type or "application/octet-stream",
            size_bytes=path.stat().st_size,
            width=width,
            height=height,
        )
        self._index[image_id] = meta
        self._save_meta(meta)
        return meta

    def _inspect_image(self, content: bytes) -> tuple[str, int, int]:
        try:
            with warnings.catch_warnings():
                warnings.simplefilter("error", Image.DecompressionBombWarning)
                with Image.open(BytesIO(content)) as image:
                    image_format = image.format or ""
                    width, height = image.size
                    image.verify()
        except Image.DecompressionBombWarning as exc:
            raise UploadTooLargeError("Image exceeds decompression bomb safety limits") from exc
        except (OSError, UnidentifiedImageError) as exc:
            raise UploadValidationError("Uploaded file is not a valid image") from exc

        image_format = image_format.upper()
        if not image_format:
            raise UploadValidationError("Uploaded image format is missing")
        return image_format, width, height

    def _extension_matches_format(self, extension: str, image_format: str) -> bool:
        expected = {
            ".png": "PNG",
            ".jpg": "JPEG",
            ".jpeg": "JPEG",
        }
        return expected.get(extension) == image_format

    def _content_type_matches_format(self, content_type: str, image_format: str) -> bool:
        expected = {
            "image/png": "PNG",
            "image/jpeg": "JPEG",
        }
        return expected.get(content_type) == image_format

    def get_meta(self, image_id: str) -> StoredImage | None:
        meta = self._index.get(image_id)
        if meta is not None:
            return meta
        meta = self._load_meta(image_id)
        if meta is not None:
            self._index[image_id] = meta
        return meta

    def load(self, image_id: str) -> LoadedImage:
        meta = self.get_meta(image_id)
        if meta is None:
            raise KeyError(image_id)
        image = Image.open(self._path_for(image_id)).convert("RGB")
        return LoadedImage(meta=meta, image=image)

    def _path_for(self, image_id: str) -> Path:
        return self.root / f"{image_id}.png"

    def _meta_path_for(self, image_id: str) -> Path:
        return self.root / f"{image_id}.json"

    def _save_meta(self, meta: StoredImage) -> None:
        self._meta_path_for(meta.image_id).write_text(_stored_image_to_json(meta), encoding="utf-8")

    def _load_meta(self, image_id: str) -> StoredImage | None:
        meta_path = self._meta_path_for(image_id)
        if not meta_path.exists():
            return None
        return _stored_image_from_json(meta_path.read_text(encoding="utf-8"))


def _stored_image_to_json(meta: StoredImage) -> str:
    if hasattr(meta, "model_dump_json"):
        return meta.model_dump_json()
    return meta.json(ensure_ascii=False)


def _stored_image_from_json(payload: str) -> StoredImage:
    if hasattr(StoredImage, "model_validate_json"):
        return StoredImage.model_validate_json(payload)
    return StoredImage.parse_obj(json.loads(payload))
