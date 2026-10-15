from __future__ import annotations

from itertools import zip_longest
from typing import Any

from .adapters import OcrAdapter
from .models import BoundingBox, OcrImageResult, OcrRegion, RegionType


def convert_legacy_v1_result(payload: dict[str, Any], image_ids: list[str] | None = None) -> list[OcrImageResult]:
    """Convert OCR version 1 task payload/result into the service OCR contract."""
    result = payload.get("result", payload)
    meta_items = result.get("meta", [])
    converted: list[OcrImageResult] = []

    for fallback_index, image_meta in enumerate(meta_items):
        image_index = int(image_meta.get("image_number_in_group", fallback_index))
        image_id = _image_id_for_index(image_index, image_ids)
        regions: list[OcrRegion] = []

        matched = image_meta.get("matched", {})
        matched_clusters = matched.get("clusters", [])
        matched_scores = matched.get("scores", [])
        matched_names = matched.get("names", [])
        for cluster, score, name in zip_longest(matched_clusters, matched_scores, matched_names, fillvalue=None):
            regions.append(
                OcrRegion(
                    type=RegionType.NAME,
                    text=_name_to_text(name),
                    confidence=_legacy_score_to_confidence(score, name),
                    bbox=_cluster_bbox(cluster),
                    raw={
                        "source": "ocr_version_1",
                        "matched": True,
                        "legacy_score": score,
                        "legacy_name": name,
                        "legacy_cluster": cluster,
                    },
                )
            )

        unmatched = image_meta.get("unmatched", {})
        for cluster in unmatched.get("clusters", []):
            regions.append(
                OcrRegion(
                    type=RegionType.NAME,
                    text="",
                    confidence=0.0,
                    bbox=_cluster_bbox(cluster),
                    raw={
                        "source": "ocr_version_1",
                        "matched": False,
                        "legacy_cluster": cluster,
                    },
                )
            )

        converted.append(
            OcrImageResult(
                image_id=image_id,
                regions=regions,
                raw={
                    "source": "ocr_version_1",
                    "legacy_image_meta": image_meta,
                },
            )
        )

    return converted


class LegacyV1StaticOcrAdapter(OcrAdapter):
    """OCR adapter backed by an already produced version 1 result payload."""

    def __init__(self, payload: dict[str, Any]) -> None:
        self.payload = payload

    def recognize(self, image_ids: list[str]) -> list[OcrImageResult]:
        return convert_legacy_v1_result(self.payload, image_ids=image_ids)


def _image_id_for_index(image_index: int, image_ids: list[str] | None) -> str:
    if image_ids and 0 <= image_index < len(image_ids):
        return image_ids[image_index]
    return f"legacy_v1_image_{image_index}"


def _name_to_text(name: Any) -> str:
    if name is None:
        return ""
    if isinstance(name, str):
        return name
    if isinstance(name, (list, tuple)):
        return " ".join(str(part) for part in name if str(part).strip())
    return str(name)


def _legacy_score_to_confidence(score: Any, name: Any) -> float:
    try:
        score_value = float(score)
    except (TypeError, ValueError):
        return 0.0

    parts_count = 1
    if isinstance(name, (list, tuple)):
        parts_count = max(1, len([part for part in name if str(part).strip()]))

    return max(0.0, min(1.0, score_value / parts_count))


def _cluster_bbox(cluster: Any) -> BoundingBox | None:
    boxes = [_box_to_float_tuple(box) for box in _iter_boxes(cluster)]
    boxes = [box for box in boxes if box is not None]
    if not boxes:
        return None

    return BoundingBox(
        x1=min(box[0] for box in boxes),
        y1=min(box[1] for box in boxes),
        x2=max(box[2] for box in boxes),
        y2=max(box[3] for box in boxes),
    )


def _iter_boxes(cluster: Any):
    if cluster is None:
        return
    if _is_box(cluster):
        yield cluster
        return
    for item in cluster:
        if _is_box(item):
            yield item


def _is_box(value: Any) -> bool:
    return isinstance(value, (list, tuple)) and len(value) == 4 and all(_is_number(item) for item in value)


def _is_number(value: Any) -> bool:
    return isinstance(value, (int, float)) and not isinstance(value, bool)


def _box_to_float_tuple(box: Any) -> tuple[float, float, float, float] | None:
    if not _is_box(box):
        return None
    return float(box[0]), float(box[1]), float(box[2]), float(box[3])

