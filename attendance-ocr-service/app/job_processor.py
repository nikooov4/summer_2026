from __future__ import annotations

from .adapters import LmsAdapter, OcrAdapter
from .matching import match_name_regions
from .models import AttendanceResult, JobStatus, ProcessingJob
from .ports import JobRepository
from .review import build_review_from_result


class JobProcessor:
    def __init__(self, repository: JobRepository, ocr: OcrAdapter, lms: LmsAdapter) -> None:
        self.repository = repository
        self.ocr = ocr
        self.lms = lms

    def process_job(self, job_id: str) -> ProcessingJob:
        job = self._require_job(job_id)
        job.status = JobStatus.PROCESSING
        self.repository.save(job)

        try:
            students = job.students if job.students is not None else self.lms.get_students(job.lesson)
            ocr_results = self.ocr.recognize_for_students(job.image_ids, job.lesson, students)
            regions = [region for result in ocr_results for region in result.regions]
            recognized_students = match_name_regions(regions, students)
            conflicts = [
                student.source_text
                for student in recognized_students
                if student.status.value == "needs_review"
            ]
            status = JobStatus.NEEDS_REVIEW if conflicts else JobStatus.COMPLETED
            job.result = AttendanceResult(
                job_id=job.job_id,
                status=status,
                lesson=job.lesson,
                students=recognized_students,
                ocr=ocr_results,
                conflicts=conflicts,
            )
            job.review = build_review_from_result(job.result)
            job.status = status
        except Exception as exc:
            job.status = JobStatus.FAILED
            job.error = str(exc)

        return self.repository.save(job)

    def _require_job(self, job_id: str) -> ProcessingJob:
        job = self.repository.get(job_id)
        if job is None:
            raise KeyError(job_id)
        return job
