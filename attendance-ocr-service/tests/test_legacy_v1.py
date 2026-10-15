from app.legacy_v1 import LegacyV1StaticOcrAdapter, convert_legacy_v1_result


LEGACY_PAYLOAD = {
    "ready": True,
    "successful": True,
    "result": {
        "ids": [0],
        "scores": [2.7],
        "meta": [
            {
                "image_number_in_group": 0,
                "matched": {
                    "clusters": [
                        [
                            [10, 20, 50, 40],
                            [55, 20, 100, 40],
                        ]
                    ],
                    "scores": [2.7],
                    "names": [["Ivan", "Smirnov"]],
                },
                "unmatched": {
                    "clusters": [
                        [
                            [12, 80, 70, 100],
                        ]
                    ]
                },
            }
        ],
    },
}


def test_convert_legacy_v1_result_maps_matched_and_unmatched_regions():
    result = convert_legacy_v1_result(LEGACY_PAYLOAD, image_ids=["image_001"])

    assert result[0].image_id == "image_001"
    assert len(result[0].regions) == 2
    assert result[0].regions[0].text == "Ivan Smirnov"
    assert result[0].regions[0].confidence == 1.0
    assert result[0].regions[0].bbox.x1 == 10
    assert result[0].regions[0].bbox.x2 == 100
    assert result[0].regions[0].raw["matched"] is True
    assert result[0].regions[1].text == ""
    assert result[0].regions[1].raw["matched"] is False


def test_legacy_v1_static_adapter_uses_requested_image_ids():
    adapter = LegacyV1StaticOcrAdapter(LEGACY_PAYLOAD)

    result = adapter.recognize(["requested_image"])

    assert result[0].image_id == "requested_image"
