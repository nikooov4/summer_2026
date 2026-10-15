from __future__ import annotations

from fastapi import APIRouter, Depends, HTTPException, Query

from ..dependencies import AppContainer, get_container
from ..models import CreateJobRequest, ProcessingJob


router = APIRouter(prefix="/attendance/ocr/jobs", tags=["jobs"])


@router.post("", response_model=ProcessingJob, status_code=202)
def create_job(
    request: CreateJobRequest,
    container: AppContainer = Depends(get_container),
) -> ProcessingJob:
    ids = [student.student_id for student in request.students or []]
    if len(ids) != len(set(ids)):
        raise HTTPException(status_code=422, detail="Duplicate student IDs in roster")
    return container.service.create_job(request.image_ids, request.lesson, request.students)


@router.post("/process-next", response_model=ProcessingJob)
def process_next_job(
    mode: str = Query(default="stub"),
    container: AppContainer = Depends(get_container),
) -> ProcessingJob:
    job_id = container.job_queue.dequeue()
    if job_id is None:
        raise HTTPException(status_code=409, detail="No queued jobs")
    try:
        return _processor_for_mode(mode, container).process_job(job_id)
    except KeyError:
        raise HTTPException(status_code=404, detail="Job not found") from None
    except RuntimeError as exc:
        raise HTTPException(status_code=409, detail=str(exc)) from None


@router.get("/{job_id}", response_model=ProcessingJob)
def get_job(
    job_id: str,
    container: AppContainer = Depends(get_container),
) -> ProcessingJob:
    job = container.service.get_job(job_id)
    if job is None:
        raise HTTPException(status_code=404, detail="Job not found")
    return job


@router.get("/{job_id}/result")
def get_job_result(
    job_id: str,
    container: AppContainer = Depends(get_container),
):
    job = container.service.get_job(job_id)
    if job is None:
        raise HTTPException(status_code=404, detail="Job not found")
    if job.result is None:
        raise HTTPException(status_code=409, detail="Job result is not ready")
    return job.result


@router.post("/{job_id}/process", response_model=ProcessingJob)
def process_job(
    job_id: str,
    mode: str = Query(default="stub"),
    container: AppContainer = Depends(get_container),
) -> ProcessingJob:
    try:
        if hasattr(container.job_queue, "remove"):
            container.job_queue.remove(job_id)
        return _processor_for_mode(mode, container).process_job(job_id)
    except KeyError:
        raise HTTPException(status_code=404, detail="Job not found") from None
    except RuntimeError as exc:
        raise HTTPException(status_code=409, detail=str(exc)) from None


@router.post("/{job_id}/confirm", response_model=ProcessingJob)
def confirm_job(
    job_id: str,
    container: AppContainer = Depends(get_container),
) -> ProcessingJob:
    try:
        return container.service.confirm_job(job_id)
    except KeyError:
        raise HTTPException(status_code=404, detail="Job not found") from None
    except ValueError as exc:
        raise HTTPException(status_code=409, detail=str(exc)) from None


def _processor_for_mode(mode: str, container: AppContainer):
    if mode == "demo":
        return container.demo_job_processor
    if mode == "nasledie":
        if not container.nasledie_pipeline.is_configured():
            raise HTTPException(
                status_code=409,
                detail={
                    "message": "Nasledie OCR pipeline is not configured",
                    "errors": container.nasledie_pipeline.configuration_errors(),
                },
            )
        return container.nasledie_job_processor
    if mode == "legacy_v1":
        if not container.legacy_v1_pipeline.is_configured():
            raise HTTPException(
                status_code=409,
                detail={
                    "message": "Legacy OCR version 1 pipeline is not configured",
                    "errors": container.legacy_v1_pipeline.configuration_errors(),
                },
            )
        return container.legacy_job_processor
    if mode == "stub":
        return container.job_processor
    raise HTTPException(status_code=400, detail=f"Unknown OCR mode: {mode}")
