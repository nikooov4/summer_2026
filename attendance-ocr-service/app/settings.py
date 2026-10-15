from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path
import os


APP_DIR = Path(__file__).resolve().parent
PROJECT_DIR = APP_DIR.parent
WORKSPACE_DIR = PROJECT_DIR.parent
LEGACY_ROOT = Path(os.getenv("OCR_LEGACY_ROOT", str(WORKSPACE_DIR / "Исходники_OCR_version_1" / "OCR-based-attendance-tracking-system")))
NASLEDIE_ROOT = Path(os.getenv("OCR_NASLEDIE_ROOT", str(WORKSPACE_DIR / "Исходники_nasledie" / "nasledie")))


@dataclass(frozen=True)
class Settings:
    allowed_origins: tuple[str, ...] = tuple(
        value.strip() for value in os.getenv("OCR_ALLOWED_ORIGINS", "http://localhost:5293,http://127.0.0.1:5293").split(",") if value.strip()
    )
    app_title: str = "Attendance OCR Service Prototype"
    app_version: str = "0.1.0"
    static_dir: Path = APP_DIR / "static"
    upload_dir: Path = PROJECT_DIR / "data" / "uploads"
    sqlite_database_path: Path = PROJECT_DIR / "data" / "attendance_ocr.sqlite3"
    max_upload_size_bytes: int = 12 * 1024 * 1024
    max_upload_pixel_count: int = 24_000_000
    allowed_upload_content_types: tuple[str, ...] = ("image/png", "image/jpeg")
    allowed_upload_extensions: tuple[str, ...] = (".png", ".jpg", ".jpeg")
    allowed_upload_formats: tuple[str, ...] = ("PNG", "JPEG")
    demo_group: str = "PI-101"
    legacy_root: Path = LEGACY_ROOT
    legacy_detection_model_path: Path = Path(os.getenv("OCR_LEGACY_DETECTION_MODEL", str(LEGACY_ROOT / "data/models/production/detection/opt_model.pt")))
    legacy_recognition_model_path: Path = Path(os.getenv("OCR_LEGACY_RECOGNITION_MODEL", str(LEGACY_ROOT / "data/models/production/recognition")))
    nasledie_root: Path = NASLEDIE_ROOT
    nasledie_models_dir: Path = Path(os.getenv("OCR_NASLEDIE_MODELS_DIR", str(NASLEDIE_ROOT / "prod_models")))
    nasledie_recognizer_model_name: str = "raxtemur/trocr-base-ru"
    nasledie_device: str = os.getenv("OCR_DEVICE", "cpu")


settings = Settings()
