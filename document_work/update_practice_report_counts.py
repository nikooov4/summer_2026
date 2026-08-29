from __future__ import annotations

import re
from pathlib import Path

import pymupdf
from docx import Document
from docx.shared import Pt


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "Готовые_материалы"


for docx_path in sorted(OUTPUT.glob("Отчет_по_летней_практике_*.docx")):
    pdf_path = docx_path.with_suffix(".pdf")
    if not pdf_path.exists():
        continue
    with pymupdf.open(pdf_path) as pdf:
        pages = len(pdf)
        pdf_text = [" ".join(page.get_text("text").split()) for page in pdf]
    document = Document(docx_path)
    if pages % 10 == 1 and pages % 100 != 11:
        page_word = "страницу"
    elif pages % 10 in (2, 3, 4) and pages % 100 not in (12, 13, 14):
        page_word = "страницы"
    else:
        page_word = "страниц"
    for paragraph in document.paragraphs:
        if paragraph.text.startswith("Отчет по летней практике содержит"):
            updated = re.sub(r"содержит \d+ \w+", f"содержит {pages} {page_word}", paragraph.text)
            if paragraph.runs:
                paragraph.runs[0].text = updated
                for run in paragraph.runs[1:]:
                    run.text = ""
            else:
                paragraph.add_run(updated)
            break

    if "Ефименко" in docx_path.name:
        headings = [
            ("Введение", "Введение"),
            ("1 Анализ предметной области и средств построения веб-интерфейса", "1 Анализ предметной области"),
            ("2 Модель пользовательского представления и сценарии работы", "2 Модель пользовательского представления"),
            ("3 Проектирование клиентского приложения", "3 Проектирование клиентского приложения"),
            ("4 Программная реализация прототипа", "4 Программная реализация прототипа"),
            ("Заключение", "Заключение"),
            ("Список использованных источников", "Список использованных источников"),
        ]
        table = next(
            table
            for table in document.tables
            if table.rows and table.columns and table.cell(0, 0).text.strip() == "Введение"
        )
        for row_index, (_, probe) in enumerate(headings):
            page_number = 0
            for text in pdf_text[5:]:
                if probe in text:
                    match = re.match(r"^(\d+)", text)
                    if match:
                        page_number = int(match.group(1))
                    break
            cell = table.cell(row_index, 1)
            cell.text = str(page_number)
            cell.paragraphs[0].alignment = 1
            for run in cell.paragraphs[0].runs:
                run.font.name = "Times New Roman"
                run.font.size = Pt(12)
    document.save(docx_path)
    print(f"{docx_path.name}: {pages} pages")
