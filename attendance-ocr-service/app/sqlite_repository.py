from __future__ import annotations

import sqlite3
from pathlib import Path

from .models import ProcessingJob


class SQLiteJobRepository:
    def __init__(self, database_path: Path) -> None:
        self.database_path = database_path
        self.database_path.parent.mkdir(parents=True, exist_ok=True)
        self._ensure_schema()

    def add(self, job: ProcessingJob) -> ProcessingJob:
        self._upsert(job)
        return job

    def get(self, job_id: str) -> ProcessingJob | None:
        with self._connect() as connection:
            row = connection.execute(
                "SELECT payload FROM jobs WHERE job_id = ?",
                (job_id,),
            ).fetchone()
        if row is None:
            return None
        return _job_from_json(row[0])

    def save(self, job: ProcessingJob) -> ProcessingJob:
        self._upsert(job)
        return job

    def _upsert(self, job: ProcessingJob) -> None:
        with self._connect() as connection:
            connection.execute(
                """
                INSERT INTO jobs (job_id, payload, updated_at)
                VALUES (?, ?, ?)
                ON CONFLICT(job_id) DO UPDATE SET
                    payload = excluded.payload,
                    updated_at = excluded.updated_at
                """,
                (job.job_id, _job_to_json(job), job.updated_at.isoformat()),
            )

    def _ensure_schema(self) -> None:
        with self._connect() as connection:
            connection.execute(
                """
                CREATE TABLE IF NOT EXISTS jobs (
                    job_id TEXT PRIMARY KEY,
                    payload TEXT NOT NULL,
                    updated_at TEXT NOT NULL
                )
                """
            )

    def _connect(self) -> sqlite3.Connection:
        return sqlite3.connect(self.database_path)


def _job_to_json(job: ProcessingJob) -> str:
    if hasattr(job, "model_dump_json"):
        return job.model_dump_json()
    return job.json()


def _job_from_json(payload: str) -> ProcessingJob:
    if hasattr(ProcessingJob, "model_validate_json"):
        return ProcessingJob.model_validate_json(payload)
    return ProcessingJob.parse_raw(payload)
