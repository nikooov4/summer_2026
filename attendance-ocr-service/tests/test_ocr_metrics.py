from scripts.evaluate_ocr_results import evaluate, normalize_name


def test_normalize_name_collapses_spaces_case_and_yo():
    assert normalize_name("  ЁЖОВ,   СЛОВО!  ") == "ежов слово"


def test_evaluate_legacy_batch_results_counts_name_precision_recall():
    predictions = {
        "results": [
            {
                "image_id": "sheet_001",
                "regions": [
                    {"type": "name", "text": "Maria Petrova"},
                    {"type": "name", "text": "Сидоров Сидор Сидорович"},
                    {"type": "date", "text": "2026-05-20"},
                ],
            }
        ]
    }
    ground_truth = {
        "dataset_id": "unit",
        "samples": [
            {
                "image_id": "sheet_001",
                "expected_present": [
                    {"full_name": "Maria Petrova"},
                    {"full_name": "Sergey Orlov"},
                ],
            }
        ],
    }

    report = evaluate(predictions, ground_truth)

    assert report["totals"] == {
        "tp": 1,
        "fp": 1,
        "fn": 1,
        "candidate_count": 2,
        "needs_review_count": 0,
    }
    assert report["metrics"]["precision"] == 0.5
    assert report["metrics"]["recall"] == 0.5
    assert report["metrics"]["f1"] == 0.5


def test_evaluate_students_result_tracks_needs_review_rate():
    predictions = {
        "samples": [
            {
                "image_id": "sheet_001",
                "students": [
                    {"full_name": "Maria Petrova", "status": "present"},
                    {"full_name": "Sergey Orlov", "status": "needs_review"},
                ],
            }
        ]
    }
    ground_truth = {
        "dataset_id": "unit",
        "samples": [
            {
                "image_id": "sheet_001",
                "expected_present": [
                    {"full_name": "Maria Petrova"},
                    {"full_name": "Sergey Orlov"},
                ],
            }
        ],
    }

    report = evaluate(predictions, ground_truth)

    assert report["totals"]["tp"] == 1
    assert report["totals"]["fn"] == 1
    assert report["totals"]["needs_review_count"] == 1
    assert report["metrics"]["needs_review_rate"] == 0.5


def test_evaluate_service_result_uses_single_ocr_image_id():
    predictions = {
        "job_id": "job_001",
        "ocr": [{"image_id": "sheet_001", "regions": []}],
        "students": [
            {"full_name": "Maria Petrova", "status": "present"},
        ],
    }
    ground_truth = {
        "dataset_id": "unit",
        "samples": [
            {
                "image_id": "sheet_001",
                "expected_present": [
                    {"full_name": "Maria Petrova"},
                ],
            }
        ],
    }

    report = evaluate(predictions, ground_truth)

    assert report["totals"]["tp"] == 1
    assert report["totals"]["fn"] == 0
