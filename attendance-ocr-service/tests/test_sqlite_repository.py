from app.models import JobStatus, LessonContext, ProcessingJob
from app.sqlite_repository import SQLiteJobRepository


def test_sqlite_repository_persists_jobs_across_instances(tmp_path):
    database_path = tmp_path / "attendance_ocr.sqlite3"
    repository = SQLiteJobRepository(database_path)
    job = ProcessingJob(
        image_ids=["image_001"],
        lesson=LessonContext(group="PI-101"),
        status=JobStatus.QUEUED,
    )

    repository.add(job)

    reloaded_repository = SQLiteJobRepository(database_path)
    reloaded = reloaded_repository.get(job.job_id)

    assert reloaded is not None
    assert reloaded.job_id == job.job_id
    assert reloaded.status == JobStatus.QUEUED
    assert reloaded.image_ids == ["image_001"]
    assert reloaded.lesson.group == "PI-101"
