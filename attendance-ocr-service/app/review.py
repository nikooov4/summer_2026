from __future__ import annotations

from .models import (
    AttendanceDecision,
    AttendanceResult,
    AttendanceStatus,
    DecisionSource,
    JobReview,
    JobStatus,
    ReviewAction,
    ReviewCorrection,
    ReviewSummary,
    Student,
)


def build_review_from_result(result: AttendanceResult) -> JobReview:
    decisions = [_decision_from_recognized_student(student) for student in result.students]
    return JobReview(
        job_id=result.job_id,
        status=result.status,
        lesson=result.lesson,
        decisions=decisions,
        summary=summarize_decisions(decisions),
        ocr=result.ocr,
    )


def apply_corrections(
    review: JobReview,
    corrections: list[ReviewCorrection],
    roster: list[Student],
) -> JobReview:
    roster_by_id = {student.student_id: student for student in roster}
    decisions_by_id = {decision.decision_id: decision for decision in review.decisions}

    for correction in corrections:
        decision = decisions_by_id.get(correction.decision_id)
        if decision is None:
            continue
        _apply_correction(decision, correction, roster_by_id)

    review.summary = summarize_decisions(review.decisions)
    review.status = JobStatus.COMPLETED if review.summary.needs_review_count == 0 else JobStatus.NEEDS_REVIEW
    return review


def summarize_decisions(decisions: list[AttendanceDecision]) -> ReviewSummary:
    return ReviewSummary(
        auto_present_count=sum(
            1
            for decision in decisions
            if decision.status == AttendanceStatus.PRESENT and decision.source == DecisionSource.AUTO
        ),
        needs_review_count=sum(1 for decision in decisions if not decision.resolved),
        absent_count=sum(1 for decision in decisions if decision.status == AttendanceStatus.ABSENT),
        unmatched_count=sum(
            1
            for decision in decisions
            if decision.student_id is None and decision.status == AttendanceStatus.NEEDS_REVIEW
        ),
        confirmed_count=sum(1 for decision in decisions if decision.resolved),
    )


def _decision_from_recognized_student(student) -> AttendanceDecision:
    resolved = student.status == AttendanceStatus.PRESENT
    return AttendanceDecision(
        student_id=student.student_id,
        status=student.status,
        source=DecisionSource.AUTO,
        region_ids=student.region_ids,
        confidence=student.confidence,
        final_full_name=student.full_name,
        source_text=student.source_text,
        candidates=student.candidates,
        resolved=resolved,
    )


def _apply_correction(
    decision: AttendanceDecision,
    correction: ReviewCorrection,
    roster_by_id: dict[str, Student],
) -> None:
    decision.source = DecisionSource.TEACHER

    if correction.action == ReviewAction.ACCEPT:
        decision.status = AttendanceStatus.PRESENT
        decision.resolved = True
        return

    if correction.action == ReviewAction.SELECT_STUDENT:
        selected = roster_by_id.get(correction.selected_student_id or "")
        if selected is None:
            decision.status = AttendanceStatus.NEEDS_REVIEW
            decision.resolved = False
            return
        decision.student_id = selected.student_id
        decision.final_full_name = selected.full_name
        decision.status = AttendanceStatus.PRESENT
        decision.resolved = True
        return

    if correction.action == ReviewAction.CORRECT_TEXT:
        decision.source_text = correction.corrected_text or decision.source_text
        decision.status = AttendanceStatus.NEEDS_REVIEW
        decision.resolved = False
        return

    if correction.action == ReviewAction.MARK_ABSENT:
        decision.status = AttendanceStatus.ABSENT
        decision.resolved = True
        return

    if correction.action == ReviewAction.NOT_A_STUDENT:
        decision.student_id = None
        decision.final_full_name = None
        decision.status = AttendanceStatus.NOT_A_STUDENT
        decision.resolved = True
        return

    if correction.action == ReviewAction.IGNORE:
        decision.status = AttendanceStatus.IGNORED
        decision.resolved = True
