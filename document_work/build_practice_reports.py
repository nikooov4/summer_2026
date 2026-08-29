from __future__ import annotations

from pathlib import Path
from typing import Iterable

from docx import Document
from docx.document import Document as DocxDocument
from docx.enum.table import WD_CELL_VERTICAL_ALIGNMENT, WD_TABLE_ALIGNMENT
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_BREAK
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Pt


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "Готовые_материалы"
ASSETS = ROOT / "document_work" / "generated_assets"

EF_SOURCE = OUTPUT / "ПЗ_ЕфименкоНП_обновленная.docx"
LOK_SOURCE = OUTPUT / "ПЗ_ЛоктевМС_обновленная.docx"

EF_OUTPUT = OUTPUT / "Отчет_по_летней_практике_ЕфименкоНП.docx"
LOK_OUTPUT = OUTPUT / "Отчет_по_летней_практике_ЛоктевМС.docx"


SOURCES = [
    "Microsoft. ASP.NET Core Blazor documentation [Электронный ресурс]. URL: https://learn.microsoft.com/aspnet/core/blazor/.",
    "Microsoft. ASP.NET Core Razor components [Электронный ресурс]. URL: https://learn.microsoft.com/aspnet/core/blazor/components/.",
    "Microsoft. Call a web API from an ASP.NET Core Blazor app [Электронный ресурс]. URL: https://learn.microsoft.com/aspnet/core/blazor/call-web-api.",
    "Microsoft. C# documentation [Электронный ресурс]. URL: https://learn.microsoft.com/dotnet/csharp/.",
    "Bray T. The JavaScript Object Notation (JSON) Data Interchange Format. RFC 8259. IETF, 2017.",
    "Shafranovich Y. Common Format and MIME Type for Comma-Separated Values (CSV) Files. RFC 4180. IETF, 2005.",
    "ISO/IEC/IEEE 12207:2017. Systems and software engineering - Software life cycle processes. Geneva: ISO, 2017.",
    "Bourque P., Fairley R. E. Guide to the Software Engineering Body of Knowledge (SWEBOK Guide). Version 4.0. IEEE Computer Society, 2024.",
    "Соммервилл И. Инженерия программного обеспечения. 10-е изд. М.: Вильямс, 2021.",
    "Pressman R. S., Maxim B. R. Software Engineering: A Practitioner's Approach. 9th ed. McGraw-Hill, 2020.",
    "Fowler M. Refactoring: Improving the Design of Existing Code. 2nd ed. Addison-Wesley, 2018.",
    "Martin R. C. Clean Architecture: A Craftsman's Guide to Software Structure and Design. Pearson, 2017.",
    "Нильсен Я. Веб-дизайн: удобство использования веб-сайтов. М.: Вильямс, 2006. 368 с.",
    "Ларман К. Применение UML 2.0 и шаблонов проектирования. 3-е изд. М.: Вильямс, 2013. 736 с.",
    "Рихтер Дж. CLR via C#. Программирование на платформе Microsoft .NET Framework. 4-е изд. СПб.: Питер, 2013. 896 с.",
    "Fielding R., Nottingham M., Reschke J. HTTP Semantics. RFC 9110. IETF, 2022.",
    "Web Content Accessibility Guidelines (WCAG) 2.2. W3C Recommendation, 2023.",
]


EF_TASKS = [
    (
        "Анализ проектов uir-main и omelea-dev-lms, определение границ переноса учета посещаемости и требований к отображению данных.",
        "Аналитический обзор, разделы отчета",
        "28.06.2026",
    ),
    (
        "Интеграция журнала посещаемости в интерфейс LMS с сохранением исходного конструктора ведомостей как самостоятельного раздела.",
        "Исходный код, интерфейс",
        "02.07.2026",
    ),
    (
        "Реализация статусов, опозданий, комментариев, поиска, фильтрации, массовой отметки, сохранения и отмены изменений.",
        "Исходный код, сценарии проверки",
        "05.07.2026",
    ),
    (
        "Реализация личного режима студента и тестовой модели доступа для администратора, преподавателя и студента.",
        "Исходный код, контрольные данные",
        "08.07.2026",
    ),
    (
        "Апробация основных сценариев, контроль работоспособности конструктора ведомостей и оформление отчетных материалов.",
        "Протокол проверки, отчет, дневник",
        "09.07.2026",
    ),
]


LOK_TASKS = [
    (
        "Анализ требований к мониторингу посещаемости, показателям полноты журналов и административным операциям в образовательной LMS.",
        "Аналитический обзор, разделы отчета",
        "28.06.2026",
    ),
    (
        "Разработка страницы мониторинга с выбором периода, фильтрами журналов и расчетом показателей посещаемости и заполненности.",
        "Исходный код, интерфейс",
        "02.07.2026",
    ),
    (
        "Реализация списка студентов, требующих внимания, карточки выбранного студента и личного экрана показателей.",
        "Исходный код, сценарии проверки",
        "05.07.2026",
    ),
    (
        "Разработка тестовой админ-панели пользователей, курсов и зачислений, а также обмена пользователями в JSON/CSV.",
        "Исходный код, контрольные данные",
        "08.07.2026",
    ),
    (
        "Апробация мониторинга и административных сценариев, подготовка отчета, дневника и презентационных материалов.",
        "Протокол проверки, отчет, дневник",
        "09.07.2026",
    ),
]


def all_paragraphs(document: DocxDocument) -> Iterable:
    for paragraph in document.paragraphs:
        yield paragraph
    for table in document.tables:
        for row in table.rows:
            for cell in row.cells:
                for paragraph in cell.paragraphs:
                    yield paragraph
    for section in document.sections:
        for part in (section.header, section.footer):
            for paragraph in part.paragraphs:
                yield paragraph


def set_paragraph_text(paragraph, text: str) -> None:
    if paragraph.runs:
        paragraph.runs[0].text = text
        for run in paragraph.runs[1:]:
            run.text = ""
    else:
        paragraph.add_run(text)


def replace_everywhere(document: DocxDocument, old: str, new: str) -> None:
    for paragraph in all_paragraphs(document):
        if old in paragraph.text:
            set_paragraph_text(paragraph, paragraph.text.replace(old, new))


def find_paragraph(document: DocxDocument, prefix: str):
    for paragraph in document.paragraphs:
        if paragraph.text.strip().startswith(prefix):
            return paragraph
    raise ValueError(f"Paragraph not found: {prefix}")


def set_starts(document: DocxDocument, prefix: str, text: str) -> None:
    set_paragraph_text(find_paragraph(document, prefix), text)


def set_run_font(run, size: float, bold: bool = False) -> None:
    run.font.name = "Times New Roman"
    run._element.get_or_add_rPr().rFonts.set(qn("w:eastAsia"), "Times New Roman")
    run.font.size = Pt(size)
    run.font.bold = bold


def add_paragraph(
    document: DocxDocument,
    elements: list,
    text: str = "",
    size: float = 10,
    bold: bool = False,
    align=WD_ALIGN_PARAGRAPH.LEFT,
    before: float = 0,
    after: float = 0,
):
    paragraph = document.add_paragraph()
    paragraph.alignment = align
    paragraph.paragraph_format.space_before = Pt(before)
    paragraph.paragraph_format.space_after = Pt(after)
    paragraph.paragraph_format.line_spacing = 1.0
    run = paragraph.add_run(text)
    set_run_font(run, size, bold)
    elements.append(paragraph._p)
    return paragraph


def set_cell(cell, text: str, size: float = 8, bold: bool = False, center: bool = False) -> None:
    cell.text = ""
    cell.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
    paragraph = cell.paragraphs[0]
    paragraph.alignment = WD_ALIGN_PARAGRAPH.CENTER if center else WD_ALIGN_PARAGRAPH.LEFT
    paragraph.paragraph_format.space_after = Pt(0)
    paragraph.paragraph_format.line_spacing = 1.0
    run = paragraph.add_run(text)
    set_run_font(run, size, bold)


def set_cell_width(cell, width_cm: float) -> None:
    cell.width = Cm(width_cm)
    properties = cell._tc.get_or_add_tcPr()
    width = properties.first_child_found_in("w:tcW")
    if width is None:
        width = OxmlElement("w:tcW")
        properties.append(width)
    width.set(qn("w:w"), str(int(width_cm * 567)))
    width.set(qn("w:type"), "dxa")


def remove_table_borders(table) -> None:
    properties = table._tbl.tblPr
    borders = properties.first_child_found_in("w:tblBorders")
    if borders is None:
        borders = OxmlElement("w:tblBorders")
        properties.append(borders)
    for edge in ("top", "left", "bottom", "right", "insideH", "insideV"):
        element = OxmlElement(f"w:{edge}")
        element.set(qn("w:val"), "nil")
        borders.append(element)


def add_institute_header(document: DocxDocument, elements: list) -> None:
    add_paragraph(document, elements, "МИНИСТЕРСТВО НАУКИ И ВЫСШЕГО ОБРАЗОВАНИЯ РОССИЙСКОЙ ФЕДЕРАЦИИ", 8.5, True, WD_ALIGN_PARAGRAPH.CENTER)
    add_paragraph(document, elements, "ФЕДЕРАЛЬНОЕ ГОСУДАРСТВЕННОЕ АВТОНОМНОЕ ОБРАЗОВАТЕЛЬНОЕ УЧРЕЖДЕНИЕ ВЫСШЕГО ОБРАЗОВАНИЯ", 8, True, WD_ALIGN_PARAGRAPH.CENTER)
    add_paragraph(document, elements, "Национальный исследовательский ядерный университет «МИФИ»", 9.5, True, WD_ALIGN_PARAGRAPH.CENTER, after=6)

    table = document.add_table(rows=1, cols=2)
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    table.autofit = False
    set_cell_width(table.cell(0, 0), 4.0)
    set_cell_width(table.cell(0, 1), 12.5)
    left = table.cell(0, 0).paragraphs[0]
    left.alignment = WD_ALIGN_PARAGRAPH.CENTER
    left.add_run().add_picture(str(ASSETS / "mephi-logo.png"), width=Cm(2.5))
    set_cell(table.cell(0, 1), "Институт интеллектуальных кибернетических систем\nКАФЕДРА КИБЕРНЕТИКИ", 10, True, True)
    remove_table_borders(table)
    elements.append(table._tbl)


def add_student_line(document: DocxDocument, elements: list, full_name: str) -> None:
    table = document.add_table(rows=2, cols=4)
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    table.autofit = False
    widths = [3.0, 3.0, 2.0, 8.7]
    values = ["Студенту гр.", "Б23-504", "", full_name]
    for index, value in enumerate(values):
        set_cell_width(table.cell(0, index), widths[index])
        set_cell(table.cell(0, index), value, 10, False, index != 0)
    captions = ["", "(группа)", "", "(ФИО)"]
    for index, value in enumerate(captions):
        set_cell_width(table.cell(1, index), widths[index])
        set_cell(table.cell(1, index), value, 8, False, True)
    remove_table_borders(table)
    elements.append(table._tbl)


def add_assignment_table(document: DocxDocument, elements: list, tasks: list[tuple[str, str, str]]) -> None:
    table = document.add_table(rows=1, cols=5)
    table.style = "Table Grid"
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    table.autofit = False
    widths = [0.8, 8.0, 3.0, 2.0, 3.2]
    headers = ["№\nп/п", "Содержание работы", "Форма\nотчетности", "Срок\nисполнения", "Отметка о выполнении\nДата, подпись рук."]
    for index, value in enumerate(headers):
        set_cell_width(table.cell(0, index), widths[index])
        set_cell(table.cell(0, index), value, 7.5, True, True)
    for number, (content, form, deadline) in enumerate(tasks, start=1):
        row = table.add_row()
        values = [str(number), content, form, deadline, ""]
        for index, value in enumerate(values):
            set_cell_width(row.cells[index], widths[index])
            set_cell(row.cells[index], value, 7.4, False, index in (0, 2, 3, 4))
    elements.append(table._tbl)


def add_reference(document: DocxDocument, elements: list, number: int, source: str) -> None:
    paragraph = add_paragraph(document, elements, f"{number}. {source}", 8.5, False, WD_ALIGN_PARAGRAPH.JUSTIFY, after=1)
    paragraph.paragraph_format.left_indent = Cm(0.4)
    paragraph.paragraph_format.first_line_indent = Cm(-0.4)


def add_signature_table(document: DocxDocument, elements: list, student_short: str) -> None:
    table = document.add_table(rows=3, cols=4)
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    table.autofit = False
    widths = [4.2, 4.2, 2.1, 6.2]
    rows = [
        ["Дата выдачи задания:", "", "Руководитель", "Рословцев В.В."],
        ["20 февраля 2026 г.", "", "Студент", student_short],
        ["", "", "", ""],
    ]
    for row_index, values in enumerate(rows):
        for column_index, value in enumerate(values):
            set_cell_width(table.cell(row_index, column_index), widths[column_index])
            set_cell(table.cell(row_index, column_index), value, 9, False, column_index in (1, 3))
    remove_table_borders(table)
    elements.append(table._tbl)


def create_assignment_pages(
    document: DocxDocument,
    full_name: str,
    student_short: str,
    topic: str,
    tasks: list[tuple[str, str, str]],
) -> tuple[list, list]:
    first: list = []
    second: list = []

    add_institute_header(document, first)
    add_paragraph(document, first, "Задание на летнюю практику", 18, True, WD_ALIGN_PARAGRAPH.CENTER, before=8, after=8)
    add_student_line(document, first, full_name)
    add_paragraph(document, first, "ТЕМА ПРАКТИКИ", 12, True, WD_ALIGN_PARAGRAPH.CENTER, before=4, after=2)
    add_paragraph(document, first, topic, 10, True, WD_ALIGN_PARAGRAPH.CENTER, after=5)
    add_paragraph(document, first, "ЗАДАНИЕ", 12, True, WD_ALIGN_PARAGRAPH.CENTER, after=3)
    add_assignment_table(document, first, tasks)
    add_paragraph(document, first, "ЛИТЕРАТУРА", 11, True, WD_ALIGN_PARAGRAPH.CENTER, before=4, after=2)
    add_reference(document, first, 1, SOURCES[0])

    for index, source in enumerate(SOURCES[1:], start=2):
        add_reference(document, second, index, source)
    add_signature_table(document, second, student_short)
    return first, second


def move_before(anchor, elements: list) -> None:
    for element in elements:
        anchor._p.addprevious(element)


def page_break_element(document: DocxDocument):
    paragraph = document.add_paragraph()
    paragraph.add_run().add_break(WD_BREAK.PAGE)
    return paragraph._p


def remove_range(document: DocxDocument, start, end) -> None:
    body = document._element.body
    active = False
    for element in list(body):
        if element is end:
            break
        if element is start:
            active = True
        if active:
            body.remove(element)


def remove_appendices(document: DocxDocument) -> None:
    anchor = find_paragraph(document, "Приложение А")
    body = document._element.body
    previous = anchor._p.getprevious()
    if previous is not None and not "".join(previous.itertext()).strip() and previous.findall(".//" + qn("w:br")):
        body.remove(previous)
    active = False
    for element in list(body):
        if element is anchor._p:
            active = True
        if active and element.tag != qn("w:sectPr"):
            body.remove(element)

    for table in document.tables:
        for row in list(table.rows):
            text = " ".join(cell.text for cell in row.cells).strip()
            if text.startswith("Приложение А") or text.startswith("Приложение Б"):
                table._tbl.remove(row._tr)


def insert_practice_paragraph(document: DocxDocument, anchor_prefix: str, text: str) -> None:
    anchor = find_paragraph(document, anchor_prefix)
    paragraph = OxmlElement("w:p")
    anchor._p.addnext(paragraph)
    inserted = anchor._parent.add_paragraph()
    inserted._p.getparent().remove(inserted._p)
    paragraph.addnext(inserted._p)
    inserted.style = anchor.style
    inserted.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    inserted.paragraph_format.first_line_indent = Cm(1.25)
    inserted.paragraph_format.line_spacing = 1.5
    run = inserted.add_run(text)
    set_run_font(run, 14)


def simplify_efimenko_toc(document: DocxDocument) -> None:
    entries = [
        "Введение",
        "1 Анализ предметной области и средств построения веб-интерфейса",
        "2 Модель пользовательского представления и сценарии работы",
        "3 Проектирование клиентского приложения",
        "4 Программная реализация прототипа",
        "Заключение",
        "Список использованных источников",
    ]
    table = document.tables[2]
    while len(table.rows) < len(entries):
        table.add_row()
    for index, title in enumerate(entries):
        set_cell(table.cell(index, 0), title, 12)
        set_cell(table.cell(index, 1), "0", 12, False, True)
    for row in list(table.rows[len(entries):]):
        table._tbl.remove(row._tr)


def common_report_replacements(document: DocxDocument) -> None:
    replacements = [
        ("Пояснительная записка", "Отчет по летней практике"),
        ("пояснительной записки", "отчета по летней практике"),
        ("пояснительная записка", "отчет по летней практике"),
        ("к учебно-исследовательской работе студента на тему:", "студента на тему:"),
        ("учебно-исследовательской работы", "летней практики"),
        ("учебно-исследовательской работе", "летней практике"),
        ("Задание на УИР", "Задание на летнюю практику"),
        ("ТЕМА УИР", "ТЕМА ПРАКТИКИ"),
        ("разделы ПЗ", "разделы отчета"),
        ("Разделы ПЗ", "Разделы отчета"),
        ("текст ПЗ", "текст отчета"),
        ("Текст ПЗ", "Текст отчета"),
        ("РСПЗ", "отчета"),
        ("2 приложения", "без приложений"),
    ]
    for old, new in replacements:
        replace_everywhere(document, old, new)


def build_efimenko() -> Path:
    document = Document(EF_SOURCE)
    document.core_properties.title = "Отчет по летней практике: учет посещаемости и тестовая модель доступа"
    document.core_properties.author = "Ефименко Н.П."
    common_report_replacements(document)

    set_starts(document, "Отчет по летней практике", "Отчет по летней практике")
    set_starts(document, "Разработка пользовательского веб-интерфейса", "«Интеграция учета посещаемости и тестовой ролевой модели в образовательную LMS»")
    set_starts(document, "Общий объем", "Отчет по летней практике содержит 0 страниц, 15 таблиц, 9 рисунков и 20 использованных источников.")
    set_starts(document, "Инженерная часть содержит", "Инженерная часть содержит компонентную архитектуру, навигацию и перспективные контракты REST API. Практическая часть описывает сохранение исходного конструктора ведомостей, добавление журнала посещений, личной истории, демонстрационных входов администратора, преподавателя и студента, а также связь страниц с каталогом пользователей, курсов и назначений.")
    set_starts(document, "Личный вклад автора включает", "Личный вклад автора включает интеграцию журнала посещаемости, статусы и правила ввода, поиск и фильтрацию, личный режим студента, демонстрационные входы и тестовую ролевую модель с ограничением доступных дисциплин и операций.")
    insert_practice_paragraph(
        document,
        "Личный вклад автора включает",
        "В ходе летней практики идеи и наработки экспериментального проекта uir-main были перенесены в omelea-dev-lms с сохранением его визуального оформления. Исходный конструктор ведомостей оставлен самостоятельным разделом, а новый функционал добавлен рядом с ним и использует общий каркас приложения.",
    )
    simplify_efimenko_toc(document)

    remove_appendices(document)

    first, second = create_assignment_pages(
        document,
        "Ефименко Николай Павлович",
        "Ефименко Н.П.",
        "Интеграция учета посещаемости и тестовой ролевой модели в образовательную LMS",
        EF_TASKS,
    )
    assignment_start = document.paragraphs[8]._p
    assignment_end = document.paragraphs[10]
    remove_range(document, assignment_start, assignment_end._p)
    move_before(assignment_end, first + [page_break_element(document)] + second)

    document.save(EF_OUTPUT)
    return EF_OUTPUT


def build_loktev() -> Path:
    document = Document(LOK_SOURCE)
    document.core_properties.title = "Отчет по летней практике: мониторинг и администрирование посещаемости"
    document.core_properties.author = "Локтев М.С."

    assignment_start = document.paragraphs[10]._p
    abstract_start = find_paragraph(document, "Реферат")._p
    remove_range(document, assignment_start, abstract_start)
    common_report_replacements(document)

    replace_everywhere(document, "Создание клиентского модуля мониторинга и администрирования посещаемости учебных занятий", "«Разработка модуля мониторинга и администрирования посещаемости в образовательной LMS»")
    set_starts(document, "Общий объем", "Отчет по летней практике содержит 0 страниц, 16 таблиц, 9 рисунков и 20 использованных источников.")
    insert_practice_paragraph(
        document,
        "Личный вклад автора включает",
        "В ходе летней практики в omelea-dev-lms добавлены рабочие страницы мониторинга и администрирования, выполненные в общем визуальном стиле проекта. Исходный конструктор ведомостей сохранен без существенной переработки и используется как самостоятельный раздел приложения.",
    )

    remove_appendices(document)

    first, second = create_assignment_pages(
        document,
        "Локтев Максим Сергеевич",
        "Локтев М.С.",
        "Разработка модуля мониторинга и администрирования посещаемости в образовательной LMS",
        LOK_TASKS,
    )
    abstract = find_paragraph(document, "Реферат")
    move_before(abstract, first + [page_break_element(document)] + second + [page_break_element(document)])

    document.save(LOK_OUTPUT)
    return LOK_OUTPUT


def main() -> None:
    OUTPUT.mkdir(parents=True, exist_ok=True)
    print(build_efimenko())
    print(build_loktev())


if __name__ == "__main__":
    main()
