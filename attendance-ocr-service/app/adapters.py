from __future__ import annotations

import asyncio
import sys
from abc import ABC, abstractmethod
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
from typing import Any, Callable

import numpy as np

from .demo_case import build_demo_ocr_results, load_demo_students
from .nasledie import convert_nasledie_result
from .models import BoundingBox, LessonContext, OcrImageResult, OcrRegion, RegionType, Student
from .ports import ImageStorage


class OcrAdapter(ABC):
    @abstractmethod
    def recognize(self, image_ids: list[str], lesson: LessonContext | None = None) -> list[OcrImageResult]:
        """Return OCR regions for the supplied image identifiers."""

    def recognize_for_students(self, image_ids: list[str], lesson: LessonContext, students: list[Student]) -> list[OcrImageResult]:
        return self.recognize(image_ids, lesson)


class LmsAdapter(ABC):
    @abstractmethod
    def get_students(self, lesson: LessonContext) -> list[Student]:
        """Return students allowed for a lesson context."""


class StubOcrAdapter(OcrAdapter):
    """Deterministic OCR adapter used until a real OCR component is available."""

    def recognize(self, image_ids: list[str], lesson: LessonContext | None = None) -> list[OcrImageResult]:
        results: list[OcrImageResult] = []
        for index, image_id in enumerate(image_ids):
            regions = [
                OcrRegion(
                    type=RegionType.DATE,
                    text="2026-05-20",
                    confidence=0.92,
                    bbox=BoundingBox(x1=20, y1=15, x2=180, y2=45),
                ),
                OcrRegion(
                    type=RegionType.SUBJECT,
                    text="Software Engineering",
                    confidence=0.89,
                    bbox=BoundingBox(x1=200, y1=15, x2=520, y2=45),
                ),
                OcrRegion(
                    type=RegionType.NAME,
                    text="Ivan Smirnov",
                    confidence=0.86,
                    bbox=BoundingBox(x1=40, y1=100 + index * 40, x2=360, y2=130 + index * 40),
                ),
                OcrRegion(
                    type=RegionType.NAME,
                    text="Maria Petrova",
                    confidence=0.76,
                    bbox=BoundingBox(x1=40, y1=140 + index * 40, x2=330, y2=170 + index * 40),
                ),
            ]
            results.append(OcrImageResult(image_id=image_id, regions=regions))
        return results

    def recognize_uploaded_image(self, image_id: str, width: int, height: int) -> OcrImageResult:
        """Return demo regions scaled to the uploaded image size."""
        sx = max(width, 1) / 800
        sy = max(height, 1) / 600

        def box(x1: float, y1: float, x2: float, y2: float) -> BoundingBox:
            return BoundingBox(x1=x1 * sx, y1=y1 * sy, x2=x2 * sx, y2=y2 * sy)

        return OcrImageResult(
            image_id=image_id,
            regions=[
                OcrRegion(type=RegionType.DATE, text="2026-05-20", confidence=0.92, bbox=box(32, 30, 210, 70)),
                OcrRegion(
                    type=RegionType.SUBJECT,
                    text="Software Engineering",
                    confidence=0.89,
                    bbox=box(230, 30, 620, 70),
                ),
                OcrRegion(
                    type=RegionType.NAME,
                    text="Ivan Smirnov",
                    confidence=0.86,
                    bbox=box(70, 140, 420, 178),
                ),
                OcrRegion(
                    type=RegionType.NAME,
                    text="Maria Petrova",
                    confidence=0.76,
                    bbox=box(70, 195, 380, 232),
                ),
            ],
        )


class DemoCaseOcrAdapter(OcrAdapter):
    """Deterministic OCR adapter backed by the packaged public demonstration case."""

    def recognize(self, image_ids: list[str], lesson: LessonContext | None = None) -> list[OcrImageResult]:
        return build_demo_ocr_results(image_ids)


class LegacyV1LivePipelineAdapter:
    """Optional adapter for running the original version 1 Pipeline in-process."""

    def __init__(
        self,
        legacy_root: Path,
        detection_model_path: Path,
        recognition_model_path: Path,
        device: str = "cpu",
    ) -> None:
        self.legacy_root = legacy_root
        self.detection_model_path = detection_model_path
        self.recognition_model_path = recognition_model_path
        self.device = device
        self._pipeline: Any | None = None

    def is_configured(self) -> bool:
        return (
            self.legacy_root.exists()
            and self.detection_model_path.exists()
            and self.recognition_model_path.exists()
        )

    def configuration_errors(self) -> list[str]:
        errors = []
        if not self.legacy_root.exists():
            errors.append(f"Legacy root not found: {self.legacy_root}")
        if not self.detection_model_path.exists():
            errors.append(f"Detection model not found: {self.detection_model_path}")
        if not self.recognition_model_path.exists():
            errors.append(f"Recognition model not found: {self.recognition_model_path}")
        return errors

    def predict(self, images: list[Any], acceptable_names: list[list[str]]) -> dict[str, Any]:
        pipeline = self._load_pipeline()
        return pipeline.predict(images, acceptable_names)

    def recognize_images(
        self,
        images: list[Any],
        image_ids: list[str],
        acceptable_names: list[list[str]],
    ) -> list[OcrImageResult]:
        from .legacy_v1 import convert_legacy_v1_result

        legacy_result = self.predict(images, acceptable_names)
        return convert_legacy_v1_result({"result": legacy_result}, image_ids=image_ids)

    def _load_pipeline(self) -> Any:
        if self._pipeline is not None:
            return self._pipeline
        if not self.is_configured():
            raise RuntimeError("; ".join(self.configuration_errors()))

        import sys

        legacy_app_path = self.legacy_root / "src" / "app"
        sys.path.insert(0, str(legacy_app_path))
        try:
            from processing import Detector, Pipeline, Recogniser

            detector = Detector(str(self.detection_model_path), device="cpu")
            recogniser = Recogniser(str(self.recognition_model_path), device=self.device)
            self._pipeline = Pipeline(detector, recogniser)
        finally:
            try:
                sys.path.remove(str(legacy_app_path))
            except ValueError:
                pass

        return self._pipeline


class StorageBackedLegacyV1OcrAdapter(OcrAdapter):
    """Run OCR version 1 against images stored by the new service."""

    def __init__(
        self,
        image_storage: ImageStorage,
        lms: LmsAdapter,
        pipeline: LegacyV1LivePipelineAdapter,
    ) -> None:
        self.image_storage = image_storage
        self.lms = lms
        self.pipeline = pipeline

    def recognize(self, image_ids: list[str], lesson: LessonContext | None = None) -> list[OcrImageResult]:
        lesson = lesson or LessonContext()
        return self.recognize_for_students(image_ids, lesson, self.lms.get_students(lesson))

    def recognize_for_students(self, image_ids: list[str], lesson: LessonContext, students: list[Student]) -> list[OcrImageResult]:
        images = [np.array(self.image_storage.load(image_id).image) for image_id in image_ids]
        acceptable_names = [student.full_name.split() for student in students]
        return self.pipeline.recognize_images(
            images=images,
            image_ids=image_ids,
            acceptable_names=acceptable_names,
        )


class NasledieLivePipelineAdapter:
    """Optional adapter for the May 2026 nasledie OCR pipeline."""

    def __init__(
        self,
        nasledie_root: Path,
        models_dir: Path,
        recognizer_model_name: str = "raxtemur/trocr-base-ru",
        device: str = "cpu",
        min_conf_score: float = 0.3,
    ) -> None:
        self.nasledie_root = nasledie_root
        self.models_dir = models_dir
        self.recognizer_model_name = recognizer_model_name
        self.device = device
        self.min_conf_score = min_conf_score
        self._pipeline: Any | None = None

    def is_configured(self) -> bool:
        return not self.configuration_errors()

    def configuration_errors(self) -> list[str]:
        errors = []
        e2e_root = self.nasledie_root / "e2e"
        if not e2e_root.exists():
            errors.append(f"Nasledie e2e root not found: {e2e_root}")
        for filename in ("rot_regr.pt", "text_det.pt", "signs_det.pt"):
            path = self.models_dir / filename
            if not path.exists():
                errors.append(f"Nasledie model not found: {path}")
        return errors

    def predict_image(
        self,
        image: Any,
        acceptable_subjects: dict[str, list[list[str]]],
        acceptable_names: dict[str, list[list[str]]],
    ) -> dict[str, Any]:
        pipeline = self._load_pipeline()
        return _run_coroutine(pipeline.predict(image, acceptable_subjects, acceptable_names))

    def predict_image_with_timings(
        self,
        image: Any,
        acceptable_subjects: dict[str, list[list[str]]],
        acceptable_names: dict[str, list[list[str]]],
    ) -> tuple[dict[str, Any], dict[str, float]]:
        import time

        timings: dict[str, float] = {}
        load_started = time.perf_counter()
        pipeline = self._load_pipeline()
        timings["model_load_ms"] = round((time.perf_counter() - load_started) * 1000, 2)

        self._stage_timings = {}
        total_started = time.perf_counter()
        try:
            result = _run_coroutine(pipeline.predict(image, acceptable_subjects, acceptable_names))
        finally:
            stage_timings = getattr(self, "_stage_timings", {})
            self._stage_timings = None

        timings.update(stage_timings)
        timings["pipeline_total_ms"] = round((time.perf_counter() - total_started) * 1000, 2)
        known_stage_ms = sum(
            timings.get(key, 0.0)
            for key in ("rotation_ms", "detection_ms", "recognition_ms")
        )
        timings["postprocess_and_matching_ms"] = round(
            max(0.0, timings["pipeline_total_ms"] - known_stage_ms),
            2,
        )
        return result, timings

    def recognize_images(
        self,
        images: list[Any],
        image_ids: list[str],
        acceptable_subjects: dict[str, list[list[str]]],
        acceptable_names: dict[str, list[list[str]]],
    ) -> list[OcrImageResult]:
        results: list[OcrImageResult] = []
        for image, image_id in zip(images, image_ids):
            raw_result = self.predict_image(image, acceptable_subjects, acceptable_names)
            results.append(convert_nasledie_result(raw_result, image_id=image_id, acceptable_names=acceptable_names))
        return results

    def _load_pipeline(self) -> Any:
        if self._pipeline is not None:
            return self._pipeline
        if not self.is_configured():
            raise RuntimeError("; ".join(self.configuration_errors()))

        e2e_root = self.nasledie_root / "e2e"
        previous_modules = {
            name: sys.modules.pop(name, None)
            for name in ("pipeline_interfaces", "utils", "pipeline", "models")
        }
        sys.path.insert(0, str(e2e_root))
        try:
            from pipeline import CombinedDetector, Pipeline
            from models import Detector, Recognizer, RotationAngleRegressor

            rot = _TimedPredictor(
                RotationAngleRegressor(str(self.models_dir / "rot_regr.pt")),
                stage_name="rotation_ms",
                recorder=self._record_stage_timing,
            )
            combined_detector = CombinedDetector(
                [
                    Detector(
                        str(self.models_dir / "text_det.pt"),
                        min_conf_score=self.min_conf_score,
                        device=self.device,
                        num_classes=2,
                    ),
                    Detector(
                        str(self.models_dir / "signs_det.pt"),
                        min_conf_score=self.min_conf_score,
                        device=self.device,
                        num_classes=3,
                    ),
                ],
                label_shifts=[0, 10],
            )
            detector = _TimedPredictor(
                combined_detector,
                stage_name="detection_ms",
                recorder=self._record_stage_timing,
            )
            recognizer = _TimedPredictor(
                Recognizer(self.recognizer_model_name, self.device),
                stage_name="recognition_ms",
                recorder=self._record_stage_timing,
            )
            self._pipeline = Pipeline(
                rot,
                detector,
                recognizer,
                text_label_idx=1,
                idx2sign={11: "+", 12: "-"},
            )
        finally:
            try:
                sys.path.remove(str(e2e_root))
            except ValueError:
                pass
            for name in ("pipeline_interfaces", "utils", "pipeline", "models"):
                sys.modules.pop(name, None)
                previous = previous_modules.get(name)
                if previous is not None:
                    sys.modules[name] = previous

        return self._pipeline

    def _record_stage_timing(self, stage_name: str, duration_ms: float) -> None:
        stage_timings = getattr(self, "_stage_timings", None)
        if stage_timings is None:
            return
        stage_timings[stage_name] = round(stage_timings.get(stage_name, 0.0) + duration_ms, 2)


class _TimedPredictor:
    def __init__(self, wrapped: Any, stage_name: str, recorder: Callable[[str, float], None]) -> None:
        self._wrapped = wrapped
        self._stage_name = stage_name
        self._recorder = recorder

    def predict(self, *args: Any, **kwargs: Any) -> Any:
        import time

        started = time.perf_counter()
        try:
            return self._wrapped.predict(*args, **kwargs)
        finally:
            self._recorder(self._stage_name, round((time.perf_counter() - started) * 1000, 2))


class StorageBackedNasledieOcrAdapter(OcrAdapter):
    """Run the nasledie OCR pipeline against images stored by the new service."""

    def __init__(
        self,
        image_storage: ImageStorage,
        lms: LmsAdapter,
        pipeline: NasledieLivePipelineAdapter,
    ) -> None:
        self.image_storage = image_storage
        self.lms = lms
        self.pipeline = pipeline

    def recognize(self, image_ids: list[str], lesson: LessonContext | None = None) -> list[OcrImageResult]:
        lesson = lesson or LessonContext()
        return self.recognize_for_students(image_ids, lesson, self.lms.get_students(lesson))

    def recognize_for_students(self, image_ids: list[str], lesson: LessonContext, students: list[Student]) -> list[OcrImageResult]:
        images = [np.array(self.image_storage.load(image_id).image) for image_id in image_ids]
        acceptable_subjects = _lesson_subjects(lesson)
        acceptable_names = _students_to_acceptable_names(students, default_group=lesson.group)
        return self.pipeline.recognize_images(
            images=images,
            image_ids=image_ids,
            acceptable_subjects=acceptable_subjects,
            acceptable_names=acceptable_names,
        )


class StubLmsAdapter(LmsAdapter):
    def get_students(self, lesson: LessonContext) -> list[Student]:
        group = lesson.group or "PI-101"
        return [
            Student(student_id="student_001", full_name="Ivan Smirnov", group=group),
            Student(student_id="student_002", full_name="Maria Petrova", group=group),
            Student(student_id="student_003", full_name="Sergey Orlov", group=group),
        ]


class DemoCaseLmsAdapter(LmsAdapter):
    """LMS adapter for the packaged public sample roster."""

    def get_students(self, lesson: LessonContext) -> list[Student]:
        return load_demo_students()


def _students_to_acceptable_names(
    students: list[Student],
    default_group: str | None = None,
) -> dict[str, list[list[str]]]:
    groups: dict[str, list[list[str]]] = {}
    for student in students:
        group = student.group or default_group or "undefined"
        groups.setdefault(group, []).append(student.full_name.split())
    return groups


def _lesson_subjects(lesson: LessonContext) -> dict[str, list[list[str]]]:
    subject = lesson.subject or "undefined"
    return {subject: [[part for part in subject.split() if part]]}


def _run_coroutine(coroutine):
    try:
        asyncio.get_running_loop()
    except RuntimeError:
        return asyncio.run(coroutine)

    with ThreadPoolExecutor(max_workers=1) as executor:
        future = executor.submit(asyncio.run, coroutine)
        return future.result()
