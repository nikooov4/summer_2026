from __future__ import annotations

import numpy as np
from fastapi import APIRouter, Depends, HTTPException, Query, UploadFile
from fastapi.responses import HTMLResponse

from ..dependencies import AppContainer, get_container
from ..matching import match_name_regions
from ..models import LessonContext


router = APIRouter(tags=["demo"])


@router.get("/", response_class=HTMLResponse)
def index(container: AppContainer = Depends(get_container)) -> str:
    return (container.settings.static_dir / "index.html").read_text(encoding="utf-8")


@router.get("/demo/pipeline-status")
def pipeline_status(container: AppContainer = Depends(get_container)) -> dict[str, object]:
    legacy_v1_pipeline = container.legacy_v1_pipeline
    nasledie_pipeline = container.nasledie_pipeline
    return {
        "stub_demo_ready": True,
        "legacy_v1_configured": legacy_v1_pipeline.is_configured(),
        "legacy_v1_errors": legacy_v1_pipeline.configuration_errors(),
        "nasledie_configured": nasledie_pipeline.is_configured(),
        "nasledie_errors": nasledie_pipeline.configuration_errors(),
    }


@router.post("/demo/process-image")
async def process_demo_image(
    file: UploadFile,
    mode: str = Query(default="stub"),
    container: AppContainer = Depends(get_container),
):
    contents = await file.read()
    try:
        stored = container.image_storage.save_upload(
            filename=file.filename or "upload.png",
            content_type=file.content_type or "application/octet-stream",
            content=contents,
        )
        loaded = container.image_storage.load(stored.image_id)
        image = loaded.image
        width, height = stored.width, stored.height
    except Exception as exc:
        raise HTTPException(status_code=400, detail=f"Invalid image: {exc}") from None

    image_id = stored.image_id
    students = container.lms.get_students(lesson=LessonContext(group=container.settings.demo_group))
    acceptable_names = [student.full_name.split() for student in students]

    if mode == "legacy_v1":
        legacy_v1_pipeline = container.legacy_v1_pipeline
        if not legacy_v1_pipeline.is_configured():
            raise HTTPException(
                status_code=409,
                detail={
                    "message": "Legacy OCR version 1 pipeline is not configured",
                    "errors": legacy_v1_pipeline.configuration_errors(),
                },
            )
        ocr_results = legacy_v1_pipeline.recognize_images(
            images=[np.array(image)],
            image_ids=[image_id],
            acceptable_names=acceptable_names,
        )
        actual_mode = "legacy_v1_live"
        note = "Изображение обработано старым Pipeline из OCR version_1 и переведено в новый контракт."
    elif mode == "nasledie":
        nasledie_pipeline = container.nasledie_pipeline
        if not nasledie_pipeline.is_configured():
            raise HTTPException(
                status_code=409,
                detail={
                    "message": "Nasledie OCR pipeline is not configured",
                    "errors": nasledie_pipeline.configuration_errors(),
                },
            )
        acceptable_names_dict = {
            container.settings.demo_group: [student.full_name.split() for student in students]
        }
        subject = "Software Engineering"
        ocr_results = nasledie_pipeline.recognize_images(
            images=[np.array(image)],
            image_ids=[image_id],
            acceptable_subjects={subject: [subject.split()]},
            acceptable_names=acceptable_names_dict,
        )
        actual_mode = "nasledie_live"
        note = "Изображение обработано новым Pipeline из nasledie и переведено в новый контракт."
    else:
        ocr_results = [container.ocr.recognize_uploaded_image(image_id, width=width, height=height)]
        actual_mode = "stub_demo"
        note = "Демо использует заменяемый OCR-адаптер. Реальные OCR-режимы доступны через mode=legacy_v1 или mode=nasledie после добавления весов моделей."

    regions = [region for result in ocr_results for region in result.regions]
    recognized = match_name_regions(regions, students)

    return {
        "mode": actual_mode,
        "image": stored,
        "ocr": ocr_results[0],
        "students": recognized,
        "note": note,
    }
