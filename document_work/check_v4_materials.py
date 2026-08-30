import re
from pathlib import Path

import pymupdf
from docx import Document
from pptx import Presentation


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "Готовые_материалы" / "Летняя_практика" / "v4"


def docx_text(path: Path) -> str:
    document = Document(path)
    paragraphs = [paragraph.text for paragraph in document.paragraphs]
    cells = [cell.text for table in document.tables for row in table.rows for cell in row.cells]
    return "\n".join(paragraphs + cells)


def pptx_text(path: Path) -> str:
    presentation = Presentation(path)
    return "\n".join(
        shape.text
        for slide in presentation.slides
        for shape in slide.shapes
        if hasattr(shape, "text")
    )


def citation_numbers(body: str) -> set[int]:
    numbers: set[int] = set()
    for value in re.findall(r"\[(\d+(?:\s*,\s*\d+)*)\]", body):
        numbers.update(int(part.strip()) for part in value.split(","))
    return numbers


def check_report(path: Path, owner: str, forbidden: list[str], required: list[str]) -> None:
    document = Document(path)
    text = docx_text(path)
    lower = text.lower()
    for phrase in required:
        if phrase.lower() not in lower:
            raise AssertionError(f"{path.name}: отсутствует обязательная фраза {phrase!r}")
    for phrase in forbidden:
        if phrase.lower() in lower:
            raise AssertionError(f"{path.name}: найдено запрещенное упоминание {phrase!r}")

    body = text.rsplit("Список литературы", 1)[0]
    actual_citations = citation_numbers(body)
    expected_citations = set(range(1, 16))
    if actual_citations != expected_citations:
        raise AssertionError(
            f"{path.name}: внутритекстовые ссылки {sorted(actual_citations)}, ожидались 1-15"
        )
    if len(document.inline_shapes) != 8:
        raise AssertionError(f"{path.name}: ожидалось 8 рисунков, найдено {len(document.inline_shapes)}")
    if "Отметка" not in text or "Дата, подпись рук." not in text:
        raise AssertionError(f"{path.name}: задание не соответствует шестиколоночному образцу")
    if "Количество использованных источников - 15" not in text:
        raise AssertionError(f"{path.name}: неверное число источников в реферате")
    print(f"{owner}: ссылки [1]-[15], расширенное задание и 8 рисунков проверены")


def main() -> None:
    ef_report = OUTPUT / "Ефименко" / "Отчет_по_летней_практике_ЕфименкоНП_v4.docx"
    lok_report = OUTPUT / "Локтев" / "Отчет_по_летней_практике_ЛоктевМС_v4.docx"
    ef_presentation = OUTPUT / "Ефименко" / "Презентация_ЕфименкоНП_v4.pptx"
    lok_presentation = OUTPUT / "Локтев" / "Презентация_ЛоктевМС_v4.pptx"

    check_report(
        ef_report,
        "Ефименко",
        ["Локтев"],
        ["Ефименко", "тестовая ролевая модель", "ЗАДАНИЕ НА ЛЕТНЮЮ ПРАКТИКУ"],
    )
    check_report(
        lok_report,
        "Локтев",
        ["Ефименко", "ролевая модель", "тестовая ролевая"],
        ["Локтев", "студенты, требующие внимания", "импорт и экспорт JSON/CSV"],
    )

    ef_ppt_text = pptx_text(ef_presentation)
    lok_ppt_text = pptx_text(lok_presentation)
    if "Основные источники" not in ef_ppt_text or "Источники:" not in ef_ppt_text:
        raise AssertionError("В презентации Ефименко отсутствуют ссылки или слайд источников")
    if "Основные источники" not in lok_ppt_text or "Источники:" not in lok_ppt_text:
        raise AssertionError("В презентации Локтева отсутствуют ссылки или слайд источников")
    if "Локтев" in ef_ppt_text:
        raise AssertionError("В презентации Ефименко упомянут Локтев")
    if "Ефименко" in lok_ppt_text or "ролевая модель" in lok_ppt_text.lower():
        raise AssertionError("В презентации Локтева найдено запрещенное упоминание")
    if len(Presentation(ef_presentation).slides) != 16 or len(Presentation(lok_presentation).slides) != 16:
        raise AssertionError("Каждая презентация должна содержать 16 слайдов")

    pdfs = {
        OUTPUT / "Ефименко" / "Отчет_по_летней_практике_ЕфименкоНП_v4.pdf": 25,
        OUTPUT / "Локтев" / "Отчет_по_летней_практике_ЛоктевМС_v4.pdf": 25,
        OUTPUT / "Ефименко" / "Презентация_ЕфименкоНП_v4.pdf": 16,
        OUTPUT / "Локтев" / "Презентация_ЛоктевМС_v4.pdf": 16,
    }
    for path, expected_pages in pdfs.items():
        actual_pages = len(pymupdf.open(path))
        if actual_pages != expected_pages:
            raise AssertionError(f"{path.name}: {actual_pages} страниц вместо {expected_pages}")

    print("v4 check: OK")
    print("Отчеты: по 25 страниц; презентации: по 16 слайдов")
    print("Разделение авторства и внутритекстовые ссылки проверены")


if __name__ == "__main__":
    main()
