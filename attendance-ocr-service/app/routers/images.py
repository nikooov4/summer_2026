from __future__ import annotations

from fastapi import APIRouter, Depends, HTTPException, UploadFile

from ..dependencies import AppContainer, get_container
from ..models import StoredImage
from ..storage import UploadTooLargeError, UploadValidationError


router = APIRouter(prefix="/attendance/ocr/images", tags=["images"])


@router.post("", response_model=StoredImage, status_code=201)
async def upload_image(
    file: UploadFile,
    container: AppContainer = Depends(get_container),
) -> StoredImage:
    content = await file.read()
    try:
        return container.image_storage.save_upload(
            filename=file.filename or "upload.png",
            content_type=file.content_type or "application/octet-stream",
            content=content,
        )
    except UploadTooLargeError as exc:
        raise HTTPException(status_code=413, detail=str(exc)) from None
    except UploadValidationError as exc:
        raise HTTPException(status_code=400, detail=str(exc)) from None
    except Exception as exc:
        raise HTTPException(status_code=400, detail=f"Invalid image: {exc}") from None
