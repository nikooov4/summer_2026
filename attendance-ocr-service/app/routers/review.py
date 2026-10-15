from __future__ import annotations

from fastapi import APIRouter, Depends, HTTPException

from ..dependencies import AppContainer, get_container
from ..models import JobReview, LmsExportResult, ProcessingJob, UpdateReviewRequest


router = APIRouter(prefix="/attendance/ocr/jobs", tags=["review"])


@router.get("/{job_id}/review", response_model=JobReview)
def get_review(
    job_id: str,
    container: AppContainer = Depends(get_container),
) -> JobReview:
    try:
        return container.service.get_review(job_id)
    except KeyError:
        raise HTTPException(status_code=404, detail="Job not found") from None
    except ValueError as exc:
        raise HTTPException(status_code=409, detail=str(exc)) from None


@router.patch("/{job_id}/review", response_model=JobReview)
def update_review(
    job_id: str,
    request: UpdateReviewRequest,
    container: AppContainer = Depends(get_container),
) -> JobReview:
    try:
        return container.service.update_review(job_id, request)
    except KeyError:
        raise HTTPException(status_code=404, detail="Job not found") from None
    except ValueError as exc:
        raise HTTPException(status_code=409, detail=str(exc)) from None


@router.post("/{job_id}/review/confirm", response_model=ProcessingJob)
def confirm_review(
    job_id: str,
    container: AppContainer = Depends(get_container),
) -> ProcessingJob:
    try:
        return container.service.confirm_review(job_id)
    except KeyError:
        raise HTTPException(status_code=404, detail="Job not found") from None
    except ValueError as exc:
        raise HTTPException(status_code=409, detail=str(exc)) from None


@router.post("/{job_id}/export", response_model=LmsExportResult)
def export_job(
    job_id: str,
    container: AppContainer = Depends(get_container),
) -> LmsExportResult:
    try:
        return container.service.export_job(job_id)
    except KeyError:
        raise HTTPException(status_code=404, detail="Job not found") from None
    except ValueError as exc:
        raise HTTPException(status_code=409, detail=str(exc)) from None
