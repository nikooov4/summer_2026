from __future__ import annotations

from pathlib import Path

from docx import Document
from docx.enum.table import WD_CELL_VERTICAL_ALIGNMENT, WD_TABLE_ALIGNMENT
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_BREAK
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Pt, RGBColor as DocxRGBColor
from PIL import Image
from pptx import Presentation
from pptx.dml.color import RGBColor
from pptx.enum.shapes import MSO_SHAPE
from pptx.enum.text import MSO_ANCHOR, PP_ALIGN
from pptx.util import Inches, Pt as PptPt


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "Готовые_материалы" / "Летняя_практика"
ASSETS = ROOT / "document_work" / "generated_assets"
SCREENS = ROOT / "document_work" / "screenshots"

REPORT_PATH = OUTPUT / "Отчет_по_летней_практике_ЕфименкоНП_v2.docx"
DIARY_PATH = OUTPUT / "Дневник_летней_практики_ЕфименкоНП_v2.docx"
PRESENTATION_PATH = OUTPUT / "Презентация_ЕфименкоНП_v2.pptx"
MEMO_PATH = OUTPUT / "Памятка_к_v2_ЕфименкоНП.md"

NAVY = "18324B"
BLUE = "5169D8"
PURPLE = "7251A6"
ORANGE = "E86F35"
GREEN = "27865B"
RED = "C64747"
INK = "243342"
GRAY = "6B7784"
LIGHT = "F3F6F9"
MID = "D9E1E8"
WHITE = "FFFFFF"


REPORT_TASKS = [
    (
        "Анализ исходного прототипа учета посещаемости в проекте uir-main и целевого интерфейса omelea-dev-lms.",
        "Аналитический обзор",
        "27.06.2026",
    ),
    (
        "Перенос моделей, сервиса и экранов учета посещаемости в Blazor WebAssembly-приложение LMS.",
        "Исходный код",
        "30.06.2026",
    ),
    (
        "Адаптация журнала преподавателя и мониторинга к навигации и визуальному оформлению omelea-dev-lms.",
        "Интерфейс, исходный код",
        "03.07.2026",
    ),
    (
        "Доработка личного режима студента, тестовой ролевой модели и ограничения доступа к дисциплинам.",
        "Сценарии доступа",
        "06.07.2026",
    ),
    (
        "Разработка тестовой панели администратора для управления пользователями, курсами и зачислениями, импорта и экспорта пользователей.",
        "Исходный код, интерфейс",
        "08.07.2026",
    ),
    (
        "Проверка пользовательских сценариев, сохранности исходного конструктора ведомостей и оформление отчетных материалов.",
        "Протокол проверки, отчет",
        "09.07.2026",
    ),
]

SOURCES = [
    "Microsoft. ASP.NET Core Blazor documentation [Электронный ресурс]. URL: https://learn.microsoft.com/aspnet/core/blazor/.",
    "Microsoft. ASP.NET Core Razor components [Электронный ресурс]. URL: https://learn.microsoft.com/aspnet/core/blazor/components/.",
    "Microsoft. Dependency injection in ASP.NET Core [Электронный ресурс]. URL: https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection.",
    "Microsoft. ASP.NET Core Blazor state management [Электронный ресурс]. URL: https://learn.microsoft.com/aspnet/core/blazor/state-management.",
    "Microsoft. C# documentation [Электронный ресурс]. URL: https://learn.microsoft.com/dotnet/csharp/.",
    "Bray T. The JavaScript Object Notation (JSON) Data Interchange Format. RFC 8259. IETF, 2017.",
    "Shafranovich Y. Common Format and MIME Type for Comma-Separated Values (CSV) Files. RFC 4180. IETF, 2005.",
    "Fowler M. Refactoring: Improving the Design of Existing Code. 2nd ed. Addison-Wesley, 2018.",
    "Соммервилл И. Инженерия программного обеспечения. 10-е изд. М.: Вильямс, 2021.",
    "Нильсен Я. Веб-дизайн: удобство использования веб-сайтов. М.: Вильямс, 2006. 368 с.",
    "Ларман К. Применение UML 2.0 и шаблонов проектирования. 3-е изд. М.: Вильямс, 2013. 736 с.",
    "ISO/IEC/IEEE 12207:2017. Systems and software engineering - Software life cycle processes. Geneva: ISO, 2017.",
]


def set_repeat_table_header(row) -> None:
    properties = row._tr.get_or_add_trPr()
    repeat = OxmlElement("w:tblHeader")
    repeat.set(qn("w:val"), "true")
    properties.append(repeat)


def shade_cell(cell, color: str) -> None:
    properties = cell._tc.get_or_add_tcPr()
    shade = OxmlElement("w:shd")
    shade.set(qn("w:fill"), color)
    properties.append(shade)


def set_cell_margins(cell, top: int = 70, start: int = 90, bottom: int = 70, end: int = 90) -> None:
    properties = cell._tc.get_or_add_tcPr()
    margins = properties.first_child_found_in("w:tcMar")
    if margins is None:
        margins = OxmlElement("w:tcMar")
        properties.append(margins)
    for name, value in (("top", top), ("start", start), ("bottom", bottom), ("end", end)):
        node = margins.find(qn(f"w:{name}"))
        if node is None:
            node = OxmlElement(f"w:{name}")
            margins.append(node)
        node.set(qn("w:w"), str(value))
        node.set(qn("w:type"), "dxa")


def set_repeat_keep(paragraph) -> None:
    paragraph.paragraph_format.keep_with_next = True


def add_page_number(paragraph) -> None:
    paragraph.alignment = WD_ALIGN_PARAGRAPH.CENTER
    run = paragraph.add_run()
    begin = OxmlElement("w:fldChar")
    begin.set(qn("w:fldCharType"), "begin")
    instruction = OxmlElement("w:instrText")
    instruction.set(qn("xml:space"), "preserve")
    instruction.text = " PAGE "
    end = OxmlElement("w:fldChar")
    end.set(qn("w:fldCharType"), "end")
    run._r.extend([begin, instruction, end])
    run.font.name = "Times New Roman"
    run.font.size = Pt(11)


def configure_report(document: Document) -> None:
    section = document.sections[0]
    section.page_width = Cm(21)
    section.page_height = Cm(29.7)
    section.left_margin = Cm(3)
    section.right_margin = Cm(1.5)
    section.top_margin = Cm(2)
    section.bottom_margin = Cm(2)
    section.different_first_page_header_footer = True
    add_page_number(section.footer.paragraphs[0])

    normal = document.styles["Normal"]
    normal.font.name = "Times New Roman"
    normal.font.size = Pt(14)
    normal._element.rPr.rFonts.set(qn("w:eastAsia"), "Times New Roman")
    normal.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    normal.paragraph_format.first_line_indent = Cm(1.25)
    normal.paragraph_format.line_spacing = 1.5
    normal.paragraph_format.space_after = Pt(0)

    for style_name, size, alignment in (
        ("Title", 16, WD_ALIGN_PARAGRAPH.CENTER),
        ("Heading 1", 16, WD_ALIGN_PARAGRAPH.CENTER),
        ("Heading 2", 14, WD_ALIGN_PARAGRAPH.LEFT),
        ("Heading 3", 14, WD_ALIGN_PARAGRAPH.LEFT),
    ):
        style = document.styles[style_name]
        style.font.name = "Times New Roman"
        style.font.size = Pt(size)
        style.font.bold = True
        style.font.color.rgb = DocxRGBColor(0, 0, 0)
        style._element.rPr.rFonts.set(qn("w:eastAsia"), "Times New Roman")
        style.paragraph_format.alignment = alignment
        style.paragraph_format.first_line_indent = Cm(0)
        style.paragraph_format.space_before = Pt(6)
        style.paragraph_format.space_after = Pt(6)
        style.paragraph_format.keep_with_next = True


def font_run(run, size: float = 14, bold: bool = False, italic: bool = False) -> None:
    run.font.name = "Times New Roman"
    run._element.get_or_add_rPr().rFonts.set(qn("w:eastAsia"), "Times New Roman")
    run.font.size = Pt(size)
    run.font.bold = bold
    run.font.italic = italic


def add_report_paragraph(
    document: Document,
    text: str,
    *,
    bold_lead: str | None = None,
    align=WD_ALIGN_PARAGRAPH.JUSTIFY,
    indent: bool = True,
    size: float = 14,
    before: float = 0,
    after: float = 0,
    spacing: float = 1.5,
):
    paragraph = document.add_paragraph()
    paragraph.alignment = align
    paragraph.paragraph_format.first_line_indent = Cm(1.25) if indent else Cm(0)
    paragraph.paragraph_format.space_before = Pt(before)
    paragraph.paragraph_format.space_after = Pt(after)
    paragraph.paragraph_format.line_spacing = spacing
    if bold_lead and text.startswith(bold_lead):
        lead = paragraph.add_run(bold_lead)
        font_run(lead, size, True)
        rest = paragraph.add_run(text[len(bold_lead):])
        font_run(rest, size)
    else:
        run = paragraph.add_run(text)
        font_run(run, size)
    return paragraph


def add_report_bullets(document: Document, items: list[str], size: float = 13.5) -> None:
    for item in items:
        paragraph = document.add_paragraph(style="List Bullet")
        paragraph.paragraph_format.left_indent = Cm(1.25)
        paragraph.paragraph_format.first_line_indent = Cm(-0.55)
        paragraph.paragraph_format.line_spacing = 1.25
        paragraph.paragraph_format.space_after = Pt(2)
        for run in paragraph.runs:
            font_run(run, size)
        if not paragraph.runs:
            font_run(paragraph.add_run(item), size)
        else:
            paragraph.runs[0].text = item


def add_report_heading(document: Document, text: str, level: int = 1) -> None:
    paragraph = document.add_heading(text, level=level)
    paragraph.paragraph_format.keep_with_next = True


def add_page_break(document: Document) -> None:
    paragraph = document.add_paragraph()
    paragraph.add_run().add_break(WD_BREAK.PAGE)


def set_table_cell(cell, text: str, size: float = 10.5, bold: bool = False, center: bool = False) -> None:
    cell.text = ""
    cell.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
    set_cell_margins(cell)
    paragraph = cell.paragraphs[0]
    paragraph.alignment = WD_ALIGN_PARAGRAPH.CENTER if center else WD_ALIGN_PARAGRAPH.LEFT
    paragraph.paragraph_format.first_line_indent = Cm(0)
    paragraph.paragraph_format.space_after = Pt(0)
    paragraph.paragraph_format.line_spacing = 1.0
    run = paragraph.add_run(text)
    font_run(run, size, bold)


def add_report_table(
    document: Document,
    headers: list[str],
    rows: list[list[str]],
    widths: list[float] | None = None,
    font_size: float = 10.5,
):
    table = document.add_table(rows=1, cols=len(headers))
    table.style = "Table Grid"
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    table.autofit = widths is None
    set_repeat_table_header(table.rows[0])
    for index, (cell, header) in enumerate(zip(table.rows[0].cells, headers)):
        if widths:
            cell.width = Cm(widths[index])
        shade_cell(cell, "D9E1E8")
        set_table_cell(cell, header, font_size, True, True)
    for values in rows:
        row = table.add_row()
        for index, (cell, value) in enumerate(zip(row.cells, values)):
            if widths:
                cell.width = Cm(widths[index])
            set_table_cell(cell, value, font_size, False, index == 0 and len(headers) <= 4)
    document.add_paragraph().paragraph_format.space_after = Pt(0)
    return table


def add_table_caption(document: Document, text: str) -> None:
    paragraph = document.add_paragraph()
    paragraph.alignment = WD_ALIGN_PARAGRAPH.LEFT
    paragraph.paragraph_format.first_line_indent = Cm(0)
    paragraph.paragraph_format.space_before = Pt(4)
    paragraph.paragraph_format.space_after = Pt(3)
    paragraph.paragraph_format.keep_with_next = True
    font_run(paragraph.add_run(text), 12)


def add_report_figure(document: Document, image_path: Path, caption: str, width_cm: float = 16.0) -> None:
    paragraph = document.add_paragraph()
    paragraph.alignment = WD_ALIGN_PARAGRAPH.CENTER
    paragraph.paragraph_format.first_line_indent = Cm(0)
    paragraph.paragraph_format.space_before = Pt(4)
    paragraph.paragraph_format.space_after = Pt(3)
    paragraph.add_run().add_picture(str(image_path), width=Cm(width_cm))
    caption_paragraph = document.add_paragraph()
    caption_paragraph.alignment = WD_ALIGN_PARAGRAPH.CENTER
    caption_paragraph.paragraph_format.first_line_indent = Cm(0)
    caption_paragraph.paragraph_format.space_after = Pt(5)
    caption_paragraph.paragraph_format.keep_with_next = False
    font_run(caption_paragraph.add_run(caption), 12)


def add_title_page(document: Document) -> None:
    for text, size, bold, before, after in (
        ("МИНИСТЕРСТВО НАУКИ И ВЫСШЕГО ОБРАЗОВАНИЯ РОССИЙСКОЙ ФЕДЕРАЦИИ", 11, True, 0, 5),
        ("Федеральное государственное автономное образовательное учреждение высшего образования", 10, False, 0, 0),
        ("«Национальный исследовательский ядерный университет «МИФИ»", 12, True, 0, 20),
        ("Институт интеллектуальных кибернетических систем", 12, False, 0, 2),
        ("Кафедра №22 «Кибернетика»", 12, False, 0, 2),
        ("Направление подготовки 09.03.04 «Программная инженерия»", 12, False, 0, 28),
        ("ОТЧЕТ", 16, True, 0, 2),
        ("по летней практике", 16, True, 0, 10),
        ("на тему:", 14, False, 0, 4),
        ("«Перенос и доработка прототипа учета посещаемости в LMS»", 15, True, 0, 30),
    ):
        add_report_paragraph(
            document,
            text,
            align=WD_ALIGN_PARAGRAPH.CENTER,
            indent=False,
            size=size,
            before=before,
            after=after,
            spacing=1.0,
        ).runs[0].font.bold = bold

    table = document.add_table(rows=3, cols=3)
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    table.autofit = False
    rows = [
        ("Группа", "Б23-504", ""),
        ("Студент", "________________", "Ефименко Н.П."),
        ("Руководитель", "________________", "Рословцев В.В."),
    ]
    widths = [4.0, 5.0, 7.0]
    for row, values in zip(table.rows, rows):
        for index, (cell, value) in enumerate(zip(row.cells, values)):
            cell.width = Cm(widths[index])
            set_table_cell(cell, value, 12, False, index == 1)
            borders = OxmlElement("w:tcBorders")
            cell._tc.get_or_add_tcPr().append(borders)

    add_report_paragraph(document, "Москва 2026", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=12, before=56, spacing=1.0)


def add_assignment_pages(document: Document) -> None:
    add_page_break(document)
    add_report_paragraph(document, "НИЯУ МИФИ", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=13, after=2, spacing=1.0).runs[0].font.bold = True
    add_report_paragraph(document, "ИНСТИТУТ ИНТЕЛЛЕКТУАЛЬНЫХ КИБЕРНЕТИЧЕСКИХ СИСТЕМ", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=11, spacing=1.0)
    add_report_paragraph(document, "КАФЕДРА №22 «КИБЕРНЕТИКА»", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=12, after=18, spacing=1.0).runs[0].font.bold = True
    add_report_paragraph(document, "ЗАДАНИЕ НА ЛЕТНЮЮ ПРАКТИКУ", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=15, after=14, spacing=1.0).runs[0].font.bold = True
    add_report_paragraph(document, "Студенту группы Б23-504 Ефименко Николаю Павловичу", indent=False, size=12, after=8, spacing=1.0)
    add_report_paragraph(document, "Тема практики: «Перенос и доработка прототипа учета посещаемости в LMS»", indent=False, size=12, after=8, spacing=1.0)
    add_report_table(document, ["№", "Содержание работы", "Форма отчетности", "Срок"], [[str(i), *task] for i, task in enumerate(REPORT_TASKS, 1)], [0.8, 9.1, 3.5, 2.4], 8.5)

    add_page_break(document)
    add_report_heading(document, "Литература к заданию", 2)
    for index, source in enumerate(SOURCES[:7], 1):
        add_report_paragraph(document, f"{index}. {source}", indent=False, size=11, after=3, spacing=1.05)
    add_report_paragraph(document, "Дата выдачи задания: 26 июня 2026 г.", indent=False, size=12, before=16, after=14, spacing=1.0)
    sign = document.add_table(rows=2, cols=3)
    sign.alignment = WD_TABLE_ALIGNMENT.CENTER
    for row, values in zip(sign.rows, [("Руководитель", "________________", "Рословцев В.В."), ("Студент", "________________", "Ефименко Н.П.")]):
        for index, (cell, value) in enumerate(zip(row.cells, values)):
            set_table_cell(cell, value, 12, False, index == 1)
            cell._tc.get_or_add_tcPr().append(OxmlElement("w:tcBorders"))


def add_abstract(document: Document) -> None:
    add_page_break(document)
    add_report_heading(document, "Реферат", 1)
    add_report_paragraph(document, "Отчет по летней практике содержит 28 страниц, 8 таблиц, 11 рисунков, 12 использованных источников и 2 приложения.")
    add_report_paragraph(document, "Ключевые слова: УЧЕТ ПОСЕЩАЕМОСТИ, LMS, BLAZOR WEBASSEMBLY, RAZOR-КОМПОНЕНТЫ, РОЛЕВАЯ МОДЕЛЬ, МОНИТОРИНГ, ЭЛЕКТРОННЫЙ ЖУРНАЛ, CSV, JSON.")
    add_report_paragraph(document, "Цель практики состояла в переносе рабочего прототипа учета посещаемости из проекта uir-main в существующее приложение omelea-dev-lms и его адаптации к структуре, навигации и визуальному стилю целевой LMS.")
    add_report_paragraph(document, "В ходе работы были перенесены модели занятий, студентов и отметок, журнал преподавателя и экран мониторинга. Реализация дополнена тестовым каталогом пользователей, курсов и зачислений, демонстрационными учетными записями администратора, преподавателя и студента, фильтрацией дисциплин по назначению пользователя и отдельным личным режимом студента без операций редактирования.")
    add_report_paragraph(document, "В административной части реализованы операции добавления и удаления пользователей и курсов, назначения ролей на курсах, а также импорт и экспорт списка пользователей в форматах JSON и CSV. Исходный конструктор ведомостей сохранен как самостоятельный раздел и не перерабатывался в журнал посещаемости.")
    add_report_paragraph(document, "Данные прототипа размещаются в оперативной памяти Blazor-приложения. Сохранение журнала фиксирует подтвержденное состояние текущего сеанса, но не обеспечивает постоянное серверное хранение. Авторизация и роли носят демонстрационный характер и предназначены для проверки интерфейсных сценариев.")


def add_contents(document: Document) -> None:
    add_page_break(document)
    add_report_heading(document, "Содержание", 1)
    entries = [
        ("Введение", "6"),
        ("1 Анализ исходного и целевого проектов", "7"),
        ("1.1 Исходный прототип учета посещаемости", "7"),
        ("1.2 Особенности omelea-dev-lms и ограничения переноса", "8"),
        ("1.3 Требования к доработанному модулю", "9"),
        ("2 Проектирование интеграции", "10"),
        ("2.1 Размещение подсистемы в приложении LMS", "10"),
        ("2.2 Модель данных посещаемости", "11"),
        ("2.3 Тестовая модель доступа", "12"),
        ("2.4 Сохранение конструктора ведомостей", "13"),
        ("3 Реализация и доработка прототипа", "14"),
        ("3.1 Перенос компонентов и сервисов", "14"),
        ("3.2 Журнал преподавателя", "15"),
        ("3.3 Проверка и фиксация изменений", "16"),
        ("3.4 Личный режим студента", "17"),
        ("3.5 Административный каталог", "18"),
        ("3.6 Мониторинг посещаемости", "19"),
        ("3.7 Хранение данных и демонстрационная авторизация", "20"),
        ("4 Апробация и результаты", "21"),
        ("4.1 Проверка пользовательских сценариев", "21"),
        ("4.2 Проверка разграничения интерфейса", "22"),
        ("4.3 Ограничения и дальнейшее развитие", "23"),
        ("Заключение", "24"),
        ("Список использованных источников", "25"),
        ("Приложение А. Журнал преподавателя", "26"),
        ("Приложение Б. Режимы студента и администратора", "27"),
    ]
    for title, page in entries:
        paragraph = document.add_paragraph()
        paragraph.paragraph_format.first_line_indent = Cm(0)
        paragraph.paragraph_format.line_spacing = 1.0
        paragraph.paragraph_format.space_after = Pt(2)
        paragraph.paragraph_format.tab_stops.add_tab_stop(Cm(15.5))
        run = paragraph.add_run(f"{title}")
        font_run(run, 11.5)
        run = paragraph.add_run("\t" + page)
        font_run(run, 11.5)


def add_report_body(document: Document) -> None:
    add_page_break(document)
    add_report_heading(document, "Введение", 1)
    add_report_paragraph(document, "Системы управления обучением объединяют сведения о дисциплинах, учебных группах, ведомостях и активности обучающихся. Однако учет присутствия часто остается отдельным процессом: преподаватель ведет журнал в таблице или специализированном приложении, а затем повторно переносит результаты в общую информационную среду. Это увеличивает число действий и затрудняет получение сводной картины.")
    add_report_paragraph(document, "До начала летней практики в проекте uir-main существовал рабочий прототип журнала посещаемости. Он поддерживал выбор занятия, статусы присутствия, сведения об опоздании, комментарии, локальное подтверждение изменений, историю студента и мониторинг. Одновременно проект omelea-dev-lms содержал готовый пользовательский каркас LMS и конструктор ведомостей, но не имел встроенного раздела учета посещений.")
    add_report_paragraph(document, "Практическая задача состояла не в разработке второго независимого прототипа, а в переносе уже проверенной идеи в другой программный проект. При этом требовалось сохранить дизайн и привычную навигацию omelea-dev-lms, не заменять существующий конструктор ведомостей и расширить перенесенный функционал с учетом пользователей, дисциплин и различных режимов доступа.")
    add_report_paragraph(document, "Цель работы - перенести и доработать прототип учета посещаемости в составе omelea-dev-lms. Для достижения цели были проанализированы два проекта, выделены переносимые модели и сценарии, реализованы страницы журнала и мониторинга, добавлены тестовые роли и административный каталог, после чего проверены основные пользовательские маршруты.")
    add_report_paragraph(document, "Объектом работы является пользовательская часть образовательной LMS. Предметом работы являются методы интеграции модуля учета посещаемости в существующее Blazor-приложение с сохранением его интерфейсных соглашений и исходных разделов.")

    add_page_break(document)
    add_report_heading(document, "1 Анализ исходного и целевого проектов", 1)
    add_report_heading(document, "1.1 Исходный прототип учета посещаемости", 2)
    add_report_paragraph(document, "Исходный прототип размещался в проекте uir-main и был реализован как Blazor-приложение. Основу составляли страница электронного журнала, страница мониторинга, набор моделей предметной области и сервис AttendanceWorkspace. Сервис создавал демонстрационные занятия и студентов, формировал строки журнала, рассчитывал показатели и поддерживал фиксацию изменений.")
    add_report_paragraph(document, "Прототип уже задавал полезную предметную модель: занятие связано с дисциплиной, темой, аудиторией и временем проведения; для каждого студента хранится отдельная отметка. Статус может принимать значения «присутствовал», «опоздал», «уважительная причина», «отсутствовал» или «не отмечен». Для опоздания указывается количество минут, а для уважительной причины - пояснение.")
    add_table_caption(document, "Таблица 1 - Состав исходного прототипа")
    add_report_table(document, ["Элемент", "Состояние в uir-main", "Значение для переноса"], [
        ["Журнал", "Редактирование отметок одного занятия", "Основной рабочий сценарий преподавателя"],
        ["Личная история", "Выбор студента в демонстрационном режиме", "Основа отдельного режима обучающегося"],
        ["Мониторинг", "Показатели занятий и студентов", "Контроль полноты и отклонений"],
        ["Хранилище", "Локальный JSON-файл серверного прототипа", "Требует замены для WebAssembly"],
    ], [3.0, 6.2, 6.5], 9.8)
    add_report_paragraph(document, "Таким образом, переносился не внешний вид исходного приложения, а его предметная логика и проверенные сценарии. Визуальная часть должна была быть заново согласована с omelea-dev-lms.")

    add_page_break(document)
    add_report_heading(document, "1.2 Особенности omelea-dev-lms и ограничения переноса", 2)
    add_report_paragraph(document, "Целевой проект представляет собой Blazor WebAssembly-приложение с главной панелью, семантическими формами и разделом ведомостей. Его интерфейс использует светлую рабочую область, компактную боковую навигацию, панели с небольшим радиусом скругления, единые поля ввода и кнопки действий. Поэтому прямое копирование разметки uir-main привело бы к визуально обособленному разделу.")
    add_report_paragraph(document, "Второе отличие связано с моделью выполнения. AttendanceWorkspace исходного прототипа мог обращаться к файловой системе веб-сервера и сохранять JSON. Blazor WebAssembly выполняется в браузере и не имеет прямого доступа к произвольному серверному файлу. В перенесенной версии состояние размещено в singleton-сервисе текущего приложения. Это позволило проверить интерфейс, но означает сброс данных после полной перезагрузки.")
    add_report_paragraph(document, "Третьим ограничением являлось отсутствие готовой прикладной ролевой модели. В исходном dev-lms присутствует отдельный сервис авторизации с JWT и PostgreSQL, однако он не был связан с автономным интерфейсом ведомостей. Для практики потребовался тестовый контур, позволяющий воспроизводимо показать администратора, преподавателя и студента без настройки внешней инфраструктуры.")
    add_report_bullets(document, [
        "сохранить маршруты и поведение конструктора ведомостей;",
        "разместить посещаемость как новый раздел общей навигации;",
        "не зависеть от серверной базы данных при демонстрации;",
        "использовать общий каталог пользователей и дисциплин для журнала и мониторинга;",
        "не предоставлять студенту элементы изменения чужих отметок.",
    ])

    add_page_break(document)
    add_report_heading(document, "1.3 Требования к доработанному модулю", 2)
    add_report_paragraph(document, "После анализа были сформулированы требования, описывающие границу работы. Они ориентированы на демонстрационный прототип и не подменяют требования к промышленной информационной системе.")
    add_table_caption(document, "Таблица 2 - Требования к доработанному модулю")
    add_report_table(document, ["Код", "Требование", "Критерий проверки"], [
        ["R1", "Преподаватель видит только назначенные дисциплины", "Список занятий фильтруется по каталогу зачислений"],
        ["R2", "Студент видит только собственную историю", "Нет таблицы редактирования и выбора другого студента"],
        ["R3", "Администратор управляет каталогом", "Доступны пользователи, курсы, назначения и роли"],
        ["R4", "Отметки проходят предметную проверку", "Опоздание требует минут, уважительная причина - комментария"],
        ["R5", "Поддерживается контроль заполнения", "Показываются завершенные и незавершенные занятия"],
        ["R6", "Конструктор ведомостей остается работоспособным", "Маршрут и расчет ведомости сохранены"],
        ["R7", "Возможен перенос списка пользователей", "Поддерживаются текстовые представления JSON и CSV"],
    ], [1.2, 7.2, 7.3], 9.4)
    add_report_paragraph(document, "Функциональные требования дополнялись интерфейсными: элементы управления должны соответствовать стилю целевой LMS, названия действий должны быть однозначными, а состояние выбранного занятия или студента не должно приводить к перестроению общей компоновки страницы.")
    add_report_paragraph(document, "Результатом аналитического этапа стало решение сохранить исходный конструктор ведомостей отдельным модулем и подключить посещаемость рядом с ним. Такое разделение снижает риск изменения чужого кода и позволяет развивать экспериментальный функционал независимо.")

    add_page_break(document)
    add_report_heading(document, "2 Проектирование интеграции", 1)
    add_report_heading(document, "2.1 Размещение подсистемы в приложении LMS", 2)
    add_report_paragraph(document, "Подсистема добавлена в тот же Blazor WebAssembly-проект, в котором расположен конструктор ведомостей. Общими стали главный макет, меню, стили и каталог учебного контекста. Журнал и мониторинг оформлены отдельными Razor-страницами, поэтому их развитие не требует изменения алгоритма расчета ведомости.")
    add_report_figure(document, ASSETS / "attendance_integration.png", "Рисунок 1 - Размещение учета посещаемости рядом с конструктором ведомостей", 15.2)
    add_report_paragraph(document, "В Program.cs сервисы каталога и посещаемости регистрируются как singleton. В пределах открытого приложения они используют одно состояние: изменения состава курса отражаются в списках студентов занятий, а назначение пользователя определяет доступный набор дисциплин.", size=12.5, spacing=1.2)

    add_page_break(document)
    add_report_heading(document, "2.2 Модель данных посещаемости", 2)
    add_report_paragraph(document, "Центральный объект AttendanceDataModel содержит списки студентов и занятий. Занятие хранит идентификатор курса, название дисциплины и группы, тему, аудиторию, время начала, время завершения и коллекцию отметок. Отметка связывает студента со статусом, количеством минут опоздания, комментарием и временем последнего изменения.")
    add_table_caption(document, "Таблица 3 - Основные модели данных")
    add_report_table(document, ["Модель", "Основные поля", "Назначение"], [
        ["AttendanceStudentModel", "Id, FullName, RecordBookNumber", "Представление студента в журнале"],
        ["AttendanceLessonModel", "CourseId, Topic, StartsAt, CompletedAt", "Контекст учебного занятия"],
        ["AttendanceEntryModel", "StudentId, Status, LateMinutes, Comment", "Индивидуальная отметка посещаемости"],
        ["AttendanceSummaryModel", "Present, Late, Excused, Absent, Unmarked", "Расчет сводных показателей"],
        ["CourseAccessContext", "CourseCode, Role, Permissions", "Контекст доступа к дисциплине"],
    ], [4.0, 6.5, 5.2], 9.5)
    add_report_paragraph(document, "Мониторинговые модели формируются вычислением поверх занятий и отметок. Отдельное хранилище статистики не создается: при изменении статуса показатели следующего отображения рассчитываются из актуального состояния.")
    add_report_paragraph(document, "При переносе удалена зависимость от IWebHostEnvironment и файловой системы. Для отмены редактирования сервис хранит словарь подтвержденных копий отметок по идентификатору занятия. Эта структура существует только в памяти текущего экземпляра приложения.")

    add_page_break(document)
    add_report_heading(document, "2.3 Тестовая модель доступа", 2)
    add_report_paragraph(document, "Для демонстрации введены три основных типа пользователей. Администратор получает все дисциплины и административный каталог. Преподаватель видит назначенные ему курсы и может изменять соответствующие журналы. Студент видит только дисциплины, на которые он зачислен, и собственные показатели без элементов редактирования.")
    add_report_figure(document, ASSETS / "role_model.png", "Рисунок 2 - Тестовая модель ролей и контекстов доступа", 15.6)
    add_report_paragraph(document, "Права определяются данными LmsCatalogService, а не доверенным серверным центром авторизации. Поэтому модель подходит для воспроизводимой проверки интерфейса и подготовки демонстрации, но не является механизмом информационной безопасности.", size=12.5, spacing=1.2)

    add_page_break(document)
    add_report_heading(document, "2.4 Сохранение конструктора ведомостей", 2)
    add_report_paragraph(document, "Конструктор ведомостей является самостоятельной частью исходного omelea-dev-lms. Он создает временный список студентов, принимает результаты по разделам и экзамену, рассчитывает сумму, оценку, код и ECTS, а затем передает готовую ведомость в StatementService.")
    add_report_figure(document, ASSETS / "statement_constructor.png", "Рисунок 3 - Сохраненный конструктор ведомостей omelea-dev-lms", 15.5)
    add_report_paragraph(document, "Файл StatementConstructor.razor и сервис StatementService при переносе посещаемости не изменялись. Связь двух подсистем выполняется на уровне общей навигации и пользовательского контекста, а не через объединение таблиц или подмену алгоритмов ведомости. Такой вариант соответствует экспериментальному характеру работы и уменьшает область влияния изменений.", size=12.5, spacing=1.2)

    add_page_break(document)
    add_report_heading(document, "3 Реализация и доработка прототипа", 1)
    add_report_heading(document, "3.1 Перенос компонентов и сервисов", 2)
    add_report_paragraph(document, "В целевой проект добавлены модели AttendanceModels.cs, сервис AttendanceService.cs и две страницы: Attendance.razor и AttendanceMonitoring.razor. Исходный AttendanceWorkspace был переработан с учетом выполнения в WebAssembly и общего каталога LMS. Названия пространств имен, регистрация зависимостей и обращения компонентов приведены к структуре omelea-dev-lms.")
    add_report_paragraph(document, "Отдельно реализованы LmsCatalogService и LmsAccessContextService. Каталог содержит пользователей, дисциплины и назначения ролей на курсах. При изменении каталога AttendanceService синхронизирует список студентов, удаляет занятия отсутствующих курсов и создает демонстрационное занятие для нового курса. За счет этого административные операции видимы в журнале без перезапуска страницы.")
    add_table_caption(document, "Таблица 4 - Добавленные компоненты и сервисы")
    add_report_table(document, ["Файл", "Назначение"], [
        ["Pages/Attendance.razor", "Журнал преподавателя и личный режим студента"],
        ["Pages/AttendanceMonitoring.razor", "Контроль заполнения, показатели и история"],
        ["Services/Attendance/AttendanceService.cs", "Состояние, проверки, расчеты и синхронизация"],
        ["Services/Admin/LmsCatalogService.cs", "Пользователи, курсы, зачисления и обмен"],
        ["Pages/Admin.razor", "Тестовая административная панель"],
        ["Services/AuthSessionService.cs", "Демонстрационная сессия и имя пользователя"],
    ], [6.4, 9.3], 10.2)
    add_report_paragraph(document, "В MainLayout добавлены пункты перехода к журналу, мониторингу и административному разделу. Стили новых страниц размещены в общем файле lms-styles.css и используют существующие цвета и размеры элементов.")

    add_page_break(document)
    add_report_heading(document, "3.2 Журнал преподавателя", 2)
    add_report_paragraph(document, "Журнал преподавателя открывается по маршруту /attendance. В левой части отображаются доступные занятия, а в основной области - строки студентов. Преподаватель может выбрать статус, указать минуты опоздания и комментарий. Панель сводки одновременно показывает количество присутствующих, опоздавших, отсутствующих и неотмеченных.")
    add_report_figure(document, ASSETS / "teacher_attendance.png", "Рисунок 4 - Журнал посещаемости в дизайне omelea-dev-lms", 15.8)
    add_report_paragraph(document, "Дополнительные операции включают поиск по ФИО и номеру зачетной книжки, фильтрацию по статусу, сортировку и массовую отметку видимых неотмеченных студентов как присутствующих. Завершенное занятие блокируется от случайного редактирования, но преподаватель может явно возобновить работу.", size=12.5, spacing=1.2)

    add_page_break(document)
    add_report_heading(document, "3.3 Проверка и фиксация изменений", 2)
    add_report_paragraph(document, "Изменение статуса выполняется через методы AttendanceService. При выборе статуса, отличного от «опоздал», значение минут очищается. Для остальных полей сервис сохраняет введенное состояние до команды подтверждения или отмены.")
    add_table_caption(document, "Таблица 5 - Правила фиксации изменений")
    add_report_table(document, ["Условие", "Реакция интерфейса"], [
        ["Статус «Опоздал», минуты не заданы или меньше 1", "Строка считается некорректной, сохранение блокируется"],
        ["Статус «Уважительная причина», комментарий пуст", "Требуется указать пояснение"],
        ["Есть несохраненные изменения", "Показывается их количество и доступна отмена"],
        ["Нажата команда «Сохранить»", "Обновляется UpdatedAt и подтвержденная копия в памяти"],
        ["Нажата команда «Отменить»", "Строки восстанавливаются из подтвержденной копии"],
        ["Все строки заполнены корректно", "Занятие можно завершить"],
    ], [7.5, 8.2], 10.5)
    add_report_paragraph(document, "Команда сохранения в текущей версии не отправляет данные на сервер. Она фиксирует контрольную копию состояния, относительно которой определяется наличие новых изменений. Это поведение достаточно для проверки сценария редактирования, однако при полном обновлении браузера создается исходный демонстрационный набор.")
    add_report_paragraph(document, "Разделение чернового и подтвержденного состояния делает пользовательское поведение предсказуемым: можно изменить несколько строк, увидеть количество изменений, исправить ошибки и либо подтвердить, либо отменить весь набор действий.")

    add_page_break(document)
    add_report_heading(document, "3.4 Личный режим студента", 2)
    add_report_paragraph(document, "В исходном прототипе режим «Студент» включался переключателем внутри журнала и позволял выбирать любого человека. После переноса этот сценарий связан с текущим тестовым пользователем. Для учетной записи student определяется соответствующая запись каталога, после чего интерфейс показывает только ее дисциплины, историю и показатели.")
    add_report_figure(document, ASSETS / "student_attendance.png", "Рисунок 5 - Личный режим посещаемости студента", 15.8)
    add_report_paragraph(document, "У студента отсутствуют кнопки изменения статусов, сохранения, завершения или возобновления занятия. При попытке открыть преподавательский вид параметром адреса компонент принудительно выбирает личный режим. Выбор занятия обновляет не только заголовок, но и личную отметку, комментарий и сводные значения.", size=12.5, spacing=1.2)

    add_page_break(document)
    add_report_heading(document, "3.5 Административный каталог", 2)
    add_report_paragraph(document, "Тестовая административная панель доступна только пользователю admin. Она объединяет три связанных набора данных: пользователей, курсы и зачисления. Администратор может создать пользователя, указать ФИО и номер зачетной книжки, назначить признак администратора, добавить дисциплину и связать пользователя с курсом в роли преподавателя, ассистента, куратора или студента.")
    add_report_figure(document, ASSETS / "admin_main.png", "Рисунок 6 - Управление пользователями в тестовой админ-панели", 15.7)
    add_report_paragraph(document, "Для обмена списком пользователей реализованы преобразования JSON и CSV. Экспорт формирует текст в поле административной страницы, а импорт разбирает вставленное содержимое и добавляет либо обновляет пользователей по логину. Операция не создает файл на сервере и не изменяет базу данных; каталог действует в текущем сеансе приложения.", size=12.2, spacing=1.15)

    add_page_break(document)
    add_report_heading(document, "3.6 Мониторинг посещаемости", 2)
    add_report_paragraph(document, "Страница /attendance/monitoring использует те же занятия и отметки, что и журнал. Для преподавателя рассчитываются количество проведенных и незавершенных занятий, распределение статусов и процент посещаемости. Доступны выбор периода и фильтр состояния журнала.")
    add_report_figure(document, ASSETS / "monitoring_main.png", "Рисунок 7 - Мониторинг заполнения журналов и показателей", 15.8)
    add_report_paragraph(document, "Список студентов, требующих внимания, сортируется с учетом выбранного элемента: первый проблемный студент выбирается автоматически и сразу раскрывается в информационной области. Преподаватель может выбрать другого студента из списка. Команды для занятия имеют явные названия «Завершить» и «Возобновить».", size=12.5, spacing=1.2)
    add_report_paragraph(document, "Для студента эта же страница преобразуется в экран «Мои показатели»: отображаются только собственный процент посещаемости, распределение статусов и история по доступным дисциплинам. Управляющие операции и общий список студентов скрываются.", size=12.5, spacing=1.2)

    add_page_break(document)
    add_report_heading(document, "3.7 Хранение данных и демонстрационная авторизация", 2)
    add_report_paragraph(document, "AttendanceService зарегистрирован как singleton и хранит AttendanceDataModel в оперативной памяти браузера. Благодаря этому состояние остается общим при переходах между страницами журнала, мониторинга и админ-панели. Полная перезагрузка приложения создает демонстрационные данные заново.")
    add_report_paragraph(document, "Для входа предусмотрены кнопки «Админ», «Преподаватель» и «Студент». Они создают демонстрационный токен с именем пользователя и сохраняют его в localStorage. AuthSessionService извлекает имя из полезной нагрузки токена, а каталог определяет доступные курсы и интерфейсные операции.")
    add_table_caption(document, "Таблица 6 - Реализация сессии и хранения данных")
    add_report_table(document, ["Механизм", "Реализовано", "Ограничение"], [
        ["Сессия", "Токен хранится в localStorage", "Подпись и срок токена не проверяются"],
        ["Роли", "Права вычисляются по каталогу и курсу", "Проверка выполняется на стороне клиента"],
        ["Посещаемость", "Общее состояние страниц в singleton-сервисе", "Нет постоянного хранилища"],
        ["Импорт/экспорт", "Разбор и формирование JSON/CSV", "Текст обрабатывается в браузере"],
        ["Вход МИФИ", "Сформирован маршрут к внешнему CAS", "Требуется отдельно настроенный auth-service"],
    ], [3.2, 6.1, 6.4], 9.7)
    add_report_paragraph(document, "Данное решение намеренно отделено от полноценной авторизации исходного dev-lms, в которой присутствуют PostgreSQL, JWT Bearer и защищенные контроллеры. Интеграция с серверным контуром относится к следующему этапу.")

    add_page_break(document)
    add_report_heading(document, "4 Апробация и результаты", 1)
    add_report_heading(document, "4.1 Проверка пользовательских сценариев", 2)
    add_report_paragraph(document, "Проверка выполнялась путем сборки Blazor-проекта и последовательного прохождения основных маршрутов для трех демонстрационных пользователей. Особое внимание уделялось связи каталога курсов с доступными занятиями и отсутствию преподавательских операций в студенческом режиме.")
    add_table_caption(document, "Таблица 7 - Результаты проверки пользовательских сценариев")
    add_report_table(document, ["Сценарий", "Ожидаемый результат", "Результат"], [
        ["Вход teacher и открытие журнала", "Показаны только назначенные дисциплины", "Выполнено"],
        ["Редактирование опоздания без минут", "Сохранение блокируется", "Выполнено"],
        ["Сохранение и отмена изменений", "Состояние фиксируется и восстанавливается", "Выполнено"],
        ["Вход student", "Показаны личная история и показатели", "Выполнено"],
        ["Открытие режима teacher студентом", "Редактирование недоступно", "Выполнено"],
        ["Добавление пользователя и зачисления", "Студент появляется в занятиях курса", "Выполнено"],
        ["Импорт JSON/CSV", "Каталог добавляет или обновляет пользователей", "Выполнено"],
        ["Открытие конструктора ведомостей", "Исходный сценарий создания ведомости сохранен", "Выполнено"],
    ], [5.2, 8.0, 2.5], 9.2)
    add_report_paragraph(document, "Проект собирается без ошибок компиляции. Результаты ручной проверки подтверждают целостность демонстрационного сценария, но не заменяют автоматизированные модульные и сквозные тесты.")

    add_page_break(document)
    add_report_heading(document, "4.2 Проверка разграничения интерфейса", 2)
    add_report_paragraph(document, "Разграничение проверялось сравнением экранов одного приложения при входе разными пользователями. Для преподавателя журнал содержит таблицу группы и операции изменения. Для студента те же данные преобразуются в персональную историю. Администратор получает все дисциплины и отдельный каталог.")
    add_report_figure(document, ASSETS / "student_monitoring.png", "Рисунок 8 - Персональный мониторинг студента", 15.8)
    add_table_caption(document, "Таблица 8 - Доступные операции тестовых пользователей")
    add_report_table(document, ["Пользователь", "Дисциплины", "Изменение журнала", "Админ-панель"], [
        ["admin", "Все", "Разрешено", "Разрешена"],
        ["teacher", "Назначенные", "Разрешено для назначенных", "Недоступна"],
        ["student", "Назначенные", "Недоступно", "Недоступна"],
    ], [3.0, 4.0, 5.2, 3.5], 9.5)

    add_page_break(document)
    add_report_heading(document, "4.3 Ограничения и дальнейшее развитие", 2)
    add_report_paragraph(document, "Полученный результат является рабочим интерфейсным прототипом. Он демонстрирует перенос предметной логики и ее согласование с существующим приложением, но не содержит серверного слоя хранения посещаемости. При обновлении страницы журнал и административный каталог возвращаются к начальному набору данных.")
    add_report_paragraph(document, "Тестовые роли ограничивают элементы интерфейса, однако не могут считаться защитой от изменения данных. Пользователь способен подменить содержимое localStorage, а вся бизнес-логика выполняется в браузере. Для эксплуатации операции необходимо перенести в серверные контроллеры и повторно проверять права на сервере.")
    add_report_bullets(document, [
        "создать таблицы курсов, занятий, зачислений и отметок посещаемости в PostgreSQL;",
        "добавить серверный API чтения и изменения журналов;",
        "подключить автономный LMS-интерфейс к JWT-авторизации dev-lms;",
        "перенести проверки ролей и дисциплин на серверную сторону;",
        "добавить журнал аудита и обработку одновременного редактирования;",
        "покрыть сервисы модульными тестами, а ключевые маршруты - браузерными тестами.",
    ])
    add_report_paragraph(document, "Несмотря на ограничения, прототип позволяет согласовать структуру интерфейса и пользовательские сценарии до разработки серверной части. Это снижает риск последующей переделки API и модели данных.")

    add_page_break(document)
    add_report_heading(document, "Заключение", 1)
    add_report_paragraph(document, "В ходе летней практики выполнен перенос рабочего прототипа учета посещаемости из проекта uir-main в Blazor WebAssembly-приложение omelea-dev-lms. Перенос включал адаптацию моделей, сервиса состояния, журнала преподавателя и страницы мониторинга к структуре и визуальному стилю целевой LMS.")
    add_report_paragraph(document, "Прототип дополнен общим каталогом пользователей, дисциплин и зачислений. На его основе реализована тестовая модель доступа: преподаватель работает только с назначенными курсами, студент видит собственные дисциплины и показатели без возможности редактирования, администратор управляет каталогом и назначениями.")
    add_report_paragraph(document, "В журнале доступны статусы посещаемости, минуты опоздания, комментарии, фильтрация, сортировка, массовая отметка, проверка значений, сохранение и отмена изменений текущего сеанса, завершение и возобновление занятия. Мониторинг формирует показатели заполнения и посещаемости по тем же данным.")
    add_report_paragraph(document, "Исходный конструктор ведомостей сохранен самостоятельным модулем. Его основные файлы и алгоритм расчета результатов не изменялись, что позволило встроить экспериментальный функционал без существенного вмешательства в существующую часть проекта.")
    add_report_paragraph(document, "Цель практики достигнута: получен единый демонстрационный интерфейс LMS, в котором можно проверить работу конструктора ведомостей, журнала посещаемости, мониторинга и разных пользовательских режимов. Следующим этапом является замена памяти браузера постоянным серверным хранилищем и подключение полноценной авторизации.")

    add_page_break(document)
    add_report_heading(document, "Список использованных источников", 1)
    for index, source in enumerate(SOURCES, 1):
        add_report_paragraph(document, f"{index}. {source}", indent=False, size=12, after=4, spacing=1.05)

    add_page_break(document)
    add_report_heading(document, "Приложение А", 1)
    add_report_paragraph(document, "Журнал преподавателя", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=14, after=8)
    add_report_figure(document, SCREENS / "02-teacher-attendance.png", "Рисунок А.1 - Полная страница журнала преподавателя", 16.0)
    add_report_paragraph(document, "На странице одновременно представлены список занятий, сводка статусов и строки студентов. Активные элементы редактирования доступны преподавателю назначенного курса.", size=12.5, spacing=1.2)

    add_page_break(document)
    add_report_heading(document, "Приложение Б", 1)
    add_report_paragraph(document, "Личный режим студента", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=14, after=6)
    add_report_figure(document, SCREENS / "03-student-attendance.png", "Рисунок Б.1 - Личная посещаемость студента", 15.5)
    add_report_paragraph(document, "В личном режиме отсутствуют операции изменения журнала и выбор другого студента.", size=12.2, spacing=1.1)

    add_page_break(document)
    add_report_heading(document, "Продолжение приложения Б", 1)
    add_report_paragraph(document, "Тестовая административная панель", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=14, after=6)
    add_report_figure(document, SCREENS / "05-admin-catalog.png", "Рисунок Б.2 - Каталог пользователей и курсов", 15.5)
    add_report_paragraph(document, "Административный экран предназначен для проверки связи пользователей, курсов и зачислений с журналом посещаемости.", size=12.2, spacing=1.1)


def build_report() -> Path:
    OUTPUT.mkdir(parents=True, exist_ok=True)
    document = Document()
    configure_report(document)
    document.core_properties.title = "Отчет по летней практике: перенос и доработка учета посещаемости в LMS"
    document.core_properties.author = "Ефименко Н.П."
    document.core_properties.subject = "Летняя практика, Blazor, LMS, учет посещаемости"
    add_title_page(document)
    add_assignment_pages(document)
    add_abstract(document)
    add_contents(document)
    add_report_body(document)
    document.save(REPORT_PATH)
    return REPORT_PATH


def configure_diary(document: Document) -> None:
    section = document.sections[0]
    section.page_width = Cm(21)
    section.page_height = Cm(29.7)
    section.left_margin = Cm(2)
    section.right_margin = Cm(1.5)
    section.top_margin = Cm(1.5)
    section.bottom_margin = Cm(1.5)
    normal = document.styles["Normal"]
    normal.font.name = "Times New Roman"
    normal.font.size = Pt(11)
    normal._element.rPr.rFonts.set(qn("w:eastAsia"), "Times New Roman")


def diary_center(document: Document, text: str, size: float = 12, bold: bool = False, before: float = 0, after: float = 0):
    paragraph = document.add_paragraph()
    paragraph.alignment = WD_ALIGN_PARAGRAPH.CENTER
    paragraph.paragraph_format.space_before = Pt(before)
    paragraph.paragraph_format.space_after = Pt(after)
    paragraph.paragraph_format.line_spacing = 1.0
    run = paragraph.add_run(text)
    font_run(run, size, bold)
    return paragraph


def diary_heading(document: Document, text: str) -> None:
    paragraph = document.add_paragraph()
    paragraph.paragraph_format.space_before = Pt(3)
    paragraph.paragraph_format.space_after = Pt(3)
    paragraph.paragraph_format.keep_with_next = True
    font_run(paragraph.add_run(text), 12, True)


def diary_field(document: Document, number: int, label: str, value: str = "") -> None:
    paragraph = document.add_paragraph()
    paragraph.paragraph_format.space_after = Pt(1)
    paragraph.paragraph_format.line_spacing = 1.0
    font_run(paragraph.add_run(f"{number}. {label} "), 10.5)
    font_run(paragraph.add_run(value if value else "____________________________________________"), 10.5)


def add_diary_task_table(document: Document) -> None:
    add_report_table(document, ["№", "Содержание работы", "Форма отчетности", "Срок", "Отметка"], [[str(i), task[0], task[1], task[2], ""] for i, task in enumerate(REPORT_TASKS, 1)], [0.7, 7.2, 3.1, 2.0, 3.5], 7.4)


def build_diary() -> Path:
    document = Document()
    configure_diary(document)
    document.core_properties.title = "Дневник летней практики Ефименко Н.П. v2"
    document.core_properties.author = "Ефименко Н.П."

    diary_center(document, "Министерство науки и высшего образования Российской Федерации", 11, False, 0, 4)
    diary_center(document, "федеральное государственное автономное образовательное учреждение высшего образования", 10)
    diary_center(document, "«Национальный исследовательский ядерный университет «МИФИ»", 12, True, 0, 55)
    diary_center(document, "ДНЕВНИК", 18, True, 0, 8)
    diary_center(document, "летней практики", 15, True, 0, 20)
    diary_center(document, "студента группы Б23-504", 13)
    diary_center(document, "Ефименко Н.П.", 13, True)
    diary_center(document, "(фамилия, инициалы)", 9, False, 0, 90)
    diary_center(document, "Москва 2026 г.", 12)

    add_page_break(document)
    diary_heading(document, "1. Общие сведения")
    diary_field(document, 1, "Фамилия", "Ефименко")
    diary_field(document, 2, "Имя, отчество", "Николай Павлович")
    diary_field(document, 3, "Группа", "Б23-504")
    diary_field(document, 4, "Направление подготовки", "09.03.04 «Программная инженерия»")
    diary_field(document, 5, "Организация", "НИЯУ МИФИ")
    diary_field(document, 6, "Руководитель практики", "Рословцев Владимир Владимирович")
    diary_field(document, 7, "Сроки практики", "с 26.06.2026 по 09.07.2026")
    diary_heading(document, "2. Индивидуальное задание студента")
    add_diary_task_table(document)

    add_page_break(document)
    diary_heading(document, "3. Рабочие записи студента")
    add_report_table(document, ["Дата", "Содержание выполненной работы", "Результат"], [
        ["26.06.2026", "Изучены структуры uir-main и omelea-dev-lms, выделены страницы и сервисы посещаемости.", "План переноса"],
        ["27.06.2026", "Проверены маршруты, стили и конструктор ведомостей целевого приложения.", "Границы изменений"],
        ["29.06.2026", "Перенесены модели посещаемости и базовая логика сервиса состояния.", "Модели и сервис"],
        ["30.06.2026", "Адаптирован журнал преподавателя к макету и стилям LMS.", "Страница журнала"],
        ["02.07.2026", "Подключены дисциплины и студенты из тестового каталога LMS.", "Фильтрация по курсам"],
        ["03.07.2026", "Доработаны статусы, опоздания, комментарии, сохранение и отмена изменений.", "Рабочий сценарий"],
        ["05.07.2026", "Реализован личный режим студента без операций редактирования.", "Ограничение интерфейса"],
        ["06.07.2026", "Добавлены демонстрационные входы и контексты администратора, преподавателя и студента.", "Тестовая модель доступа"],
        ["07.07.2026", "Разработана панель пользователей, курсов и зачислений.", "Админ-каталог"],
        ["08.07.2026", "Добавлены импорт и экспорт пользователей в JSON и CSV, проверен мониторинг.", "Обмен и показатели"],
        ["09.07.2026", "Проверены основные маршруты и конструктор ведомостей, подготовлены материалы.", "Отчет и презентация"],
    ], [2.3, 10.1, 4.1], 7.8)

    add_page_break(document)
    diary_heading(document, "4. Заключение студента по итогам практики")
    add_report_paragraph(document, "В ходе практики рабочий прототип учета посещаемости был перенесен из проекта uir-main в приложение omelea-dev-lms. Модели, журнал и мониторинг адаптированы к структуре Blazor WebAssembly и визуальному оформлению целевой LMS. Добавлены тестовый каталог пользователей и дисциплин, зачисления на курсы, демонстрационные режимы администратора, преподавателя и студента, личный просмотр показателей и административные операции импорта и экспорта пользователей. Исходный конструктор ведомостей сохранен самостоятельным модулем. Выполнена проверка основных пользовательских сценариев. Текущая версия использует состояние в памяти браузера; серверное хранение и защищенная авторизация определены как направление дальнейшей работы.", size=11, spacing=1.25)
    diary_center(document, "Подпись студента ____________________    «_____» ____________ 2026 г.", 10.5, False, 12, 18)
    diary_heading(document, "5. Производственная характеристика студента")
    for _ in range(6):
        add_report_paragraph(document, "________________________________________________________________________________", indent=False, size=9, spacing=0.8)
    add_report_paragraph(document, "(Указываются качество выполненной работы, теоретическая и практическая подготовка, трудовая дисциплина и итоговая оценка.)", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=9, spacing=1.0)
    diary_center(document, "Руководитель практики ____________________ / Рословцев В.В. /", 10.5, False, 16, 10)
    diary_heading(document, "6. Заключение комиссии по результатам защиты")
    for _ in range(4):
        add_report_paragraph(document, "________________________________________________________________________________", indent=False, size=9, spacing=0.8)
    diary_center(document, "Председатель комиссии ____________________", 10.5, False, 8)

    document.save(DIARY_PATH)
    return DIARY_PATH


def ppt_rgb(value: str) -> RGBColor:
    return RGBColor.from_string(value)


def ppt_set_background(slide, color: str = WHITE) -> None:
    fill = slide.background.fill
    fill.solid()
    fill.fore_color.rgb = ppt_rgb(color)


def ppt_add_text(
    slide,
    x: float,
    y: float,
    w: float,
    h: float,
    text: str,
    size: int = 20,
    color: str = INK,
    bold: bool = False,
    align=PP_ALIGN.LEFT,
    valign=MSO_ANCHOR.MIDDLE,
    margin: float = 0.08,
):
    box = slide.shapes.add_textbox(Inches(x), Inches(y), Inches(w), Inches(h))
    frame = box.text_frame
    frame.clear()
    frame.margin_left = Inches(margin)
    frame.margin_right = Inches(margin)
    frame.margin_top = Inches(margin)
    frame.margin_bottom = Inches(margin)
    frame.vertical_anchor = valign
    frame.word_wrap = True
    paragraph = frame.paragraphs[0]
    paragraph.alignment = align
    paragraph.space_after = Pt(0)
    run = paragraph.add_run()
    run.text = text
    run.font.name = "Aptos"
    run.font.size = PptPt(size)
    run.font.bold = bold
    run.font.color.rgb = ppt_rgb(color)
    return box


def ppt_add_rect(slide, x: float, y: float, w: float, h: float, fill: str, line: str | None = None):
    shape = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(x), Inches(y), Inches(w), Inches(h))
    shape.fill.solid()
    shape.fill.fore_color.rgb = ppt_rgb(fill)
    shape.line.color.rgb = ppt_rgb(line or fill)
    return shape


def ppt_add_card(slide, x: float, y: float, w: float, h: float, title: str, body: str, accent: str = BLUE, title_size: int = 18, body_size: int = 15):
    ppt_add_rect(slide, x, y, w, h, LIGHT, MID)
    ppt_add_rect(slide, x, y, 0.08, h, accent, accent)
    ppt_add_text(slide, x + 0.22, y + 0.12, w - 0.35, 0.42, title, title_size, accent, True)
    ppt_add_text(slide, x + 0.22, y + 0.58, w - 0.35, h - 0.7, body, body_size, INK, False, valign=MSO_ANCHOR.TOP)


def ppt_add_picture_cover(slide, path: Path, x: float, y: float, w: float, h: float, border: bool = True):
    with Image.open(path) as image:
        image_ratio = image.width / image.height
    box_ratio = w / h
    picture = slide.shapes.add_picture(str(path), Inches(x), Inches(y), width=Inches(w), height=Inches(h))
    if image_ratio > box_ratio:
        visible_ratio = box_ratio / image_ratio
        crop = (1 - visible_ratio) / 2
        picture.crop_left = crop
        picture.crop_right = crop
    elif image_ratio < box_ratio:
        visible_ratio = image_ratio / box_ratio
        crop = (1 - visible_ratio) / 2
        picture.crop_top = crop
        picture.crop_bottom = crop
    if border:
        picture.line.color.rgb = ppt_rgb(MID)
        picture.line.width = Pt(1)
    return picture


def ppt_add_picture_contain(slide, path: Path, x: float, y: float, w: float, h: float, border: bool = True):
    with Image.open(path) as image:
        image_ratio = image.width / image.height
    box_ratio = w / h
    if image_ratio >= box_ratio:
        picture_width = w
        picture_height = w / image_ratio
        picture_x = x
        picture_y = y + (h - picture_height) / 2
    else:
        picture_height = h
        picture_width = h * image_ratio
        picture_x = x + (w - picture_width) / 2
        picture_y = y
    picture = slide.shapes.add_picture(
        str(path),
        Inches(picture_x),
        Inches(picture_y),
        width=Inches(picture_width),
        height=Inches(picture_height),
    )
    if border:
        picture.line.color.rgb = ppt_rgb(MID)
        picture.line.width = Pt(1)
    return picture


def ppt_add_header(slide, title: str, number: int) -> None:
    ppt_add_rect(slide, 0, 0, 13.333, 0.1, BLUE, BLUE)
    ppt_add_text(slide, 0.55, 0.25, 11.6, 0.55, title, 27, NAVY, True)
    ppt_add_text(slide, 12.2, 0.25, 0.55, 0.5, str(number), 15, GRAY, True, PP_ALIGN.RIGHT)
    ppt_add_rect(slide, 0.55, 7.12, 12.2, 0.02, MID, MID)
    ppt_add_text(slide, 0.55, 7.15, 8.0, 0.22, "Ефименко Н.П. · Б23-504 · летняя практика", 9, GRAY)


def ppt_bullet_text(items: list[str]) -> str:
    return "\n".join(f"• {item}" for item in items)


def build_presentation() -> Path:
    presentation = Presentation()
    presentation.slide_width = Inches(13.333)
    presentation.slide_height = Inches(7.5)
    blank = presentation.slide_layouts[6]

    slide = presentation.slides.add_slide(blank)
    ppt_set_background(slide, NAVY)
    ppt_add_rect(slide, 0, 0, 5.3, 7.5, NAVY, NAVY)
    ppt_add_picture_cover(slide, SCREENS / "02-teacher-attendance.png", 5.3, 0, 8.033, 7.5, False)
    ppt_add_rect(slide, 0.55, 0.48, 0.12, 0.9, ORANGE, ORANGE)
    ppt_add_text(slide, 0.85, 0.46, 3.9, 0.42, "НИЯУ МИФИ · КАФЕДРА №22", 13, WHITE, True)
    ppt_add_text(slide, 0.55, 1.75, 4.2, 2.2, "Перенос и доработка\nпрототипа учета\nпосещаемости в LMS", 30, WHITE, True, valign=MSO_ANCHOR.TOP)
    ppt_add_text(slide, 0.55, 4.35, 4.0, 0.45, "Отчёт по летней практике", 18, "C9D4DF", False)
    ppt_add_text(slide, 0.55, 5.45, 4.1, 0.9, "Ефименко Н.П.\nБ23-504", 19, WHITE, True, valign=MSO_ANCHOR.TOP)
    ppt_add_text(slide, 0.55, 6.75, 4.1, 0.3, "Руководитель: Рословцев В.В. · Москва, 2026", 10, "C9D4DF")

    slide = presentation.slides.add_slide(blank)
    ppt_set_background(slide)
    ppt_add_header(slide, "Исходная ситуация", 2)
    ppt_add_card(slide, 0.55, 1.1, 3.75, 4.9, "uir-main", "Рабочий прототип:\n\n• журнал занятия\n• статусы и комментарии\n• история студента\n• мониторинг\n• локальное состояние", PURPLE, 21, 18)
    ppt_add_card(slide, 4.78, 1.1, 3.75, 4.9, "omelea-dev-lms", "Целевой проект:\n\n• готовый дизайн LMS\n• навигация и страницы\n• конструктор ведомостей\n• семантические формы\n• отдельный auth-service", BLUE, 21, 18)
    ppt_add_card(slide, 9.0, 1.1, 3.75, 4.9, "Задача практики", "Перенести предметную логику посещаемости, оформить её как часть LMS и дополнить контекстом пользователей, дисциплин и тестовых ролей.", ORANGE, 21, 18)

    slide = presentation.slides.add_slide(blank)
    ppt_set_background(slide)
    ppt_add_header(slide, "Цель и задачи", 3)
    ppt_add_rect(slide, 0.65, 1.05, 12.0, 1.0, NAVY, NAVY)
    ppt_add_text(slide, 0.95, 1.13, 11.4, 0.82, "Цель: встроить рабочий прототип посещаемости в omelea-dev-lms\nбез переработки конструктора ведомостей", 19, WHITE, True, PP_ALIGN.CENTER)
    cards = [
        ("1", "Проанализировать два проекта", BLUE),
        ("2", "Перенести модели и сервис", PURPLE),
        ("3", "Адаптировать журнал и мониторинг", GREEN),
        ("4", "Добавить роли и каталог", ORANGE),
        ("5", "Проверить сценарии", RED),
    ]
    for index, (number, text, color) in enumerate(cards):
        x = 0.65 + index * 2.43
        ppt_add_rect(slide, x, 2.65, 2.05, 2.55, LIGHT, MID)
        ppt_add_rect(slide, x + 0.15, 2.85, 0.55, 0.55, color, color)
        ppt_add_text(slide, x + 0.15, 2.85, 0.55, 0.55, number, 19, WHITE, True, PP_ALIGN.CENTER)
        ppt_add_text(slide, x + 0.16, 3.55, 1.73, 1.35, text, 15, INK, True, PP_ALIGN.CENTER)

    slide = presentation.slides.add_slide(blank)
    ppt_set_background(slide)
    ppt_add_header(slide, "Граница изменений", 4)
    ppt_add_picture_contain(slide, ASSETS / "statement_constructor.png", 0.55, 1.1, 7.3, 4.8)
    ppt_add_card(slide, 8.25, 1.1, 4.5, 2.1, "Конструктор сохранён", "StatementConstructor и StatementService не изменялись. Посещаемость добавлена рядом, а не встроена в расчёт ведомости.", GREEN, 20, 17)
    ppt_add_card(slide, 8.25, 3.55, 4.5, 2.35, "Что связано", "Общий макет и меню\nЕдиный стиль интерфейса\nКаталог пользователей и курсов\nПереходы между разделами", BLUE, 20, 17)

    slide = presentation.slides.add_slide(blank)
    ppt_set_background(slide)
    ppt_add_header(slide, "Архитектура переноса", 5)
    ppt_add_picture_contain(slide, ASSETS / "attendance_integration.png", 0.65, 1.0, 12.0, 5.8)

    slide = presentation.slides.add_slide(blank)
    ppt_set_background(slide)
    ppt_add_header(slide, "Модель данных и состояние", 6)
    ppt_add_card(slide, 0.55, 1.05, 3.75, 4.95, "Студент", "Id\nFullName\nRecordBookNumber", GREEN, 21, 20)
    ppt_add_card(slide, 4.78, 1.05, 3.75, 4.95, "Занятие", "CourseId\nTopic · Room\nStartsAt · CompletedAt\nEntries[]", BLUE, 21, 20)
    ppt_add_card(slide, 9.0, 1.05, 3.75, 4.95, "Отметка", "StudentId\nStatus\nLateMinutes\nComment\nUpdatedAt", PURPLE, 21, 20)
    ppt_add_text(slide, 0.8, 6.18, 11.7, 0.65, "Состояние хранится в памяти браузера: переходы между страницами сохраняют данные,\nF5 возвращает демонстрационный набор.", 13, RED, True, PP_ALIGN.CENTER)

    slide = presentation.slides.add_slide(blank)
    ppt_set_background(slide)
    ppt_add_header(slide, "Журнал преподавателя", 7)
    ppt_add_picture_contain(slide, ASSETS / "teacher_attendance.png", 0.55, 1.05, 8.25, 5.65)
    ppt_add_card(slide, 9.15, 1.05, 3.6, 5.65, "Реализовано", ppt_bullet_text(["выбор занятия", "5 статусов", "минуты опоздания", "комментарий", "поиск и фильтр", "массовая отметка", "завершение журнала"]), BLUE, 20, 16)

    slide = presentation.slides.add_slide(blank)
    ppt_set_background(slide)
    ppt_add_header(slide, "Проверка и фиксация", 8)
    ppt_add_card(slide, 0.6, 1.1, 3.7, 4.9, "Валидация", "Опоздание требует положительного числа минут.\n\nУважительная причина требует комментария.\n\nОшибка строки блокирует сохранение.", RED, 20, 18)
    ppt_add_card(slide, 4.82, 1.1, 3.7, 4.9, "Сохранение", "Изменённые строки получают UpdatedAt.\n\nПодтверждённая копия хранится в AttendanceService.\n\nПоказывается число изменений.", GREEN, 20, 18)
    ppt_add_card(slide, 9.04, 1.1, 3.7, 4.9, "Отмена", "До подтверждения журнал можно вернуть к предыдущему состоянию.\n\nЗавершённое занятие блокируется до команды «Возобновить».", PURPLE, 20, 18)

    slide = presentation.slides.add_slide(blank)
    ppt_set_background(slide)
    ppt_add_header(slide, "Личный режим студента", 9)
    ppt_add_picture_contain(slide, ASSETS / "student_attendance.png", 0.55, 1.05, 8.25, 5.65)
    ppt_add_card(slide, 9.15, 1.05, 3.6, 5.65, "Изменение прототипа", "Студент связан с тестовой учётной записью.\n\nПоказаны только его дисциплины, отметка, комментарий, история и показатели.\n\nРедактирование и выбор другого студента отсутствуют.", GREEN, 20, 17)

    slide = presentation.slides.add_slide(blank)
    ppt_set_background(slide)
    ppt_add_header(slide, "Тестовая ролевая модель", 10)
    ppt_add_picture_contain(slide, ASSETS / "role_model.png", 0.65, 1.0, 12.0, 5.8)

    slide = presentation.slides.add_slide(blank)
    ppt_set_background(slide)
    ppt_add_header(slide, "Административный каталог", 11)
    ppt_add_picture_contain(slide, ASSETS / "admin_main.png", 0.55, 1.05, 8.2, 5.65)
    ppt_add_card(slide, 9.1, 1.05, 3.65, 5.65, "Администратор", ppt_bullet_text(["добавляет пользователей", "создаёт курсы", "назначает роли", "формирует зачисления", "импортирует JSON/CSV", "экспортирует JSON/CSV"]), ORANGE, 20, 16)

    slide = presentation.slides.add_slide(blank)
    ppt_set_background(slide)
    ppt_add_header(slide, "Мониторинг", 12)
    ppt_add_picture_contain(slide, ASSETS / "monitoring_main.png", 0.55, 1.05, 8.25, 5.65)
    ppt_add_card(slide, 9.15, 1.05, 3.6, 5.65, "Общие данные", "Контроль заполнения журналов\n\nФильтр периода и состояния\n\nРаспределение статусов\n\nСтуденты, требующие внимания\n\nАвтовыбор первого студента", PURPLE, 20, 17)

    slide = presentation.slides.add_slide(blank)
    ppt_set_background(slide)
    ppt_add_header(slide, "Проверка результата", 13)
    tests = [
        ("teacher", "назначенные курсы и редактирование", GREEN),
        ("student", "личные показатели без редактирования", BLUE),
        ("admin", "каталог и все дисциплины", ORANGE),
        ("валидация", "блокировка некорректной строки", RED),
        ("ведомости", "исходный конструктор работает", PURPLE),
    ]
    for index, (title, body, color) in enumerate(tests):
        y = 1.0 + index * 1.08
        ppt_add_rect(slide, 0.75, y, 1.95, 0.78, color, color)
        ppt_add_text(slide, 0.85, y + 0.05, 1.75, 0.66, title, 17, WHITE, True, PP_ALIGN.CENTER)
        ppt_add_rect(slide, 2.7, y, 9.85, 0.78, LIGHT, MID)
        ppt_add_text(slide, 3.0, y + 0.05, 9.2, 0.66, body + " — выполнено", 18, INK, False)
    ppt_add_text(slide, 0.75, 6.45, 11.8, 0.4, "Сборка проекта: 0 ошибок. Проверка выполнена вручную по основным маршрутам.", 16, GREEN, True, PP_ALIGN.CENTER)

    slide = presentation.slides.add_slide(blank)
    ppt_set_background(slide)
    ppt_add_header(slide, "Ограничения и следующий этап", 14)
    ppt_add_card(slide, 0.6, 1.05, 5.75, 5.3, "Сейчас", ppt_bullet_text(["данные в памяти браузера", "демонстрационные токены", "роли проверяются на клиенте", "нет серверного аудита", "нет автоматических E2E-тестов"]), RED, 22, 18)
    ppt_add_card(slide, 6.95, 1.05, 5.75, 5.3, "Развитие", ppt_bullet_text(["PostgreSQL для журналов", "серверный API", "JWT-авторизация dev-lms", "серверная проверка ролей", "аудит и конфликты версий"]), GREEN, 22, 18)

    slide = presentation.slides.add_slide(blank)
    ppt_set_background(slide)
    ppt_add_header(slide, "Результаты практики", 15)
    ppt_add_rect(slide, 0.7, 1.05, 11.95, 1.0, NAVY, NAVY)
    ppt_add_text(slide, 1.0, 1.18, 11.35, 0.7, "Прототип посещаемости стал частью интерфейса omelea-dev-lms", 24, WHITE, True, PP_ALIGN.CENTER)
    results = [
        ("Перенос", "Модели, сервис, журнал и мониторинг", BLUE),
        ("Доработка", "Курсы, роли, студент и админ-панель", PURPLE),
        ("Совместимость", "Конструктор ведомостей сохранён", GREEN),
        ("Апробация", "Проверены три пользовательских режима", ORANGE),
    ]
    for index, (title, body, color) in enumerate(results):
        x = 0.7 + (index % 2) * 6.15
        y = 2.55 + (index // 2) * 1.75
        ppt_add_card(slide, x, y, 5.8, 1.35, title, body, color, 19, 16)

    slide = presentation.slides.add_slide(blank)
    ppt_set_background(slide, NAVY)
    ppt_add_text(slide, 0.7, 1.5, 11.95, 1.2, "Спасибо за внимание", 38, WHITE, True, PP_ALIGN.CENTER)
    ppt_add_rect(slide, 5.35, 3.05, 2.65, 0.08, ORANGE, ORANGE)
    ppt_add_text(slide, 0.7, 3.55, 11.95, 0.55, "Ефименко Николай Павлович", 22, WHITE, True, PP_ALIGN.CENTER)
    ppt_add_text(slide, 0.7, 4.2, 11.95, 0.45, "Б23-504 · 09.03.04 «Программная инженерия»", 17, "C9D4DF", False, PP_ALIGN.CENTER)
    ppt_add_text(slide, 0.7, 6.45, 11.95, 0.35, "Перенос и доработка прототипа учета посещаемости в LMS", 14, "C9D4DF", False, PP_ALIGN.CENTER)

    presentation.core_properties.title = "Перенос и доработка прототипа учета посещаемости в LMS"
    presentation.core_properties.author = "Ефименко Н.П."
    presentation.save(PRESENTATION_PATH)
    return PRESENTATION_PATH


def write_memo() -> Path:
    text = """# Памятка к материалам v2 — Ефименко Н.П.

Материалы описывают только перенос прототипа посещаемости из `uir-main` в `omelea-dev-lms` и выполненные в целевом проекте доработки.

## Где формулировки немного усилены

- «Апробация» означает ручное прохождение основных маршрутов и успешную сборку, а не полный набор автоматизированных тестов.
- «Тестовая ролевая модель» реально переключает интерфейс и доступные дисциплины, но не является защищенной серверной авторизацией.
- «Сохранение журнала» фиксирует подтвержденную копию в памяти текущего сеанса; после полной перезагрузки демонстрационные данные восстанавливаются.
- Импорт и экспорт JSON/CSV реализованы как преобразование текста в административной панели; файлы на сервере не создаются.
- «Интеграция» означает размещение модулей в одном Blazor-приложении, общую навигацию, дизайн и каталог. Серверный auth-service и база PostgreSQL к посещаемости не подключены.

## Что подтверждается кодом

- журнал преподавателя, статусы, опоздания, комментарии, фильтры, сортировка, массовая отметка, отмена и завершение занятия;
- личный режим студента без редактирования и без выбора другого студента;
- мониторинг журналов и показатели студента;
- тестовые пользователи `admin`, `teacher`, `student` и фильтрация дисциплин;
- админ-панель пользователей, курсов, зачислений и ролей;
- импорт/экспорт пользователей в JSON и CSV;
- исходный `StatementConstructor.razor` и `StatementService.cs` не изменялись при переносе посещаемости.
"""
    MEMO_PATH.write_text(text, encoding="utf-8")
    return MEMO_PATH


def main() -> None:
    OUTPUT.mkdir(parents=True, exist_ok=True)
    paths = [build_report(), build_diary(), build_presentation(), write_memo()]
    for path in paths:
        print(path)


if __name__ == "__main__":
    main()
