from __future__ import annotations

from datetime import datetime

from .models import ProcessingJob


class InMemoryJobRepository:
    def __init__(self) -> None:
        self._jobs: dict[str, ProcessingJob] = {}

    def add(self, job: ProcessingJob) -> ProcessingJob:
        self._jobs[job.job_id] = job
        return job

    def get(self, job_id: str) -> ProcessingJob | None:
        return self._jobs.get(job_id)

    def save(self, job: ProcessingJob) -> ProcessingJob:
        job.updated_at = datetime.utcnow()
        self._jobs[job.job_id] = job
        return job

