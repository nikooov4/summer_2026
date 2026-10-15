from __future__ import annotations

from .adapters import (
    DemoCaseLmsAdapter,
    DemoCaseOcrAdapter,
    LegacyV1LivePipelineAdapter,
    NasledieLivePipelineAdapter,
    StorageBackedLegacyV1OcrAdapter,
    StorageBackedNasledieOcrAdapter,
    StubLmsAdapter,
    StubOcrAdapter,
)
from .job_processor import JobProcessor
from .job_queue import InMemoryJobQueue
from .ports import ImageStorage, JobRepository
from .service import AttendanceProcessingService
from .settings import Settings, settings
from .sqlite_repository import SQLiteJobRepository
from .storage import FileImageStorage


class AppContainer:
    def __init__(self, app_settings: Settings) -> None:
        self.settings = app_settings
        self.ocr = StubOcrAdapter()
        self.repository: JobRepository = SQLiteJobRepository(app_settings.sqlite_database_path)
        self.job_queue = InMemoryJobQueue()
        self.lms = StubLmsAdapter()
        self.image_storage: ImageStorage = FileImageStorage(
            app_settings.upload_dir,
            max_size_bytes=app_settings.max_upload_size_bytes,
        )
        self.service = AttendanceProcessingService(self.repository, self.job_queue)
        self.job_processor = JobProcessor(self.repository, self.ocr, self.lms)
        self.demo_lms = DemoCaseLmsAdapter()
        self.demo_ocr = DemoCaseOcrAdapter()
        self.demo_job_processor = JobProcessor(self.repository, self.demo_ocr, self.demo_lms)
        self.legacy_v1_pipeline = LegacyV1LivePipelineAdapter(
            legacy_root=app_settings.legacy_root,
            detection_model_path=app_settings.legacy_detection_model_path,
            recognition_model_path=app_settings.legacy_recognition_model_path,
        )
        self.legacy_ocr = StorageBackedLegacyV1OcrAdapter(
            image_storage=self.image_storage,
            lms=self.lms,
            pipeline=self.legacy_v1_pipeline,
        )
        self.legacy_job_processor = JobProcessor(self.repository, self.legacy_ocr, self.lms)
        self.nasledie_pipeline = NasledieLivePipelineAdapter(
            nasledie_root=app_settings.nasledie_root,
            models_dir=app_settings.nasledie_models_dir,
            recognizer_model_name=app_settings.nasledie_recognizer_model_name,
            device=app_settings.nasledie_device,
        )
        self.nasledie_ocr = StorageBackedNasledieOcrAdapter(
            image_storage=self.image_storage,
            lms=self.lms,
            pipeline=self.nasledie_pipeline,
        )
        self.nasledie_job_processor = JobProcessor(self.repository, self.nasledie_ocr, self.lms)


container = AppContainer(settings)


def get_container() -> AppContainer:
    return container
