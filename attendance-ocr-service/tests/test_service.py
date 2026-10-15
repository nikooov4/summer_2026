from app.adapters import StubLmsAdapter, StubOcrAdapter
from app.job_processor import JobProcessor
from app.job_queue import InMemoryJobQueue
from app.models import JobStatus, LessonContext
from app.repository import InMemoryJobRepository
from app.service import AttendanceProcessingService


def test_create_job_queues_work_without_processing_immediately():
    queue = InMemoryJobQueue()
    repository = InMemoryJobRepository()
    service = AttendanceProcessingService(
        repository,
        queue,
    )

    job = service.create_job(["demo_image_001"], LessonContext(group="PI-101"))

    assert job.status == JobStatus.QUEUED
    assert job.result is None
    assert queue.pending() == [job.job_id]


def test_job_processor_runs_stub_flow():
    repository = InMemoryJobRepository()
    service = AttendanceProcessingService(repository, InMemoryJobQueue())
    processor = JobProcessor(repository, StubOcrAdapter(), StubLmsAdapter())

    job = service.create_job(["demo_image_001"], LessonContext(group="PI-101"))
    processed = processor.process_job(job.job_id)

    assert processed.status in {JobStatus.COMPLETED, JobStatus.NEEDS_REVIEW}
    assert processed.result is not None
    assert len(processed.result.ocr) == 1
    assert len(processed.result.students) >= 1


def test_job_processor_marks_failed_on_ocr_error():
    class BrokenOcrAdapter(StubOcrAdapter):
        def recognize(self, image_ids: list[str], lesson=None):
            raise RuntimeError("OCR exploded")

    repository = InMemoryJobRepository()
    service = AttendanceProcessingService(repository, InMemoryJobQueue())
    processor = JobProcessor(repository, BrokenOcrAdapter(), StubLmsAdapter())

    job = service.create_job(["demo_image_001"], LessonContext(group="PI-101"))
    processed = processor.process_job(job.job_id)

    assert processed.status == JobStatus.FAILED
    assert processed.result is None
    assert processed.error == "OCR exploded"
