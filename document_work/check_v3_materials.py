from pathlib import Path

import pymupdf
from docx import Document
from pptx import Presentation


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "Готовые_материалы" / "Летняя_практика" / "v3"


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


def require(text: str, phrases: list[str], label: str) -> None:
    missing = [phrase for phrase in phrases if phrase.lower() not in text.lower()]
    if missing:
        raise AssertionError(f"{label}: отсутствуют обязательные фразы: {missing}")


def forbid(text: str, phrases: list[str], label: str) -> None:
    found = [phrase for phrase in phrases if phrase.lower() in text.lower()]
    if found:
        raise AssertionError(f"{label}: найдены запрещенные фразы: {found}")


def main() -> None:
    ef_report = OUTPUT / "Ефименко" / "Отчет_по_летней_практике_ЕфименкоНП_v3.docx"
    ef_presentation = OUTPUT / "Ефименко" / "Презентация_ЕфименкоНП_v3.pptx"
    lok_report = OUTPUT / "Локтев" / "Отчет_по_летней_практике_ЛоктевМС_v3.docx"
    lok_presentation = OUTPUT / "Локтев" / "Презентация_ЛоктевМС_v3.pptx"

    ef_text = docx_text(ef_report) + "\n" + pptx_text(ef_presentation)
    lok_text = docx_text(lok_report) + "\n" + pptx_text(lok_presentation)

    require(ef_text, [
        "Ефименко",
        "Журнал преподавателя",
        "Присутствовал",
        "Опоздал",
        "Уважительная причина",
        "Личный режим студента",
        "Тестовая ролевая модель",
        "25 страниц",
    ], "Материалы Ефименко")
    forbid(ef_text, ["Локтев"], "Материалы Ефименко")

    require(lok_text, [
        "Локтев",
        "Мониторинг",
        "Студенты, требующие внимания",
        "Административная панель",
        "Курсы и зачисления",
        "Импорт и экспорт JSON/CSV",
        "25 страниц",
    ], "Материалы Локтева")
    forbid(lok_text, ["Ефименко", "ролевая модель", "тестовая ролевая"], "Материалы Локтева")

    for path in [ef_report, lok_report]:
        document = Document(path)
        if len(document.inline_shapes) != 8:
            raise AssertionError(f"{path.name}: ожидалось 8 рисунков, найдено {len(document.inline_shapes)}")

    expected = {
        OUTPUT / "Ефименко" / "Отчет_по_летней_практике_ЕфименкоНП_v3.pdf": 25,
        OUTPUT / "Локтев" / "Отчет_по_летней_практике_ЛоктевМС_v3.pdf": 25,
        OUTPUT / "Ефименко" / "Презентация_ЕфименкоНП_v3.pdf": 15,
        OUTPUT / "Локтев" / "Презентация_ЛоктевМС_v3.pdf": 15,
    }
    for path, page_count in expected.items():
        actual = len(pymupdf.open(path))
        if actual != page_count:
            raise AssertionError(f"{path.name}: ожидалось {page_count}, найдено {actual}")

    if len(Presentation(ef_presentation).slides) != 15:
        raise AssertionError("Презентация Ефименко должна содержать 15 слайдов")
    if len(Presentation(lok_presentation).slides) != 15:
        raise AssertionError("Презентация Локтева должна содержать 15 слайдов")

    print("v3 check: OK")
    print("Ефименко: 25 страниц, 8 рисунков, 15 слайдов")
    print("Локтев: 25 страниц, 8 рисунков, 15 слайдов")
    print("Разделение авторства и запрещенные упоминания проверены")


if __name__ == "__main__":
    main()
