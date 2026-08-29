from pathlib import Path

import pymupdf
from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "Готовые_материалы"
TARGETS = {
    "Дневник_летней_практики_ЕфименкоНП.pdf": [],
    "Дневник_летней_практики_ЛоктевМС.pdf": [],
    "Отчет_по_летней_практике_ЕфименкоНП.pdf": [
        "Личный вклад автора включает",
        "Рисунок 3.2",
        "Рисунок 4.1",
        "Заключение",
    ],
    "Отчет_по_летней_практике_ЛоктевМС.pdf": [
        "Личный вклад автора включает",
        "Рисунок 3.3",
        "Рисунок 4.1",
        "Заключение",
    ],
    "ПЗ_ЕфименкоНП_обновленная.pdf": [
        "Личный вклад автора включает",
        "Рисунок 3.2",
        "Рисунок 3.3",
        "Рисунок 4.1",
    ],
    "ПЗ_ЛоктевМС_обновленная.pdf": [
        "Личный вклад автора включает",
        "Рисунок 3.3",
        "Рисунок 4.1",
        "4.4 Модуль мониторинга",
    ],
}
FONT = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", 18)

for file_name, needles in TARGETS.items():
    document = pymupdf.open(OUTPUT / file_name)
    pages = {0, 1, 2, 3}
    for index, page in enumerate(document):
        text = page.get_text("text")
        if any(needle in text for needle in needles):
            pages.add(index)

    previews = []
    for index in sorted(pages):
        pixmap = document[index].get_pixmap(matrix=pymupdf.Matrix(1.25, 1.25), alpha=False)
        image = Image.frombytes("RGB", (pixmap.width, pixmap.height), pixmap.samples)
        image.thumbnail((500, 707))
        previews.append((index + 1, image.copy()))

    columns = 4
    rows = (len(previews) + columns - 1) // columns
    sheet = Image.new("RGB", (columns * 500, rows * 745), "white")
    draw = ImageDraw.Draw(sheet)
    for item_index, (page_number, image) in enumerate(previews):
        x = (item_index % columns) * 500
        y = (item_index // columns) * 745
        sheet.paste(image, (x, y))
        draw.text((x + 8, y + 712), f"Страница {page_number}", font=FONT, fill="#1D334A")

    sheet.save(Path(__file__).resolve().parent / f"report-check-{Path(file_name).stem}.png")
