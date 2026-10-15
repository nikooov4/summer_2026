from __future__ import annotations

from datetime import datetime
from enum import Enum
from typing import Any
from uuid import uuid4

from pydantic import BaseModel, Field


def new_id(prefix: str) -> str:
    return f"{prefix}_{uuid4().hex}"


class JobStatus(str, Enum):
    CREATED = "created"
    QUEUED = "queued"
    PROCESSING = "processing"
    NEEDS_REVIEW = "needs_review"
    COMPLETED = "completed"
    FAILED = "failed"


class RegionType(str, Enum):
    NAME = "name"
    DATE = "date"
    SUBJECT = "subject"
    GROUP = "group"
    NUMBER = "number"
    COMMENT = "comment"
    UNKNOWN = "unknown"


class AttendanceStatus(str, Enum):
    PRESENT = "present"
    ABSENT = "absent"
    NEEDS_REVIEW = "needs_review"
    IGNORED = "ignored"
    NOT_A_STUDENT = "not_a_student"


class DecisionSource(str, Enum):
    AUTO = "auto"
    TEACHER = "teacher"
    LMS = "lms"


class ReviewAction(str, Enum):
    ACCEPT = "accept"
    SELECT_STUDENT = "select_student"
    CORRECT_TEXT = "correct_text"
    MARK_ABSENT = "mark_absent"
    NOT_A_STUDENT = "not_a_student"
    IGNORE = "ignore"


class AuditEventType(str, Enum):
    UPLOADED = "uploaded"
    JOB_CREATED = "job_created"
    PROCESSED = "processed"
    REVIEW_CORRECTED = "review_corrected"
    CONFIRMED = "confirmed"
    EXPORTED = "exported"
    FAILED = "failed"


class BoundingBox(BaseModel):
    x1: float
    y1: float
    x2: float
    y2: float


class OcrRegion(BaseModel):
    region_id: str = Field(default_factory=lambda: new_id("region"))
    type: RegionType = RegionType.UNKNOWN
    text: str = ""
    confidence: float = Field(default=0.0, ge=0.0, le=1.0)
    bbox: BoundingBox | None = None
    raw: dict[str, Any] = Field(default_factory=dict)


class OcrImageResult(BaseModel):
    image_id: str
    regions: list[OcrRegion] = Field(default_factory=list)
    raw: dict[str, Any] = Field(default_factory=dict)


class StoredImage(BaseModel):
    image_id: str
    filename: str
    content_type: str
    size_bytes: int
    width: int
    height: int


class Student(BaseModel):
    student_id: str
    full_name: str
    group: str | None = None


class LessonContext(BaseModel):
    lesson_id: str | None = None
    date: str | None = None
    subject: str | None = None
    group: str | None = None
    teacher_id: str | None = None


class RecognizedStudent(BaseModel):
    student_id: str | None
    full_name: str | None
    source_text: str
    confidence: float = Field(ge=0.0)
    status: AttendanceStatus
    region_ids: list[str] = Field(default_factory=list)
    candidates: list[Student] = Field(default_factory=list)


class AttendanceResult(BaseModel):
    job_id: str
    status: JobStatus
    lesson: LessonContext
    students: list[RecognizedStudent] = Field(default_factory=list)
    ocr: list[OcrImageResult] = Field(default_factory=list)
    conflicts: list[str] = Field(default_factory=list)


class AttendanceDecision(BaseModel):
    decision_id: str = Field(default_factory=lambda: new_id("decision"))
    student_id: str | None = None
    status: AttendanceStatus
    source: DecisionSource = DecisionSource.AUTO
    region_ids: list[str] = Field(default_factory=list)
    confidence: float = Field(default=0.0, ge=0.0)
    final_full_name: str | None = None
    source_text: str = ""
    candidates: list[Student] = Field(default_factory=list)
    resolved: bool = False


class ReviewCorrection(BaseModel):
    decision_id: str
    action: ReviewAction
    image_id: str | None = None
    region_id: str | None = None
    selected_student_id: str | None = None
    corrected_text: str | None = None
    comment: str | None = None


class ReviewSummary(BaseModel):
    auto_present_count: int = 0
    needs_review_count: int = 0
    absent_count: int = 0
    unmatched_count: int = 0
    confirmed_count: int = 0


class JobReview(BaseModel):
    job_id: str
    status: JobStatus
    lesson: LessonContext
    decisions: list[AttendanceDecision] = Field(default_factory=list)
    summary: ReviewSummary = Field(default_factory=ReviewSummary)
    ocr: list[OcrImageResult] = Field(default_factory=list)


class UpdateReviewRequest(BaseModel):
    corrections: list[ReviewCorrection] = Field(default_factory=list)


class LmsExportResult(BaseModel):
    export_id: str = Field(default_factory=lambda: new_id("export"))
    job_id: str
    status: str = "exported"
    target: str = "mock_lms"
    external_reference: str | None = None
    error: str | None = None


class AuditEvent(BaseModel):
    event_id: str = Field(default_factory=lambda: new_id("audit"))
    event_type: AuditEventType
    job_id: str | None = None
    created_at: datetime = Field(default_factory=datetime.utcnow)
    actor_id: str | None = None
    metadata: dict[str, Any] = Field(default_factory=dict)


class ProcessingJob(BaseModel):
    job_id: str = Field(default_factory=lambda: new_id("job"))
    status: JobStatus = JobStatus.CREATED
    created_at: datetime = Field(default_factory=datetime.utcnow)
    updated_at: datetime = Field(default_factory=datetime.utcnow)
    lesson: LessonContext = Field(default_factory=LessonContext)
    image_ids: list[str] = Field(default_factory=list)
    students: list[Student] | None = None
    result: AttendanceResult | None = None
    review: JobReview | None = None
    error: str | None = None
    audit_events: list[AuditEvent] = Field(default_factory=list)


class CreateJobRequest(BaseModel):
    image_ids: list[str] = Field(min_length=1)
    lesson: LessonContext = Field(default_factory=LessonContext)
    students: list[Student] | None = Field(default=None, max_length=2000)


class ConfirmAttendanceRequest(BaseModel):
    students: list[RecognizedStudent]
