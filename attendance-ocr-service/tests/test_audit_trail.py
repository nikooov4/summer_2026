from app.adapters import StubLmsAdapter, StubOcrAdapter
from app.job_processor import JobProcessor
from app.job_queue import InMemoryJobQueue
from app.models import AuditEventType, JobStatus, LessonContext, ReviewAction, UpdateReviewRequest
from app.repository import InMemoryJobRepository
from app.service import AttendanceProcessingService
from app.sqlite_repository import SQLiteJobRepository


def test_job_created_audit_event_persists_without_schema_migration(tmp_path):
    database_path = tmp_path / "attendance_ocr.sqlite3"
    service = AttendanceProcessingService(
        SQLiteJobRepository(database_path),
        InMemoryJobQueue(),
    )

    job = service.create_job(
        ["image_001", "image_002"],
        LessonContext(group="PI-101", subject="Math"),
    )

    reloaded = SQLiteJobRepository(database_path).get(job.job_id)

    assert reloaded is not None
    assert [event.event_type for event in reloaded.audit_events] == [
        AuditEventType.JOB_CREATED
    ]
    event = reloaded.audit_events[0]
    assert event.job_id == job.job_id
    assert event.metadata == {"image_count": 2, "lesson_present": True}


def test_review_correction_audit_event_keeps_pii_out_of_metadata():
    service, processor = _service_with_processor()
    job = service.create_job(["demo_image_001"], LessonContext(group="PI-101"))
    processor.process_job(job.job_id)
    review = service.get_review(job.job_id)
    decision = review.decisions[0]

    service.update_review(
        job.job_id,
        UpdateReviewRequest(
            corrections=[
                {
                    "decision_id": decision.decision_id,
                    "action": ReviewAction.CORRECT_TEXT,
                    "corrected_text": "Maria Petrova",
                    "comment": "teacher-entered name",
                }
            ]
        ),
    )

    saved = service.get_job(job.job_id)
    event = saved.audit_events[-1]

    assert event.event_type == AuditEventType.REVIEW_CORRECTED
    assert event.metadata["correction_count"] == 1
    assert event.metadata["corrections"] == [
        {"decision_id": decision.decision_id, "action": ReviewAction.CORRECT_TEXT.value}
    ]
    assert "Maria" not in str(event.metadata)
    assert "teacher-entered name" not in str(event.metadata)


def test_confirm_and_export_append_service_level_audit_events():
    service, processor = _service_with_processor()
    job = service.create_job(["demo_image_001"], LessonContext(group="PI-101"))
    processor.process_job(job.job_id)
    review = service.get_review(job.job_id)
    unresolved = [decision for decision in review.decisions if not decision.resolved]

    if unresolved:
        service.update_review(
            job.job_id,
            UpdateReviewRequest(
                corrections=[
                    {
                        "decision_id": decision.decision_id,
                        "action": ReviewAction.IGNORE,
                    }
                    for decision in unresolved
                ]
            ),
        )

    confirmed = service.confirm_review(job.job_id)
    export = service.export_job(job.job_id)
    saved = service.get_job(job.job_id)

    assert confirmed.status == JobStatus.COMPLETED
    assert export.status == "exported"
    assert [event.event_type for event in saved.audit_events] == [
        AuditEventType.JOB_CREATED,
        *([AuditEventType.REVIEW_CORRECTED] if unresolved else []),
        AuditEventType.CONFIRMED,
        AuditEventType.EXPORTED,
    ]


def _service_with_processor():
    repository = InMemoryJobRepository()
    service = AttendanceProcessingService(repository, InMemoryJobQueue())
    processor = JobProcessor(repository, StubOcrAdapter(), StubLmsAdapter())
    return service, processor
