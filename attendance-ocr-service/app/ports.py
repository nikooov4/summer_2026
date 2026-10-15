from __future__ import annotations

from typing import Protocol

from .models import ProcessingJob, StoredImage
from .storage import LoadedImage


class JobRepository(Protocol):
    def add(self, job: ProcessingJob) -> ProcessingJob:
        """Persist a newly created job."""

    def get(self, job_id: str) -> ProcessingJob | None:
        """Return a job by id, if it exists."""

    def save(self, job: ProcessingJob) -> ProcessingJob:
        """Persist changes to an existing job."""


class ImageStorage(Protocol):
    def save_upload(self, filename: str, content_type: str, content: bytes) -> StoredImage:
        """Validate and store uploaded image bytes."""

    def get_meta(self, image_id: str) -> StoredImage | None:
        """Return stored image metadata by id."""

    def load(self, image_id: str) -> LoadedImage:
        """Load stored image metadata and pixels by id."""
