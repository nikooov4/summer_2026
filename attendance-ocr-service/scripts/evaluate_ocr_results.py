from __future__ import annotations

import argparse
import json
import re
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any


NAME_REGION_TYPE = "name"
PRESENT_STATUS = "present"
NEEDS_REVIEW_STATUS = "needs_review"
LATENCY_KEYS = ("latency_ms", "duration_ms", "processing_ms")


@dataclass
class SamplePrediction:
    image_id: str
    present_names: set[str] = field(default_factory=set)
    review_count: int = 0
    candidate_count: int = 0
    latency_ms: float | None = None


@dataclass
class SampleGroundTruth:
    image_id: str
    present_names: set[str]


def normalize_name(value: str | None) -> str:
    if not value:
        return ""
    value = value.replace("Ё", "Е").replace("ё", "е").lower()
    value = re.sub(r"[^а-яa-zё ]+", " ", value)
    return re.sub(r"\s+", " ", value).strip()


def evaluate(predictions_payload: dict[str, Any], ground_truth_payload: dict[str, Any]) -> dict[str, Any]:
    predictions = parse_predictions(predictions_payload)
    ground_truth = parse_ground_truth(ground_truth_payload)

    total_tp = 0
    total_fp = 0
    total_fn = 0
    total_review = 0
    total_candidates = 0
    latencies: list[float] = []
    samples: list[dict[str, Any]] = []

    for image_id, gt_sample in ground_truth.items():
        prediction = predictions.get(image_id, SamplePrediction(image_id=image_id))
        tp_names = prediction.present_names & gt_sample.present_names
        fp_names = prediction.present_names - gt_sample.present_names
        fn_names = gt_sample.present_names - prediction.present_names

        total_tp += len(tp_names)
        total_fp += len(fp_names)
        total_fn += len(fn_names)
        total_review += prediction.review_count
        total_candidates += prediction.candidate_count
        if prediction.latency_ms is not None:
            latencies.append(prediction.latency_ms)

        samples.append(
            {
                "image_id": image_id,
                "tp": len(tp_names),
                "fp": len(fp_names),
                "fn": len(fn_names),
                "precision": safe_divide(len(tp_names), len(tp_names) + len(fp_names)),
                "recall": safe_divide(len(tp_names), len(tp_names) + len(fn_names)),
                "f1": f1_score(len(tp_names), len(fp_names), len(fn_names)),
                "needs_review_rate": safe_divide(prediction.review_count, prediction.candidate_count),
                "false_positive_names": sorted(fp_names),
                "missed_names": sorted(fn_names),
            }
        )

    precision = safe_divide(total_tp, total_tp + total_fp)
    recall = safe_divide(total_tp, total_tp + total_fn)

    return {
        "dataset_id": ground_truth_payload.get("dataset_id"),
        "sample_count": len(ground_truth),
        "totals": {
            "tp": total_tp,
            "fp": total_fp,
            "fn": total_fn,
            "candidate_count": total_candidates,
            "needs_review_count": total_review,
        },
        "metrics": {
            "precision": precision,
            "recall": recall,
            "f1": f1_from_precision_recall(precision, recall),
            "needs_review_rate": safe_divide(total_review, total_candidates),
            "latency_ms_avg": safe_divide(sum(latencies), len(latencies)) if latencies else None,
        },
        "samples": samples,
    }


def parse_ground_truth(payload: dict[str, Any]) -> dict[str, SampleGroundTruth]:
    samples = payload.get("samples")
    if not isinstance(samples, list):
        raise ValueError("Ground truth must contain a samples list.")

    parsed: dict[str, SampleGroundTruth] = {}
    for sample in samples:
        image_id = require_string(sample, "image_id")
        present_names = {
            normalized
            for item in sample.get("expected_present", [])
            if (normalized := normalize_name(item.get("full_name")))
        }
        parsed[image_id] = SampleGroundTruth(image_id=image_id, present_names=present_names)
    return parsed


def parse_predictions(payload: dict[str, Any]) -> dict[str, SamplePrediction]:
    if isinstance(payload.get("results"), list):
        return parse_ocr_image_results(payload["results"])
    if isinstance(payload.get("samples"), list):
        return parse_prediction_samples(payload["samples"])
    if isinstance(payload.get("students"), list):
        image_id = payload.get("image_id") or single_ocr_image_id(payload) or payload.get("job_id") or "single_result"
        return {str(image_id): parse_students_result(str(image_id), payload)}
    if isinstance(payload.get("regions"), list):
        image_id = require_string(payload, "image_id")
        return {image_id: parse_ocr_image_result(payload)}
    raise ValueError("Unsupported predictions JSON. Expected results, samples, students or regions.")


def parse_prediction_samples(samples: list[dict[str, Any]]) -> dict[str, SamplePrediction]:
    parsed: dict[str, SamplePrediction] = {}
    for sample in samples:
        image_id = require_string(sample, "image_id")
        if isinstance(sample.get("students"), list):
            parsed[image_id] = parse_students_result(image_id, sample)
        elif isinstance(sample.get("regions"), list):
            parsed[image_id] = parse_ocr_image_result(sample)
        else:
            names = {normalize_name(name) for name in sample.get("present_names", [])}
            names.discard("")
            parsed[image_id] = SamplePrediction(
                image_id=image_id,
                present_names=names,
                review_count=int(sample.get("needs_review_count", 0) or 0),
                candidate_count=int(sample.get("candidate_count", len(names)) or 0),
                latency_ms=get_latency(sample),
            )
    return parsed


def parse_ocr_image_results(results: list[dict[str, Any]]) -> dict[str, SamplePrediction]:
    return {require_string(result, "image_id"): parse_ocr_image_result(result) for result in results}


def parse_ocr_image_result(result: dict[str, Any]) -> SamplePrediction:
    image_id = require_string(result, "image_id")
    present_names: set[str] = set()
    review_count = 0
    candidate_count = 0

    for region in result.get("regions", []):
        if str(region.get("type", "")).lower() != NAME_REGION_TYPE:
            continue
        name = normalize_name(region.get("text"))
        if not name:
            continue
        status = str(region.get("status") or region.get("raw", {}).get("status") or PRESENT_STATUS).lower()
        candidate_count += 1
        if status == NEEDS_REVIEW_STATUS:
            review_count += 1
        else:
            present_names.add(name)

    return SamplePrediction(
        image_id=image_id,
        present_names=present_names,
        review_count=review_count,
        candidate_count=candidate_count,
        latency_ms=get_latency(result),
    )


def parse_students_result(image_id: str, result: dict[str, Any]) -> SamplePrediction:
    present_names: set[str] = set()
    review_count = 0
    candidate_count = 0

    for student in result.get("students", []):
        status = str(student.get("status") or "").lower()
        name = normalize_name(student.get("full_name") or student.get("source_text"))
        if not name:
            continue
        candidate_count += 1
        if status == PRESENT_STATUS:
            present_names.add(name)
        elif status == NEEDS_REVIEW_STATUS:
            review_count += 1

    return SamplePrediction(
        image_id=image_id,
        present_names=present_names,
        review_count=review_count,
        candidate_count=candidate_count,
        latency_ms=get_latency(result),
    )


def get_latency(payload: dict[str, Any]) -> float | None:
    for key in LATENCY_KEYS:
        value = payload.get(key) or payload.get("raw", {}).get(key)
        if isinstance(value, int | float):
            return float(value)
    return None


def single_ocr_image_id(payload: dict[str, Any]) -> str | None:
    ocr_results = payload.get("ocr")
    if not isinstance(ocr_results, list) or len(ocr_results) != 1:
        return None
    image_id = ocr_results[0].get("image_id")
    return image_id if isinstance(image_id, str) and image_id else None


def require_string(payload: dict[str, Any], key: str) -> str:
    value = payload.get(key)
    if not isinstance(value, str) or not value:
        raise ValueError(f"Expected non-empty string field: {key}")
    return value


def safe_divide(numerator: float, denominator: float) -> float:
    if denominator == 0:
        return 0.0
    return numerator / denominator


def f1_score(tp: int, fp: int, fn: int) -> float:
    return f1_from_precision_recall(safe_divide(tp, tp + fp), safe_divide(tp, tp + fn))


def f1_from_precision_recall(precision: float, recall: float) -> float:
    if precision + recall == 0:
        return 0.0
    return 2 * precision * recall / (precision + recall)


def main() -> None:
    args = parse_args()
    predictions = json.loads(Path(args.predictions).read_text(encoding="utf-8"))
    ground_truth = json.loads(Path(args.ground_truth).read_text(encoding="utf-8"))
    report = evaluate(predictions, ground_truth)
    encoded = json.dumps(report, ensure_ascii=False, indent=2)
    if args.output:
        output_path = Path(args.output)
        output_path.parent.mkdir(parents=True, exist_ok=True)
        output_path.write_text(encoded + "\n", encoding="utf-8")
        print(output_path)
    else:
        print(encoded)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Evaluate attendance OCR output against smoke ground truth.")
    parser.add_argument("--predictions", required=True, help="Path to OCR prediction JSON.")
    parser.add_argument("--ground-truth", required=True, help="Path to smoke ground truth JSON.")
    parser.add_argument("--output", help="Optional path for metrics report JSON.")
    return parser.parse_args()


if __name__ == "__main__":
    main()
