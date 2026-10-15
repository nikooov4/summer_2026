from __future__ import annotations

from .job_queue import JobQueue
from .models import (
    AuditEvent,
    AuditEventType,
    JobReview,
    JobStatus,
    LessonContext,
    LmsExportResult,
    ProcessingJob,
    ReviewCorrection,
    Student,
    UpdateReviewRequest,
)
from .ports import JobRepository
from .review import apply_corrections, build_review_from_result


class AttendanceProcessingService:
    def __init__(self, repository: JobRepository, queue: JobQueue) -> None:
        self.repository = repository
        self.queue = queue

    def create_job(self, image_ids: list[str], lesson: LessonContext, students: list[Student] | None = None) -> ProcessingJob:
        job = ProcessingJob(image_ids=image_ids, lesson=lesson, students=students, status=JobStatus.QUEUED)
        self._append_audit_event(
            job,
            AuditEventType.JOB_CREATED,
            metadata={
                "image_count": len(image_ids),
                "lesson_present": bool(
                    lesson.lesson_id
                    or lesson.date
                    or lesson.subject
                    or lesson.group
                    or lesson.teacher_id
                ),
            },
        )
        self.repository.add(job)
        self.queue.enqueue(job.job_id)
        return job

    def get_job(self, job_id: str) -> ProcessingJob | None:
        return self.repository.get(job_id)

    def confirm_job(self, job_id: str) -> ProcessingJob:
        job = self._require_job(job_id)
        if job.result is None:
            raise ValueError("Cannot confirm job without result")
        review = self.get_review(job_id)
        if review.summary.needs_review_count:
            raise ValueError("Cannot confirm job with unresolved review decisions")
        job.status = JobStatus.COMPLETED
        job.result.status = JobStatus.COMPLETED
        job.result.conflicts = []
        job.review = review
        job.review.status = JobStatus.COMPLETED
        self._append_audit_event(
            job,
            AuditEventType.CONFIRMED,
            metadata={"decision_count": len(review.decisions)},
        )
        return self.repository.save(job)

    def get_review(self, job_id: str) -> JobReview:
        job = self._require_job(job_id)
        if job.result is None:
            raise ValueError("Cannot review job without result")
        if job.review is None:
            job.review = build_review_from_result(job.result)
            self.repository.save(job)
        return job.review

    def update_review(self, job_id: str, request: UpdateReviewRequest) -> JobReview:
        job = self._require_job(job_id)
        review = self.get_review(job_id)
        decision_ids = {decision.decision_id for decision in review.decisions}
        unknown_ids = [
            correction.decision_id
            for correction in request.corrections
            if correction.decision_id not in decision_ids
        ]
        if unknown_ids:
            raise ValueError(f"Unknown review decision: {unknown_ids[0]}")
        roster = job.students if job.students is not None else self._review_roster(review)
        review = apply_corrections(review, request.corrections, roster)
        job.review = review
        if request.corrections:
            self._append_audit_event(
                job,
                AuditEventType.REVIEW_CORRECTED,
                metadata={
                    "correction_count": len(request.corrections),
                    "corrections": [
                        self._audit_correction_reference(correction)
                        for correction in request.corrections
                    ],
                    "remaining_needs_review_count": review.summary.needs_review_count,
                },
            )
        if job.result is not None:
            job.result.status = review.status
            job.result.conflicts = [
                decision.source_text
                for decision in review.decisions
                if not decision.resolved
            ]
        job.status = review.status
        self.repository.save(job)
        return review

    def confirm_review(self, job_id: str) -> ProcessingJob:
        return self.confirm_job(job_id)

    def export_job(self, job_id: str) -> LmsExportResult:
        job = self._require_job(job_id)
        if job.status != JobStatus.COMPLETED:
            raise ValueError("Cannot export job before it is completed")
        export = LmsExportResult(
            job_id=job.job_id,
            external_reference=f"mock_lms:{job.job_id}",
        )
        self._append_audit_event(
            job,
            AuditEventType.EXPORTED,
            metadata={"target": export.target, "status": export.status},
        )
        self.repository.save(job)
        return export

    def _require_job(self, job_id: str) -> ProcessingJob:
        job = self.repository.get(job_id)
        if job is None:
            raise KeyError(job_id)
        return job

    def _review_roster(self, review: JobReview):
        roster_by_id = {}
        for decision in review.decisions:
            for candidate in decision.candidates:
                roster_by_id[candidate.student_id] = candidate
        return list(roster_by_id.values())

    def _append_audit_event(
        self,
        job: ProcessingJob,
        event_type: AuditEventType,
        metadata: dict | None = None,
    ) -> None:
        job.audit_events.append(
            AuditEvent(
                event_type=event_type,
                job_id=job.job_id,
                metadata=metadata or {},
            )
        )

    def _audit_correction_reference(self, correction: ReviewCorrection) -> dict[str, str]:
        reference = {
            "decision_id": correction.decision_id,
            "action": correction.action.value,
        }
        if correction.image_id:
            reference["image_id"] = correction.image_id
        if correction.region_id:
            reference["region_id"] = correction.region_id
        return reference
